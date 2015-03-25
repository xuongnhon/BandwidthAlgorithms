using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class BCRA : RoutingStrategy
    {
        private Dictionary<Link, double> _Cost;
        private Dijkstra _Dijsktra;

        public BCRA(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _Cost = new Dictionary<Link, double>();
            _Dijsktra = new Dijkstra(_Topology);
        }

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}

        //public override List<NetworkComponents.Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    EliminateAllLinksNotSatisfy(bandwidth);

        //    // Calculate link weight with all link that satisfy the bandwidth
        //    foreach (Link link in _Topology.Links)
        //    {
        //        if (link.ResidualBandwidth > 0)
        //        {
        //            _Cost[link] = (Math.Pow(10, 8) / link.Capacity) * (link.UsingBandwidth / link.Capacity) + 1;
        //        }
        //    }

        //    // Use dijsktra to get path
        //    var resultPath = _Dijsktra.GetShortestPath(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId], _Cost);

        //    RestoreTopology();
        //    return resultPath;
        //}

        public override List<NetworkComponents.Link> GetPath(SimulatorComponents.Request request)
        {
            EliminateAllLinksNotSatisfy(request.Demand);

            // Calculate link weight with all link that satisfy the bandwidth
            foreach (Link link in _Topology.Links)
            {
                if (link.ResidualBandwidth > 0)
                {
                    //_Cost[link] = (Math.Pow(10, 8) / link.Capacity) * (link.UsingBandwidth / link.ResidualBandwidth) + 1;
                    _Cost[link] = (Math.Pow(10, 8) / link.Capacity) * (link.UsingBandwidth / link.Capacity) + 1;
                }
            }

            // Use dijsktra to get path
            var resultPath = _Dijsktra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _Cost);

            RestoreTopology();
            return resultPath;
        }

    }
}
