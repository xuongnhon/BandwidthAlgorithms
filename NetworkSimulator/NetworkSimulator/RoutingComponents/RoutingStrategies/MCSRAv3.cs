using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonObjects;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using Troschuetz.Random;
using System.Threading;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class MCSRAv3 : RoutingStrategy
    {
        private class Request
        { 
            private int _SourceId;

            public int SourceId
            {
                get { return _SourceId; }
                set { _SourceId = value; }
            }

            private int _DestinationId;

            public int DestinationId
            {
                get { return _DestinationId; }
                set { _DestinationId = value; }
            }

            private double _Demand;

            public double Demand
            {
                get { return _Demand; }
                set { _Demand = value; }
            }

            public string IEKey
            {
                get { return _SourceId + "|" + _DestinationId; }
            } 

            public Request(int sourceId, int destinationId, double bandwidth)
            {
                this._SourceId = sourceId;
                this._DestinationId = destinationId;
                this._Demand = bandwidth;
            }
        }

        private static List<Request> _HistoricalRequests;
        private static Dictionary<string, List<Request>> _IEHistoricalRequests;
        private static Dictionary<string, double> _IEProbability;
        private static Dictionary<string, double> _PridictionCriticality;
        private static Dictionary<string, double> _ActualCriticality;
        private static Dictionary<string, double[]> _MinAvgMax;
        private static DiscreteUniformDistribution _UniformDistribution;
        private static TriangularDistribution _TriangularDistribution;
        private static Generator _Generator;
        private static readonly Object _LockingObject = new Object();
        private static bool _Initialized = false;
        private static int _TotalRequest = 0;
        private static int _L;
        private static int _K;
        private static int _N;
        private static double _T;
        private static double _Epsilon;
        private static double _Alpha;

        public MCSRAv3(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {
            lock (_LockingObject)
            {
                if (!_Initialized)
                {
                    //
                    _HistoricalRequests = new List<Request>();
                    _IEHistoricalRequests = new Dictionary<string, List<Request>>();
                    _IEProbability = new Dictionary<string, double>();
                    _PridictionCriticality = new Dictionary<string, double>();
                    _ActualCriticality = new Dictionary<string, double>();
                    _Generator = new StandardGenerator();
                    _UniformDistribution = new DiscreteUniformDistribution(_Generator);
                    _TriangularDistribution = new TriangularDistribution(_Generator);
                    _MinAvgMax = new Dictionary<string, double[]>();
                    _L = 100;
                    _K = 4;
                    _N = 100;
                    _T = 500;
                    _Epsilon = 0.1;
                    _Alpha = Math.Pow(_Epsilon / _T, (double)_K / _L);
                    _UniformDistribution.Alpha = 0;
                    _UniformDistribution.Beta = 99;
                    //
                    foreach (var link in _Topology.Links)
                    {
                        _PridictionCriticality[link.Key] = 1d / link.Capacity;
                        _ActualCriticality[link.Key] = 0;
                    }
                    Simulate();
                    _Initialized = true;
                }
            }
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

        private List<Link> FindPath(Topology topology, Request request, bool isSimulate)
        {
            //Dictionary<string, double> criticality = null;
            //lock (_PridictionCriticality)
            //{ 
            //    criticality = new Dictionary<string, double>(_PridictionCriticality);
            //}
            
            Dijkstra dijkstra = new Dijkstra(topology);

            // Eliminate all link that have residual bandwidth less than bandwidth demand
            EliminateLinks(topology, request.Demand);

            List<Link> links = topology.Links.Where(o => o.ResidualBandwidth >= request.Demand).ToList();

            Dictionary<Link, double> LB = new Dictionary<Link, double>();
            Dictionary<Link, double> LP = new Dictionary<Link,double>();
            Dictionary<Link, double> LW = new Dictionary<Link, double>();

            if (isSimulate)
            {
                foreach (var link in links)
                {
                    LB[link] = link.ResidualBandwidth;
                }

                Normalize(LB, 1, 100);

                foreach (var link in links)
                {
                    LW[link] = LB[link];
                }
            }
            else
            {
                foreach (var link in links)
                {
                    LB[link] = 1d / link.ResidualBandwidth;
                    LP[link] = _PridictionCriticality[link.Key];
                }

                Normalize(LB, 1, 100);
                Normalize(LP, 1, 100);

                foreach (var link in links)
                {
                    LW[link] = 0.8 * LB[link] + 0.2 * LP[link];
                }
            }

            // Use dijkstra algorithm to find path with link weight array that computed above
            var path = dijkstra.GetShortestPath(request.SourceId, request.DestinationId, LW);

            // Restore topology after eliminating links
            RestoreTopology(topology);

            return path;
        }

        private Request GetRequest()
        {
            int randomNumber = _UniformDistribution.Next();
            double alpha = 0;
            double beta = 0;
            IEPair reqIE = null;
            foreach (var ie in _Topology.IEPairs)
            {
                beta = alpha + _IEProbability[ie.Key];
                if (randomNumber >= alpha && randomNumber < beta)
                {
                    reqIE = ie;
                    break;
                }
                alpha = beta;
            }

            _TriangularDistribution.Alpha = _MinAvgMax[reqIE.Key][0] / _MinAvgMax[reqIE.Key][2];
            _TriangularDistribution.Gamma = _MinAvgMax[reqIE.Key][1] / _MinAvgMax[reqIE.Key][2];
            _TriangularDistribution.Beta = _MinAvgMax[reqIE.Key][2] / _MinAvgMax[reqIE.Key][2];

            Request req = 
                new Request(reqIE.Ingress.Key, reqIE.Egress.Key, 
                    Math.Round(_TriangularDistribution.NextDouble() * _MinAvgMax[reqIE.Key][2]));

            return req;
        }

        private void Simulate()
        {
            foreach (var ie in _Topology.IEPairs)
            {
                _IEHistoricalRequests[ie.Key] = _HistoricalRequests
                    .Where(r => r.IEKey == ie.Key).ToList();

                double[] minAvgMax = new double[3];
                if (_IEHistoricalRequests[ie.Key].Count > 0)
                {
                    minAvgMax[0] = _IEHistoricalRequests[ie.Key].Min(o => o.Demand);
                    minAvgMax[1] = _IEHistoricalRequests[ie.Key].Average(o => o.Demand);
                    minAvgMax[2] = _IEHistoricalRequests[ie.Key].Max(o => o.Demand);
                }
                else
                {
                    minAvgMax[0] = 10;
                    minAvgMax[1] = 25;
                    minAvgMax[2] = 40;
                }
                _MinAvgMax[ie.Key] = minAvgMax;
            }

            double sum = _IEHistoricalRequests.Values.Sum(t => t.Count + _T);

            foreach (var ie in _Topology.IEPairs)
            {
                _IEProbability[ie.Key] = (_IEHistoricalRequests[ie.Key].Count + _T) * 100d / sum; 
            }

            // Clone common topology for simulating
            Topology topology = new Topology(_Topology);

            Dictionary<string, double> totalCriticality = new Dictionary<string, double>();
            Dictionary<string, double> averageCriticality = new Dictionary<string, double>();

            foreach (var link in _Topology.Links)
            {
                averageCriticality[link.Key] = 0;
            }

            for (int i = 0; i < _N; i++)
            {
                Topology tempTopology = new Topology(topology);
                Dijkstra dijkstra = new Dijkstra(tempTopology);

                foreach (var link in topology.Links)
                {
                    totalCriticality[link.Key] = 0;
                }

                for (int j = 0; j < _K; j++)
                {
                    Request req = GetRequest();
                    // Print out
                    //Console.WriteLine("s=" + req.SourceId + " d=" + req.DestinationId + " d=" + req.Demand);

                    var path = FindPath(tempTopology, req, true);

                    // Reserve bandwidth demand
                    ReserveBandwidth(path, req.Demand);

                    // Compute criticality of links
                    foreach (var link in path)
                    {
                        totalCriticality[link.Key] += req.Demand;
                        //Console.WriteLine(link);
                    }
                    //Console.WriteLine("------------------------------");
                }

                foreach (var link in _Topology.Links)
                {
                    averageCriticality[link.Key] += totalCriticality[link.Key] / _N;
                }
            }

            // Lock pridiction criticality for updating
            //lock (_PridictionCriticality)
            {
                _PridictionCriticality = averageCriticality;
            }
            Console.WriteLine("///////////////////////////////////////////////////////////////");

            if (_T > _Epsilon)
                _T *= _Alpha;

            //debug++;
            //if (debug == 30)
            //{ }
        }

        //static int debug = 0;

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        {
            Request hrequest = new Request(sourceId, destinationId, bandwidth);
            _HistoricalRequests.Add(hrequest);
            if (_HistoricalRequests.Count > _L)
                _HistoricalRequests.RemoveAt(0);
            _TotalRequest++;

            if (_TotalRequest % _K == 0)
            {
                //lock (_PridictionCriticality)
                //{
                //    foreach (var link in _Topology.Links)
                //    {
                //        _PridictionCriticality[link.Key] = _ActualCriticality[link.Key];
                //        _ActualCriticality[link.Key] = 0;
                //    }
                //}
                //Thread worker = new Thread(new ThreadStart(Simulate));
                //worker.Start();
                Simulate();
            }

            var path = FindPath(_Topology, hrequest, false);

            //foreach (var link in path)
            //{
            //    _ActualCriticality[link.Key] += bandwidth;
            //}

            //if (_TotalRequest == 100)
            //{ }

            return path;
        }

        #region Not implement
        public override List<NetworkComponents.Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
