using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.CommonObjects;
using NetworkSimulator.NetworkComponents;
using System.Diagnostics;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class NewMIRA : RoutingStrategy
    {
        private int _Alpha;
        private Dijkstra _Dijkstra;
        private Dictionary<Link, double> _Cost;
        FordFulkerson _FordFulkerson;

        public int Alpha
        {
            set { this._Alpha = value; }
        }

        public NewMIRA(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _Alpha = 1;
            _Dijkstra = new Dijkstra(_Topology);
            _Cost = new Dictionary<Link, double>();
            _FordFulkerson = new FordFulkerson(_Topology);
            //ResetCostLink();
        }

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            //  EliminateAllLinksNotSatisfy(request.Demand);


            GenerateNewMIRACost(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId]);

            EliminateAllLinksNotSatisfy(request.Demand);

            //int debug=0;
            //if(request.Id > 250)
            //    debug = -1;

            var resultPath = _Dijkstra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _Cost);

            RestoreTopology();

            return resultPath;
        }

        private void ResetCostLink()
        {
            foreach (Link link in _Topology.Links)
            {
                _Cost[link] = double.Epsilon;
            }
        }

        // caoth
        // Generate MIRA cost
        private void GenerateNewMIRACost(Node source, Node destination)
        {
            ResetCostLink();
            foreach (var item in _Topology.IEPairs)
            {                
                // caoth
                if (item.Ingress != source || item.Egress != destination)
                {
                    double maxflow = 0;//_FordFulkerson.ComputeMaxFlow(item.Ingress, item.Egress);
                    Dictionary<Link, double> subflows = _FordFulkerson.SubFlowOfAllLinks(item.Ingress, item.Egress, ref maxflow);

                    maxflow = maxflow == 0 ? double.Epsilon : maxflow;

                    foreach (var link in _Topology.Links)
                    {
                        //if (maxflow > 0 && link.ResidualBandwidth>0)
                            _Cost[link] += subflows[link] / (maxflow * link.ResidualBandwidth);
                    }
                }                
            }
        }
    }
}
