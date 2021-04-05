using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Collections.Generic;
using cAlgo.Indicators;

// forex simple grid, open buy and sell, take profit directly when profit is hit.
// no stop loss, 
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class forex_simpleGrid : Robot
    {
        #region parameters

        [Parameter("grid size", DefaultValue = 50, MinValue = 1)]
        public int PipRadius { get; set; }

        [Parameter("Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Volume { get; set; }

        [Parameter("max spread", DefaultValue = 3, MinValue = 0, Group = "default")]
        public double MaxaSpread { get; set; }
        #endregion

        Dictionary<int, Position> openedPositions = new Dictionary<int, Position>();
        int stopLoss;

        protected override void OnStart()
        {
            stopLoss = PipRadius * 6000;
            Positions.Closed += onClosePosition;
            Positions.Opened += onOpenPosition;
            openTrade(TradeType.Sell, Volume, PipRadius);
            openTrade(TradeType.Buy, Volume, PipRadius);

            for (int i = 1; i < 30; i++)
            {
                setPendingOrder(TradeType.Sell, Volume, PipRadius, (Symbol.Ask - (Symbol.PipSize * PipRadius * i)));
                setPendingOrder(TradeType.Sell, Volume, PipRadius, (Symbol.Ask + (Symbol.PipSize * PipRadius * i)));
                setPendingOrder(TradeType.Buy, Volume, PipRadius, (Symbol.Ask - (Symbol.PipSize * PipRadius * i)));
                setPendingOrder(TradeType.Buy, Volume, PipRadius, (Symbol.Ask + (Symbol.PipSize * PipRadius * i)));
            }

        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
            foreach (Position p in Positions)
            {
                p.Close();
            }
        }

        protected void onClosePosition(PositionClosedEventArgs pos)
        {
            setPendingOrder(pos.Position.TradeType, Volume, PipRadius, pos.Position.EntryPrice);
        }

        protected void onOpenPosition(PositionOpenedEventArgs pos)
        {

        }

        protected void openTrade(TradeType tradeType, int volume, double range)
        {
            TradeResult trade1 = ExecuteMarketOrder(tradeType, Symbol.Name, volume, "", stopLoss, range);
            //setPendingOrder(tradeType, volume, range, (Symbol.Ask - Symbol.PipSize * range));
            //setPendingOrder(tradeType, volume, range, (Symbol.Ask + Symbol.PipSize * range));

        }

        protected void setPendingOrder(TradeType tradeType, int volume, double range, double targetPrice)
        {
            if (tradeType == TradeType.Sell && targetPrice < Symbol.Bid)
            {
                PlaceStopOrder(tradeType, Symbol.Name, volume, targetPrice, "", stopLoss, range);
            }
            else if (tradeType == TradeType.Sell)
            {
                PlaceLimitOrder(tradeType, Symbol.Name, volume, targetPrice, "", stopLoss, range);
            }
            else if (tradeType == TradeType.Buy && targetPrice > Symbol.Ask)
            {
                PlaceStopOrder(tradeType, Symbol.Name, volume, targetPrice, "", stopLoss, range);
            }
            else
            {
                PlaceLimitOrder(tradeType, Symbol.Name, volume, targetPrice, "", stopLoss, range);
            }

        }
    }

    class PositionData
    {
        public double level;
        public double upperLevel;
        public double lowerLevel;
        private Robot _myRobot;

        public PositionData(double level, int range, Robot robot)
        {
            this.level = level;
            _myRobot = robot;
            upperLevel = level + range * _myRobot.Symbol.PipSize;
            lowerLevel = level - range * _myRobot.Symbol.PipSize;
        }

        public bool isUpperLevelHit(double currentPrice)
        {
            return currentPrice > upperLevel;
        }

        public bool isLowerLevelHit(double currentPrice)
        {
            return currentPrice < upperLevel;
        }



    }
}
