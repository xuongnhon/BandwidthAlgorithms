using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonObjects;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using Troschuetz.Random;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class Request
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

        public Request(int sourceId, int destinationId, double demand)
        {
            _SourceId = sourceId;
            _DestinationId = destinationId;
            _Demand = demand;
        }
    }

    class MCSRA : RoutingStrategy
    {
        #region Fields

        private static List<Request> _PastData;

        private static Dictionary<IEPair, double> _Probability;

        private static Dictionary<IEPair, double> _AverageDemand;

        private static Dictionary<string, double> _PredictionValues;

        private static Troschuetz.Random.StandardGenerator _Generator;

        private static bool simulated = false;

        private int _N;

        private int _K;

        private DiscreteUniformDistribution _UniformRandom;

        private TriangularDistribution _TriangularRandom;

        private Dijkstra _Dijkstra;

        #endregion

        #region Properties

        #endregion

        static MCSRA()
        {
            _PastData = new List<Request>();
            _Probability = new Dictionary<IEPair, double>();
            _AverageDemand = new Dictionary<IEPair, double>();
            _PredictionValues = new Dictionary<string, double>();
            _Generator = new StandardGenerator();
        }

        public MCSRA(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        private void Initialize()
        {   
            _Dijkstra = new Dijkstra(_Topology);
            _N = 50;
            _K = 20;
            _UniformRandom = new DiscreteUniformDistribution(_Generator);
            _UniformRandom.Alpha = 0;
            _UniformRandom.Beta = 3;

            _TriangularRandom = new TriangularDistribution(_Generator);
            _TriangularRandom.Alpha = 0.25;
            _TriangularRandom.Beta = 1.0;
            _TriangularRandom.Gamma = 0.625;

            if (simulated == false)
            {
                simulated = true;
                foreach (var link in _Topology.Links)
                {
                    _PredictionValues[link.Key] = 1;
                }
                MCS();
            }
        }

        public void MCS()
        {
            // clone common topology for simulating
            Topology topology = null;
            topology = new Topology(_Topology);

            Dijkstra dijkstra = null;

            // New and initialize for predictionValue dictionary.
            Dictionary<string, double> tempValues = new Dictionary<string,double>();
            foreach (var link in topology.Links)
            {
                // Set 0 value for each prediction value of links.
                tempValues[link.Key] = 0;
                // Abs residual bandwidth value to ensure all values are positive
                link.ResidualBandwidth = Math.Abs(link.ResidualBandwidth);
            }
            
            // Simulate _N time for _K next requests.
            for (int i = 0; i < _N; i++)
            {
                Topology tempTopology = new Topology(topology);
                dijkstra = new Dijkstra(tempTopology);

                for (int j = 0; j < _K; j++)
                {
                    Request req = PredictRequest();
                    EliminateLinks(tempTopology, req.Demand);
                    Dictionary<Link, double> LinkWeight = new Dictionary<Link,double>();
                    foreach (var link in tempTopology.Links)
                    {
                        LinkWeight[link] = _PredictionValues[link.Key] / link.ResidualBandwidth;
                    }
                    var path = dijkstra.GetShortestPath(req.SourceId, req.DestinationId, LinkWeight);
                    ReserveBandwidth(path, req.Demand);
                    RestoreTopology(tempTopology);
                }

                foreach (var link in tempTopology.Links)
                {
                    tempValues[link.Key] += 1d / (link.ResidualBandwidth + 1);
                }
            }

            // Update _PredictionValues dictionary so we have to lock it.
            lock (_PredictionValues)
            {
                foreach (var link in _Topology.Links)
                {
                    _PredictionValues[link.Key] = tempValues[link.Key] / _N;
                    //Console.WriteLine(link + " " + _PredictionValues[link.Key]);
                }
            }
            
            Console.WriteLine("Finished ... ");
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

        public double Estimate(string linkKey)
        {
            //Link link = _Topology.Links.SingleOrDefault(l => l.Key.Equals(linkKey));
            return 1;
        }

        private Request PredictRequest()
        {
            IEPair ie = _Topology.IEPairs[_UniformRandom.Next()];
            //Console.WriteLine(ie);
            return new Request(ie.Ingress.Key, ie.Egress.Key, PredictDemand(ie));
        }

        private double PredictDemand(IEPair ie)
        {
            return Math.Round(_TriangularRandom.NextDouble() * 40);
        }

        private IEPair GetIEPair(int sourceId, int destinationId)
        {
            return _Topology.IEPairs
                .SingleOrDefault(o => o.Ingress.Key == sourceId && o.Egress.Key == destinationId);
        }

        private void AddPastReq(int sourceId, int destinationid, double bandwidth)
        {
            _PastData.Add(new Request(sourceId, destinationid, bandwidth));
            foreach (var ie in _Topology.IEPairs)
            {
                List<Request> reqs = _PastData
                    .Where(r => r.SourceId == ie.Ingress.Key && r.DestinationId == ie.Egress.Key)
                    .ToList();

                _Probability[ie] = (double)reqs.Count / _PastData.Count;
                _AverageDemand[ie] = reqs.Sum(r => r.Demand) / reqs.Count;
            }
            if (_PastData.Count % _K == 0)
            {
                System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(MCS));
                t.Start();
            }
        }

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    AddPastReq(sourceId, destinationId, bandwidth);
        //    Dictionary<Link, double> linkWeight = new Dictionary<Link, double>();
        //    lock (_PredictionValues)
        //    {
        //        foreach (var link in _Topology.Links)
        //        {
        //            linkWeight[link] = _PredictionValues[link.Key] / link.ResidualBandwidth;
        //        }   
        //    }
        //    EliminateAllLinksNotSatisfy(bandwidth);
        //    var path = _Dijkstra.GetShortestPath(sourceId, destinationId, linkWeight);
        //    RestoreTopology();
        //    return path;
        //}

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            AddPastReq(request.SourceId,request.DestinationId, request.Demand);
            Dictionary<Link, double> linkWeight = new Dictionary<Link, double>();
            lock (_PredictionValues)
            {
                foreach (var link in _Topology.Links)
                {
                    linkWeight[link] = _PredictionValues[link.Key] / link.ResidualBandwidth;
                }
            }
            EliminateAllLinksNotSatisfy(request.Demand);
            var path = _Dijkstra.GetShortestPath(request.SourceId, request.DestinationId, linkWeight);
            RestoreTopology();
            return path;
        }
    }
}
