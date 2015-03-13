using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.MulticastSimulatorComponents;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using System.Threading;
using NetworkSimulator.RoutingComponents.MulticastRoutingStrategies;

namespace NetworkSimulator.RoutingComponents.MulticastCommonAlgorithms
{
    public class Lmmira : MulticastRoutingStrategy
    {
        private LmmiraCore _LCore;

        protected MulticastDijkstra _MD;

        public Dictionary<Link, double> cost;

        public List<MulticastRequest> randomRequests;

        public Lmmira(Topology topology): base(topology)
        {            
            Initialize();
        }

        private void Initialize()
        {
            cost = new Dictionary<Link, double>();
            randomRequests = new List<MulticastRequest>();
            _MD = new MulticastDijkstra(_Topology);

            _LCore = new LmmiraCore(_Topology, 3, 0, 1000, this);
            _LCore.Start();
            
        }

        #region throw
        //Dung
        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            throw new NotImplementedException("for Unicast");
        }
        //public override List<Link> GetPath(int sourceId, int destinationID, double bandwidth)
        //{
        //    throw new NotImplementedException("for Unicast");
        //    //return temp;
        //}

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("for Unicast");
        //}
        #endregion

        public override Tree GetTree(MulticastRequest request)
        {
            List<Node> des = new List<Node>();
            foreach (int id in request.Destinations)
                des.Add(_Topology.Nodes[id]);

            EliminateAllLinksNotSatisfy(request.Demand);
            Tree tree = new Tree();
            lock (cost)
            {
                tree = _MD.GetShortestTree(_Topology.Nodes[request.SourceId], des, cost);
                
               // Console.WriteLine("So link {0}",cost.Count(r => r.Value > 0));
            }
            randomRequests.Add(request);
            RestoreTopology();
            return tree;
        }
       
    }
}
