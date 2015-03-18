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
            double totalNumberOfPaths = 0;

            foreach (var ie in _Topology.IEPairs)
            {
                AllSimplePaths asp = new AllSimplePaths(_Topology);
                _P[ie] = asp.GetPaths(ie.Ingress, ie.Egress);

                foreach (var path in _P[ie])
                {
                    foreach (var link in path)
                        _Cl[link] += 1d; // / _P[ie].Count;
                }

                totalNumberOfPaths += _P[ie].Count;
            }

            foreach (Link link in _Topology.Links)
            {
                _Cl[link] = _Cl[link] / totalNumberOfPaths;
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

            // restore topology
            RestoreTopology();

            // return path
            return path;
        }
    }
}
