using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.SimulatorComponents;

namespace NetworkSimulator.MulticastSimulatorComponents
{
    public class MulticastRequest : Request
    {
        private List<int> _Destination;

        public List<int> Destinations
        {
            get { return _Destination; }
        }

        public MulticastRequest(int id, int sourceId, List<int> destination, double demand, long incomingTime, long holdingTime)
        {
            _Id = id;
            _SourceId = sourceId;
            _Destination = destination;
            _Demand = demand;
            _IncomingTime = incomingTime;
            _HoldingTime = holdingTime;
        }

        public override string ToString()
        {
            string str = "";
            str="REQ-" + _Id + " SCR=" + _SourceId + " DESMulticast=" ;
            foreach (var des in _Destination)
            {
                str+= "::"+ des;
            }
            str+=" BWD=" + _Demand + " ICT=" + _IncomingTime + " HDT=" + _HoldingTime;
            return str;
        }
    }
}
