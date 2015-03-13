using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.SimulatorComponents
{
    public class Request
    {
        protected int _Id;

        public int Id
        {
            get { return _Id; }
        }

        // Hien HC
        protected bool _Reroute = false;
        public bool Reroute
        {
            set { _Reroute = value; }
        }
        public bool isReroute()
        {
            return _Reroute;
        }

        protected int _SourceId;

        public int SourceId
        {
            get { return _SourceId; }
        }

        private int _DestinationId;

        public int DestinationId
        {
            get { return _DestinationId; }
        }

        protected double _Demand;

        public double Demand
        {
            get { return _Demand; }
        }

        protected long _IncomingTime;

        public long IncomingTime
        {
            get { return _IncomingTime; }    
        }

        protected long _HoldingTime;

        public long HoldingTime
        {
            get { return _HoldingTime; }
            set { _HoldingTime = value; }
        }

        // caoth
        public Request()
        {
        }

        public Request(int id, int sourceId, int destinationId, double demand, long incomingTime, long holdingTime)
        {
            _Id = id;
            _SourceId = sourceId;
            _DestinationId = destinationId;
            _Demand = demand;
            _IncomingTime = incomingTime;
            _HoldingTime = holdingTime;
        }

        public override string ToString()
        {
            return "REQ-" + _Id + " SCR=" + _SourceId + " DES=" + _DestinationId + " BWD=" + _Demand + " ICT=" + _IncomingTime + " HDT=" + _HoldingTime;
        }
    }
}
