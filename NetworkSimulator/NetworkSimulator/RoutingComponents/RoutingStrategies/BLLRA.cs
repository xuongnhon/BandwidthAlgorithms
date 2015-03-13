using NetworkSimulator.NetworkComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class BLLRA:RoutingStrategy
    {
        private List<Link> _LL;
        private Dictionary<Link, double> _LW;

        public BLLRA(Topology topology)
            : base(topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            _LL = new List<Link>();
            _LW = new Dictionary<Link, double>();
            foreach (var link in _Topology.Links)
            {
                _LL.Add(link);
            }
        }

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            _LL = _LL.OrderByDescending(l => l.ResidualBandwidth).ToList();
            
            double p = 0.0000001;
            double a = 2;
            double min = _LL.First().ResidualBandwidth;
            foreach (var link in _LL)
            {
                _LW[link] = p;
                if (link.ResidualBandwidth < min)
                {
                    min = link.ResidualBandwidth;
                    p = p * a;
                }
                
            }
            EliminateAllLinksNotSatisfy(request.Demand);
            Dijkstra dijkstra = new Dijkstra(_Topology);
            var path = dijkstra.GetShortestPath(request.SourceId, request.DestinationId, _LW);
            RestoreTopology();
            return path;
        }
    }
}
