using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class MHA : RoutingStrategy
    {
        //private BreadthFirstSearch _BFS;
        private Dictionary<Link, double> _Cost;
        private Dijkstra _Dijsktra;
        

        public MHA(Topology topology) 
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            //_BFS = new BreadthFirstSearch(_Topology);

            _Cost = new Dictionary<Link, double>();
            _Dijsktra = new Dijkstra(_Topology);

            foreach (var link in _Topology.Links)
            {
                _Cost[link] = 1;
            }
        }

        //Edited: 02/08 Dung
        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    List<Link> path = new List<Link>();
        //    EliminateAllLinksNotSatisfy(bandwidth);
        //    path = _BFS.FindPath(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId]);
        //    RestoreTopology();
        //    return path;
        //}        

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            //List<Link> path = new List<Link>();

            //EliminateAllLinksNotSatisfy(request.Demand);

            //path = _BFS.FindPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId]);
            
            //RestoreTopology();

            //return path;

            // caoth            

            EliminateAllLinksNotSatisfy(request.Demand);           

            // Use dijsktra to get path
            var resultPath = _Dijsktra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _Cost);

            RestoreTopology();
            return resultPath;
        }



    }
}
