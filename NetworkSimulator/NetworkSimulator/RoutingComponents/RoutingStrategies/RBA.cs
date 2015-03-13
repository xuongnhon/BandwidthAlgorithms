using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class RBA : RoutingStrategy
    {
        private Dijkstra _Dijkstra;
        private Dictionary<Link, double> _LinkCost;


        public RBA(Topology topology) 
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _Dijkstra = new Dijkstra(_Topology);
            _LinkCost = new Dictionary<Link, double>();
        }


        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);
            foreach (var link in _Topology.Links)
            {
                _LinkCost[link] = 1 / link.ResidualBandwidth;
            }
            path = _Dijkstra.GetShortestPath(request.SourceId, request.DestinationId, _LinkCost);
            RestoreTopology();
            return path;
            
        }
    }
}
