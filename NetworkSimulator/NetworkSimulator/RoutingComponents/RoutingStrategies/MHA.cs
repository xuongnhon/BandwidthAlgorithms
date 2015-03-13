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
        private BreadthFirstSearch _BFS;

        

        public MHA(Topology topology) 
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            _BFS = new BreadthFirstSearch(_Topology);
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
            List<Link> path = new List<Link>();

            EliminateAllLinksNotSatisfy(request.Demand);

            path = _BFS.FindPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId]);
            
            RestoreTopology();
            return path;
        }



    }
}
