using NetworkSimulator.MulticastSimulatorComponents;
using NetworkSimulator.NetworkComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.RoutingComponents.MulticastRoutingStrategies
{
    public class SPTR:MulticastRoutingStrategy
    {
        protected MulticastDijkstra _MD;
        public Dictionary<Link, double> cost;
        private bool isFirst = true;
        public SPTR(Topology topology) 
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _MD = new MulticastDijkstra(_Topology);
            cost = new Dictionary<Link, double>();
        }

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            throw new NotImplementedException("for Unicast");
        }

        public override Tree GetTree(MulticastRequest request)
        {
            List<Node> des = new List<Node>();
            Tree tree = new Tree();
            foreach (int id in request.Destinations)
                des.Add(_Topology.Nodes[id]);

            
            
            EliminateAllLinksNotSatisfy(request.Demand);
           
                cost.Clear();
                if (isFirst)
                {
                    foreach (var link in _Topology.Links)
                    {
                        cost.Add(link, 1);
                    }
                }
                else
                {
                    foreach (var link in _Topology.Links)
                    {
                        cost.Add(link, link.UsingBandwidth / link.Capacity);
                    }
                }
                 tree = _MD.GetShortestTree(_Topology.Nodes[request.SourceId], des, cost);
            
           
            RestoreTopology();
            return tree;
        }
    }
}
