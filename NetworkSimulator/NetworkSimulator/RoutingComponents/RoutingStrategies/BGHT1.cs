using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class BGHT1 : RoutingStrategy
    {
        long _WindowSize;
        static long _MaxTime;
        static long _MinTime;
        long _Mode;

        Random r_troj;
        Dijkstra _Dijkstra;

        static Dictionary<Link, List<long>> _LinkReleaseTime;
        static Dictionary<Link, List<double>> _LinkReleaseBandwidth;

        //Fix _RequestICT, khong duyet lai, gay cham
        //static List<long> _RequestICT;
        static List<double> _RequestBandwidth;


        Dictionary<Link, double> _LinkCost;

        //Fix _RequestICT, khong duyet lai, gay cham
        static long sumIncommingTime, lastIncommingTime, countRequest;

        public BGHT1(Topology topology)
            : base(topology)
        {
            r_troj = new Random();
            _LinkReleaseTime = new Dictionary<Link, List<long>>();
            _LinkReleaseBandwidth = new Dictionary<Link, List<double>>();

            //Fix _RequestICT, khong duyet lai, gay cham
            //_RequestICT = new List<long>();

            //Fix _RequestICT, khong duyet lai, gay cham
            sumIncommingTime = 0;
            lastIncommingTime = 0;
            countRequest = 0;

            _RequestBandwidth = new List<double>();
            _LinkCost = new Dictionary<Link, double>();
            _Dijkstra = new Dijkstra(topology);
            Initialize();
        }

        private void Initialize()
        {
            foreach (var link in _Topology.Links)
            {
                _LinkReleaseTime[link] = new List<long>();
                _LinkReleaseBandwidth[link] = new List<double>();
            }
            _MaxTime = _MinTime = 0;
        }

        public double GetTriagleDistribution(double _min, double _max, double _mode)
        {
            double uniform = r_troj.NextDouble();
            double fc;

            fc = (_mode - _min) / (_max - _min);

            if (uniform < fc)
            {
                return _min + Math.Sqrt(uniform * (_max - _min) * (_mode - _min));
            }
            else return _max - Math.Sqrt((1 - uniform) * (_max - _min) * (_max - _mode));

        }



        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);
            //_RequestICT.Add(request.IncomingTime);
            //Fix _RequestICT, khong duyet lai, gay cham
            countRequest++;

            _RequestBandwidth.Add(request.Demand);


            #region Remove value of released requests
            foreach (var link in _Topology.Links)
            {
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (_LinkReleaseTime[link][i] <= request.IncomingTime)
                    {
                        _LinkReleaseTime[link].RemoveAt(i);
                        _LinkReleaseBandwidth[link].RemoveAt(i);
                    }
                }
            }
            #endregion

            #region Compute Window Size by Triangle Distribution
            /*if (_RequestICT.Count == 1)
            {
                _MinTime = _MaxTime = _Mode = request.IncomingTime;
            }
            else
            {
                //Tính tổng thời gian vào của tất cả các request đã có
                //tại sao phải duyệt lại

                //Reset Mode
                _Mode = 0;

                for (int i = 0; i < _RequestICT.Count - 1; i++)
                {
                    _Mode += _RequestICT[i + 1] - _RequestICT[i];
                }

                _Mode /= _RequestICT.Count - 1;

                long tmp = _RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2];
                if (tmp < _MinTime)
                {
                    _MinTime = tmp;
                }
                if (tmp > _MaxTime)
                {
                    _MaxTime = tmp;
                }
            }

            _WindowSize = (long)GetTriagleDistribution(_MinTime, _MaxTime, _Mode);*/
            #endregion

            //Fix _RequestICT, khong duyet lai, gay cham
            #region Compute Window Size by Triangle Distribution
            if (countRequest == 1)
            {
                _MinTime = _MaxTime = _Mode = request.IncomingTime;
                lastIncommingTime = request.IncomingTime;
            }
            else
            {
                //Tính tổng thời gian vào của tất cả các request đã có
                //tại sao phải duyệt lại

                //Reset Mode
                _Mode = 0;

                /*for (int i = 0; i < _RequestICT.Count - 1; i++)
                {
                    _Mode += _RequestICT[i + 1] - _RequestICT[i];
                }*/
                sumIncommingTime += request.IncomingTime - lastIncommingTime;

                //_Mode /= _RequestICT.Count - 1;
                _Mode = sumIncommingTime / (countRequest - 1);


                //long tmp = _RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2];
                long tmp = request.IncomingTime - lastIncommingTime;
                lastIncommingTime = request.IncomingTime;

                if (tmp < _MinTime)
                {
                    _MinTime = tmp;
                }
                if (tmp > _MaxTime)
                {
                    _MaxTime = tmp;
                }
            }

            _WindowSize = (long)GetTriagleDistribution(_MinTime, _MaxTime, _Mode);
            #endregion

            foreach (var link in _Topology.Links)
            {
                double totalBw = 0;
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (_LinkReleaseTime[link][i] <= (request.IncomingTime + _WindowSize) && request.HoldingTime != int.MaxValue)
                    {
                        totalBw += _LinkReleaseBandwidth[link][i];
                    }
                }
                _LinkCost[link] = 1 / (totalBw + link.ResidualBandwidth);

            }

            path = _Dijkstra.GetShortestPath(request.SourceId, request.DestinationId, _LinkCost);

            //Save info new path
            foreach (var link in path)
            {
                long tmpHoldingTime = request.HoldingTime == int.MaxValue ? request.HoldingTime : request.IncomingTime + request.HoldingTime;
                _LinkReleaseTime[link].Add(tmpHoldingTime);
                //Console.WriteLine(request.HoldingTime +"  ------ "+ int.MaxValue);
                _LinkReleaseBandwidth[link].Add(request.Demand);
            }


            RestoreTopology();
            return path;
        }

    }
}
