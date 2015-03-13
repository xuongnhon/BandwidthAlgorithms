using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.CommonObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class MCSRAv2 : RoutingStrategy
    {
        #region Static fields
        private static readonly Object _LockingObject;
        private static Dictionary<IEPair, int> _IEReqCount;
        private static int _TotalReq;
        private static Dictionary<IEPair, double> _IEProbability;
        private static Dictionary<IEPair, Dictionary<Link, int>> _IELinkCriticality;
        private static Dictionary<IEPair, double> _IETotalBandwidthDemand;
        private static Generator _Generator;
        private static int _N;
        private static int _K;
        private static bool _Initialized = false;
        private static Dictionary<string, double> _Criticality;
        private static DiscreteUniformDistribution _URandForReq;
        private static Dictionary<string, double> _PastCriticality;
        private static Dictionary<string, double> _PridictionCriticality;
        #endregion

        #region Non-static fields

        #endregion

        static MCSRAv2()
        {
            _LockingObject = new Object();
            _Generator = new StandardGenerator();
            _IEReqCount = new Dictionary<IEPair, int>();
            _IEProbability = new Dictionary<IEPair, double>();
            _IELinkCriticality = new Dictionary<IEPair, Dictionary<Link, int>>();
            _IETotalBandwidthDemand = new Dictionary<IEPair, double>();
            _Criticality = new Dictionary<string, double>();
            _URandForReq = new DiscreteUniformDistribution(_Generator);
            _PastCriticality = new Dictionary<string, double>();
            _PridictionCriticality = new Dictionary<string, double>();
            _TotalReq = 0;
            _N = 2;
            _K = 10;
        }

        public MCSRAv2(Topology topology)
            : base(topology)
        {
            this._Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            lock (_LockingObject)
            {
                if (!_Initialized)
                {
                    foreach (var ie in _Topology.IEPairs)
                    {
                        _IEReqCount[ie] = 0;
                        _IEProbability[ie] = (1d / _Topology.IEPairs.Count) * 100;
                        _IETotalBandwidthDemand[ie] = 0;
                        _IELinkCriticality[ie] = new Dictionary<Link, int>();
                        foreach (var link in _Topology.Links)
                        {
                            _IELinkCriticality[ie][link] = 0;
                        }
                    }
                    foreach (var link in _Topology.Links)
                    {
                        _PastCriticality[link.Key] = 4800d / link.Capacity;
                        _PridictionCriticality[link.Key] = 1;
                    }
                    // Initialize for uniform random
                    _URandForReq.Alpha = 0;
                    _URandForReq.Beta = 99;

                    // Do offline phase
                    DoOfflinePhase();

                    _Initialized = true;
                }
            }
        }

        private IEPair GetIEPair(int sourceId, int destinationId)
        {
            return _Topology.IEPairs
                .SingleOrDefault(o => o.Ingress.Key == sourceId && o.Egress.Key == destinationId);
        }

        private void EliminateLinks(Topology topology, double demand)
        {
            foreach (var link in topology.Links)
            {
                if (link.ResidualBandwidth < demand)
                    link.ResidualBandwidth = -link.ResidualBandwidth;
            }
        }

        private void RestoreTopology(Topology topology)
        {
            foreach (var link in topology.Links)
            {
                link.ResidualBandwidth = Math.Abs(link.ResidualBandwidth);
            }
        }

        private void ReserveBandwidth(List<Link> path, double bandwidth)
        {
            foreach (var link in path)
            {
                link.UsingBandwidth += bandwidth;
            }
        }

        private struct request
        {
            int _SourceId;

            public int SourceId
            {
                get { return _SourceId; }
                set { _SourceId = value; }
            }

            int _DestinationId;

            public int DestinationId
            {
                get { return _DestinationId; }
                set { _DestinationId = value; }
            }

            double _Demand;

            public double Demand
            {
                get { return _Demand; }
                set { _Demand = value; }
            }

            public request(int sourceId, int destinationId, double bandwidth)
            {
                this._SourceId = sourceId;
                this._DestinationId = destinationId;
                this._Demand = bandwidth;
            }
        }

        private request GetRequest()
        {
            int randomNumber = _URandForReq.Next();
            double alpha = 0;
            double beta = 0;
            IEPair reqIE = null;
            foreach (var ie in _Topology.IEPairs)
            {
                beta = alpha + _IEProbability[ie];
                if (randomNumber >= alpha && randomNumber < beta)
                {
                    reqIE = ie;
                    break;
                }
                alpha = beta;
            }

            request req = new request(reqIE.Ingress.Key, reqIE.Egress.Key, 30);
            return req;
        }

        private void Normalize(Dictionary<Link, double> data, int a, int b)
        {
            double A = data.Values.Min();
            double B = data.Values.Max();
            List<Link> links = data.Keys.ToList();
            foreach (var link in links)
            {
                double x = data[link];
                if (A - B != 0)
                    data[link] = a + (x - A) * (b - a) / (B - A);
                else
                    data[link] = b;
            }
        }

        private void DoOfflinePhase()
        {
            // Clone common topology for simulating
            Topology topology = new Topology(_Topology);
            Dijkstra dijkstra = null;

            Dictionary<string, double> criticality = new Dictionary<string, double>();
            Dictionary<string, double> averageCriticality = new Dictionary<string, double>();

            foreach (var link in _Topology.Links)
            {
                averageCriticality[link.Key] = 0;
            }

            for (int i = 0; i < _N; i++)
            {
                Topology tempTopology = new Topology(topology);
                dijkstra = new Dijkstra(tempTopology);

                foreach (var link in topology.Links)
                {
                    criticality[link.Key] = 1;
                } 

                for (int j = 0; j < _K; j++)
                {
                    request req = GetRequest();
                    // print out
                    Console.WriteLine("s=" + req.SourceId + " d=" + req.DestinationId + " d="+req.Demand);

                    // Load balancing
                    Dictionary<Link, double> LB = new Dictionary<Link, double>();
                    foreach (var link in tempTopology.Links)
                    {
                        LB[link] = (link.UsingBandwidth > 0 ? link.UsingBandwidth : 1 * _PastCriticality[link.Key]) / link.Capacity;
                    }

                    // Normalize LB array
                    Normalize(LB, 1, 100);

                    // Compute link weight
                    Dictionary<Link, double> LW = new Dictionary<Link, double>();
                    foreach (var link in tempTopology.Links)
                    {
                        LW[link] = LB[link];
                    }
                    // Eliminate all link that have residual bandwidth less than bandwidth demand
                    EliminateLinks(tempTopology, req.Demand);

                    // Use dijkstra algorithm to find path with link weight array that computed above
                    var path = dijkstra.GetShortestPath(req.SourceId, req.DestinationId, LB);

                    // Restore topology after eliminating links
                    RestoreTopology(tempTopology);

                    // Reserve bandwidth demand
                    ReserveBandwidth(path, req.Demand);

                    // Compute criticality of links
                    foreach (var link in path)
                    {
                        criticality[link.Key]++;
                        Console.WriteLine(link + " link weight=" + LB[link]);
                    }
                    // TODO:
                    Console.WriteLine("||||||||||||||||||||||||||");
                }

                foreach (var link in _Topology.Links)
                {
                    averageCriticality[link.Key] += criticality[link.Key] / _N;
                }
            }

            lock (_PridictionCriticality)
            {
                _PridictionCriticality = averageCriticality;    
            }
        }

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        {
            // Code for statistics here
            IEPair cie = GetIEPair(sourceId, destinationId);
            _IEReqCount[cie]++;
            _TotalReq++;
            _IETotalBandwidthDemand[cie] += bandwidth;
            foreach (var ie in _Topology.IEPairs)
            {
                _IEProbability[ie] = ((double)_IEReqCount[ie] * 100) / _TotalReq;
            }

            Console.WriteLine("______________________________________");

            return null;
        }

        #region Not implement
        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
