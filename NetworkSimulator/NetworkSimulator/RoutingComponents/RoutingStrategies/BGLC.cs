using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class BGLC : RoutingStrategy
    {
        private Dictionary<IEPair, List<List<Link>>> _P;
        private Dictionary<Link, double> _Cl;

        double totalNumberOfPaths ;

        public BGLC(Topology topology)
            : base(topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            _P = new Dictionary<IEPair, List<List<Link>>>();
            _Cl = new Dictionary<Link, double>();
            foreach (var link in _Topology.Links)
                _Cl[link] = 0;

            DoOffinePhase();
        }

        private void DoOffinePhase()
        {
            // caoth
            // caoth No. of demand per link / length of all possible connection
            // formular (1) of 2012_BGMRA
            totalNumberOfPaths = 0;

            foreach (var ie in _Topology.IEPairs)
            {
                AllSimplePaths asp = new AllSimplePaths(_Topology);
                _P[ie] = asp.GetPaths(ie.Ingress, ie.Egress);

                //foreach (var path in _P[ie])
                //{
                //    foreach (var link in path)
                //    {
                //        _Cl[link] += 1d; // / _P[ie].Count;
                //    }

                //    totalNumberOfPaths += path.Count;
                //}

                totalNumberOfPaths += _P[ie].Count; // total path
            }

            

            foreach (Link link in _Topology.Links)
            {
               // _Cl[link] = _Cl[link] / totalNumberOfPaths;
                _Cl[link] = 1;
            }
        }

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            Dictionary<Link, double> w = new Dictionary<Link, double>();
            // compute link weight
            foreach (var link in _Topology.Links)
                w[link] = _Cl[link] / link.ResidualBandwidth;

            // eliminate all link not satisfy bandwidth demand
            EliminateAllLinksNotSatisfy(request.Demand);

            // find path
            Dijkstra dijkstra = new Dijkstra(_Topology);
            var path = dijkstra.GetShortestPath(request.SourceId, request.DestinationId, w);

            // no. of demand per link
            foreach (Link link in path)
                _Cl[link] += 1;

            // restore topology
            RestoreTopology();

            // return path
            return path;
        }
    }
}
