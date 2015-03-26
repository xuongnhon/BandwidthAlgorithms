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
    class MIRA : RoutingStrategy
    {
        private int _Alpha;
        private Dijkstra _Dijkstra;
        private Dictionary<Link, double> _Cost;
        FordFulkerson _FordFulkerson;

        public int Alpha
        {
            set { this._Alpha = value; }
        }

        public MIRA(Topology topology)
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

           
            GenerateMIRACost(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId]);
           
            EliminateAllLinksNotSatisfy(request.Demand);
           
     
            var resultPath = _Dijkstra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _Cost);
           
            RestoreTopology();

            return resultPath;
        }


        private void ResetCostLink()
        {
            foreach (Link link in _Topology.Links)
            {
                _Cost[link] = double.Epsilon; // caoth gan 0 moi sai, khi tat ca weight = 0, Dijkstra ko tim ra dg di ngan nhat
               // _Cost[link] = 0;
            }
        }

        // Generate MIRA cost
        private void GenerateMIRACost(Node source, Node destination)
        {
            ResetCostLink();
            foreach (var item in _Topology.IEPairs)
            {
                // Only use Ingress Egress in IEList, except the actual source and destination
              
                //Dung
                //if (item.Ingress != source && item.Egress != destination)
                if (item.Ingress != source || item.Egress != destination)
                {
                    var criticalLinks = _FordFulkerson.FindMinCutSet(item.Ingress, item.Egress);
                    foreach (Link link in criticalLinks)
                    {                       
                        _Cost[link] += _Alpha;
                    }
                }
            }
        }        

    }
}
