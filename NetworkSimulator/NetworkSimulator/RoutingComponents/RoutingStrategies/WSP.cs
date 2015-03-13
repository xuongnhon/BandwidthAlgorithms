using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class WSP : RoutingStrategy
    {
        private ShortestPaths _ShortestPaths;

        public WSP(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _ShortestPaths = new ShortestPaths(_Topology);
        }

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    EliminateAllLinksNotSatisfy(bandwidth);

        //    Dictionary<Link, double> cost = new Dictionary<Link,double>();
        //    foreach (Link link in _Topology.Links)
        //    {
        //        cost[link] = 1;
        //    }
        //    var shortestPaths = _ShortestPaths.GetShortestPaths(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId], cost);

        //    List<Link> resultPath = new List<Link>();
        //    double maxResidualBw = 0; 
        //    foreach (var path in shortestPaths)
        //    {
        //        double min = double.MaxValue;
        //        foreach (Link link in path)
        //        {
        //            if (min > link.ResidualBandwidth)
        //                min = link.ResidualBandwidth;
        //        }
        //        if (maxResidualBw < min)
        //        {
        //            maxResidualBw = min;
        //            resultPath = path;
        //        }
        //    }
        //    RestoreTopology();
        //    return resultPath;
        //}        

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            EliminateAllLinksNotSatisfy(request.Demand);

            Dictionary<Link, double> cost = new Dictionary<Link, double>();
            foreach (Link link in _Topology.Links)
            {
                cost[link] = 1;
            }
            var shortestPaths = _ShortestPaths.GetShortestPaths(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], cost);

            List<Link> resultPath = new List<Link>();
            double maxResidualBw = 0;
            foreach (var path in shortestPaths)
            {
                double min = double.MaxValue;
                foreach (Link link in path)
                {
                    if (min > link.ResidualBandwidth)
                        min = link.ResidualBandwidth;
                }
                if (maxResidualBw < min)
                {
                    maxResidualBw = min;
                    resultPath = path;
                }
            }
            RestoreTopology();
            return resultPath;
        }        

    }
}
