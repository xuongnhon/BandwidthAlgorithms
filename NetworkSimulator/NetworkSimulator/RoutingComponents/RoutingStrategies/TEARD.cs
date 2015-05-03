using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.CommonObjects;
using System.Diagnostics;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class TEARD : RoutingStrategy
    {
        #region Fields
        private Dijkstra _Dijkstra;

        private AllSimplePaths _ASP;

        // private List<IEPair> _IEList = new List<IEPair>();

        private static Dictionary<Link, double> _CostLink;

        //private Dictionary<Link, double> _CIe;

        private static Dictionary<Link, double> _CReq; // = _Probability * _CIeLink

        private static Dictionary<Link, double> _Load;
        private static Dictionary<Link, double> _CLink;
        private static Dictionary<Link, double> _UsedLinkBw;

        private static Dictionary<Link, double> _UsedLinkCount;

        //private static List<Request> _PastData;

        private static Dictionary<IEPair, double> _Probability;
        private static Dictionary<IEPair, Dictionary<Link, double>> _CIeLink; // count link criticality per IE

        private static Dictionary<IEPair, double> _IECount;
        private static double _TotalRequest = 0;

        private static double K1, K2, K3;

        private const double MAX = 1000;

        private static double _TotalPath = 0;


        #endregion

        public TEARD(Topology topology)
            : base(topology)
        {
            Initialize();
        }

        public TEARD(Topology topology, double _K1, double _K2, double _K3)
            : base(topology)
        {
            Initialize();
            K1 = _K1;
            K2 = _K2;
            K3 = _K3;
        }

        private void Initialize()
        {
            _Dijkstra = new Dijkstra(_Topology);

            _ASP = new AllSimplePaths(_Topology);

            //_IEList = _Topology.IEPairs;

            _CostLink = new Dictionary<Link, double>();

            //_CIe = new Dictionary<Link, double>();

            _CReq = new Dictionary<Link, double>();

            _Load = new Dictionary<Link, double>();

            _CLink = new Dictionary<Link, double>();

            _UsedLinkCount = new Dictionary<Link, double>();

            // for caculator Support Ie
            //_PastData = new List<Request>();

            _Probability = new Dictionary<IEPair, double>();
            _CIeLink = new Dictionary<IEPair, Dictionary<Link, double>>();
            _IECount = new Dictionary<IEPair, double>();


            Offline();

            K1 = 0.3;
            K2 = 0.4;
            K3 = 0.3;
        }

        public void Offline()
        {
            foreach (var link in _Topology.Links)
            {
                //_CostLink[link] = double.Epsilon;

                //_CIe[link] = 0;

                // gan o ham GetPath
                //_CReq[link] = 0;
                //_Load[link] = 0;

                _UsedLinkCount[link] = 0;
            }

            //Stopwatch _Stopwatch = new Stopwatch();
            //_Stopwatch.Restart();

            foreach (var ie in _Topology.IEPairs)
            {
                //_Probability[ie] = 0.25;

                //_Probability[ie] = 25;
                _Probability[ie] = 0;
                _IECount[ie] = 0;

                // haiz: bad for reroute
                _CIeLink[ie] = new Dictionary<Link, double>();
                foreach (var link in _Topology.Links)
                {
                    _CIeLink[ie][link] = 0;
                }

                List<List<Link>> paths = _ASP.GetPaths(ie.Ingress, ie.Egress);
                foreach (var path in paths)
                {
                    foreach (var link in path)
                    {
                        _CIeLink[ie][link] += 1;
                    }
                }

                // chia _CIeLink cho tong so paths cua cap ie, de dam bao ti le nhu 2 so con lai
                foreach (Link link in _Topology.Links)
                {
                    //_CIeLink[ie][link] = _CIeLink[ie][link] *100 / paths.Count;
                    _CIeLink[ie][link] = _CIeLink[ie][link] / paths.Count;

                }
            }

            //_Stopwatch.Stop();
            //Console.WriteLine("Count  _CIeLink[ie][link]: " + _Stopwatch.ElapsedMilliseconds);
            // ~ 25ms for MIRA topo 
            // :( too long comparing to ?
        }

        #region

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            // Caculator at Requesting time            
            _TotalRequest += 1;
            IEPair iepair = (from ie in _Topology.IEPairs
                             where ie.Ingress.Key == request.SourceId && ie.Egress.Key == request.DestinationId
                             select ie).First();
            _IECount[iepair] += 1;

            foreach (var ie in _Topology.IEPairs)
            {
                //_Probability[ie] = _IECount[ie] * 100 / _TotalRequest;
                _Probability[ie] = _IECount[ie] / _TotalRequest;
            }

            // Caculator cost for link
            foreach (var link in _Topology.Links)
            {
                //_Load[link] = link.UsingBandwidth *100 / link.Capacity;
                //_Load[link] = link.UsingBandwidth * 100 / link.ResidualBandwidth;
                _Load[link] = link.UsingBandwidth / link.ResidualBandwidth;

                //Console.WriteLine("_Load[link]" + _Load[link]);

                _CReq[link] = 0;
                foreach (var ie in _Topology.IEPairs)
                {
                    _CReq[link] += _Probability[ie] * _CIeLink[ie][link];
                    // Console.WriteLine("Probability[ie]" + _Probability[ie] + "  _CIeLink[ie][link]" + _CIeLink[ie][link]);
                }
                //_CReq[link] *= 100;

                //Console.WriteLine("_CReq[link]" + _CReq[link]);

                if (_TotalPath == 0)
                    _CLink[link] = 0;
                else
                {
                    //_CLink[link] = _UsedLinkCount[link] * 100 / _TotalPath;
                    _CLink[link] = _UsedLinkCount[link] / _TotalPath;
                }



                //Console.WriteLine("_CLink[link]" + _CLink[link]);

                // use k1 k2 k3, công thức cũ
                _CostLink[link] = (K1 * _CReq[link]) + (K2 * _Load[link]) + (K3 * _CLink[link]) + double.Epsilon;                


                //công thức mới [1] * [3]
                //  _CostLink[link] = K1 * (_CReq[link] * _CLink[link]) + K2 * _Load[link];
                //_CReq[link] = _CReq[link] * _CLink[link];//To normalize

                //công thức mới [1] * [2]
                //_CostLink[link] = K1 * (_CReq[link] * _Load[link]) + K2 * _CLink[link] + 1;

                //công thức mới [1] * [2] * [3] * 10^8 + 1, 10^8 là gt tốt nhất
                // _CostLink[link] = (_CReq[link] * _Load[link] * _CLink[link]) * 100000000 + 1;

                //_CostLink[link] = (100000000 / link.Capacity * _Load[link]) + 1; // test bcra here :)) fuck every one

                //Console.WriteLine("test = " + _CReq[link] * _Load[link] * _CLink[link]);
                //Console.WriteLine("_CostLink[link]" + _CostLink[link]);
            }



            #region Normalized

            //double DeltaCReq, DeltaLoad;

            //double MinCReq = double.MaxValue;
            //double MaxCReq = double.MinValue;
            //double MinLoad = double.MaxValue;
            //double MaxLoad = double.MinValue;
            //foreach (var link in  _Topology.Links)
            //{
            //    if (MinCReq > _CReq[link])
            //        MinCReq = _CReq[link];
            //    if (MaxCReq < _CReq[link])
            //        MaxCReq = _CReq[link];
            //    if (MinLoad > _Load[link])
            //        MinLoad = _Load[link];
            //    if (MaxLoad < _Load[link])
            //        MaxLoad = _Load[link];
            //}

            //DeltaCReq = MaxCReq - MinCReq;
            //DeltaLoad = MaxLoad - MinLoad;


            //foreach (var link in _Topology.Links)
            //{
            //    //_CostLink[link] = NormalizedCReq(_CReq[link]) + NormalizedLoad(_Load[link]) + NormalizedCLnk(_CLink[link]);

            //    //công thức mới[1]*[3]
            //    _CReq[link] = DeltaCReq > 0 ? ((_CReq[link] - MinCReq) * MAX / DeltaCReq) : MAX;
            //    _Load[link] = DeltaLoad > 0 ? ((_Load[link] - MinLoad) * MAX /DeltaLoad) : MAX;
            //    _CostLink[link] = K1* _CReq[link] + K2 * _Load[link];

            //}
            #endregion


            // Use dijsktra to get path
            EliminateAllLinksNotSatisfy(request.Demand);
            var resultPath = _Dijkstra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _CostLink);
            RestoreTopology();

            // Caculator when finish findpath
            if (resultPath.Count > 0)// neu tim dc dg 
            {
                _TotalPath += 1;
                foreach (var l in resultPath)
                {
                    _UsedLinkCount[l] += 1;
                }
            }

            return resultPath;
        }

        #endregion


    }
}