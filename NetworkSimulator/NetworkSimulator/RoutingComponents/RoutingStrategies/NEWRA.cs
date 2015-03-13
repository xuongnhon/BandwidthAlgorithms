using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.RoutingComponents.CommonObjects;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class NEWRA : RoutingStrategy
    {
        private static readonly Object _LockingObject;
        private static Dictionary<IEPair, int> _IEReqCount;
        private static int _TotalReq;
        private static List<IEPair> _IEPairs;
        private static Dictionary<IEPair, double> _IEProbability;
        private static Dictionary<IEPair, Dictionary<Link, int>> _IELinkCriticality;
        private static Dictionary<IEPair, Dictionary<Link, double>> _IETotalBandwidthDemand;
        private static AllSimplePaths _AllSimplePaths;

        private Dictionary<Link, double> _ITF;
        private Dictionary<Link, double> _LB;
        private Dijkstra _Dijkstra;

        public NEWRA(Topology topology)
            : base(topology)
        {
            this._Topology = topology;
            Initialize();
        }

        static NEWRA()
        {
            _LockingObject = new Object();
            _IEReqCount = new Dictionary<IEPair, int>();
            _IEProbability = new Dictionary<IEPair, double>();
            _IELinkCriticality = new Dictionary<IEPair, Dictionary<Link, int>>();
            _IETotalBandwidthDemand = new Dictionary<IEPair, Dictionary<Link, double>>();
            _TotalReq = 0;
        }

        private void Initialize()
        {
            // Initialize for non-static objects
            _ITF = new Dictionary<Link, double>();
            _LB = new Dictionary<Link, double>();
            _Dijkstra = new Dijkstra(_Topology);

            // Initialize for static objects
            lock (_LockingObject)
            {
                if (_IEPairs == null)
                {
                    _IEPairs = _Topology.IEPairs;
                    foreach (var ie in _IEPairs)
                    {
                        _IEReqCount[ie] = 0;
                        _IELinkCriticality[ie] = new Dictionary<Link, int>();
                        foreach (var link in _Topology.Links)
                        {
                            _IELinkCriticality[ie][link] = 0;
                        }
                    }
                }

                DoOfflinePhase();
            }
        }

        private void DoOfflinePhase()
        {
            if (_AllSimplePaths == null)
            {
                foreach (var ie in _IEPairs)
                {
                    _AllSimplePaths = new AllSimplePaths(_Topology);
                    var paths = _AllSimplePaths.GetPaths(ie.Ingress, ie.Egress);
                    foreach (var path in paths)
                    {
                        foreach (var link in path)
                        {
                            _IELinkCriticality[ie][link]++;
                        }
                    }
                }
                Console.WriteLine("Doing offline phase have been finished !");
            }
        }

        private static IEPair GetIEPair(int sourceId, int destinationId)
        {
            return _IEPairs
                .SingleOrDefault(o => o.Ingress.Key == sourceId && o.Egress.Key == destinationId);
        }

        private void Normalize(Dictionary<Link, double> data, int a, int b)
        {
            double A = data.Values.Min();
            double B = data.Values.Max();
            foreach (var link in _Topology.Links)
            {
                double x = data[link];
                if (A - B != 0)
                    data[link] = a + (x - A) * (b - a) / (B - A);
                else
                    data[link] = b;
            }
        }

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        {
            // Code for statistics here
            IEPair cie = GetIEPair(sourceId, destinationId);
            _IEReqCount[cie]++;
            _TotalReq++;
            foreach (var ie in _IEPairs)
            {
                _IEProbability[ie] = (double)_IEReqCount[ie] / _TotalReq;
            }
            
            foreach (var link in _Topology.Links)
            {
                // Interference
                double criticality = 0;
                foreach (var ie in _IEPairs)
                {
                    criticality += _IEProbability[ie] * _IELinkCriticality[ie][link];
                }
                _ITF[link] = criticality;
                // load balance
                _LB[link] = link.UsingBandwidth / link.Capacity;
            }

            Normalize(_ITF, 1, 100);
            Normalize(_LB, 1, 100);

            Dictionary<Link, double> linkCosts = new Dictionary<Link, double>();
            foreach (var link in _Topology.Links.Where(l => l.ResidualBandwidth >= bandwidth))
            {
                linkCosts[link] = 0 * _ITF[link] + 1 * _LB[link];
            }

            EliminateAllLinksNotSatisfy(bandwidth);

            var path = _Dijkstra.GetShortestPath(sourceId, destinationId, linkCosts);

            RestoreTopology();

            if (++count == 2000)
            { }

            return path;
        }

        static int count = 0;

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        {
            throw new NotImplementedException();
        }
    }
}
