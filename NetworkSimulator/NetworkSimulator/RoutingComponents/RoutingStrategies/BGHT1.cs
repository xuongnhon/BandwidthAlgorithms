using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using Troschuetz.Random;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class BGHT1 : RoutingStrategy
    {
        long _WindowSize;
        static long _MaxTime;
        static long _MinTime;
        double _Mode;

        //Random r_troj;
        Dijkstra _Dijkstra;

        //static Dictionary<Link, List<long>> _LinkReleaseTime;
        //static Dictionary<Link, List<double>> _LinkReleaseBandwidth;
        static Dictionary<Link, double> _TotalBandwidth;
        static Dictionary<Link, bool> _NeedToReset;

        //Fix _RequestICT, khong duyet lai, gay cham
        //static List<long> _RequestICT;
        //static List<double> _RequestBandwidth;

        Dictionary<Link, double> _LinkCost;

        //xuongnhon
        static long sumIncommingTime, lastIncommingTime, countRequest;
        NetworkSimulator.SimulatorComponents.ResponseManager _ResponseManager;

        //Cai nay gio se loi
        /*public BGHT1(Topology topology)
            : base(topology)
        {
            r_troj = new Random();
            //_LinkReleaseTime = new Dictionary<Link, List<long>>();
            //_LinkReleaseBandwidth = new Dictionary<Link, List<double>>();

            //Fix _RequestICT, khong duyet lai, gay cham
            //_RequestICT = new List<long>();

            sumIncommingTime = 0;
            lastIncommingTime = 0;
            countRequest = 0;
            _TotalBandwidth = new Dictionary<Link, double>();
            _NeedToReset = new Dictionary<Link, bool>();

            //_RequestBandwidth = new List<double>();
            _LinkCost = new Dictionary<Link, double>();
            _Dijkstra = new Dijkstra(topology);
            Initialize();
        }*/

        public BGHT1(Topology topology, NetworkSimulator.SimulatorComponents.ResponseManager _ResponseManager)
            : base(topology)
        {
            //Khong con dung
            //r_troj = new Random();
            //_LinkReleaseTime = new Dictionary<Link, List<long>>();
            //_LinkReleaseBandwidth = new Dictionary<Link, List<double>>();

            //Fix _RequestICT, khong duyet lai, gay cham
            //_RequestICT = new List<long>();

            //xuongnhon
            sumIncommingTime = 0;
            lastIncommingTime = 0;
            countRequest = 0;
            this._ResponseManager = _ResponseManager;
            _TotalBandwidth = new Dictionary<Link, double>();
            _NeedToReset = new Dictionary<Link, bool>();

            //_RequestBandwidth = new List<double>();
            _LinkCost = new Dictionary<Link, double>();
            _Dijkstra = new Dijkstra(topology);
            Initialize();
        }

        private void Initialize()
        {
            foreach (var link in _Topology.Links)
            {
                //_LinkReleaseTime[link] = new List<long>();
                //_LinkReleaseBandwidth[link] = new List<double>();
                _TotalBandwidth.Add(link, 0);
                _NeedToReset.Add(link, true);
            }
            _MaxTime = _MinTime = 0;
        }

        /*public double GetTriagleDistribution(double _min, double _max, double _mode)
        {
            double uniform = r_troj.NextDouble();
            double fc;

            fc = (_mode - _min) / (_max - _min);

            if (uniform < fc)
            {
                return _min + Math.Sqrt(uniform * (_max - _min) * (_mode - _min));
            }
            else return _max - Math.Sqrt((1 - uniform) * (_max - _min) * (_max - _mode));

        }*/

        #region Old GetPath

        /*public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);
            _RequestICT.Add(request.IncomingTime);
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
            if (_RequestICT.Count == 1)
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

            _WindowSize = (long)GetTriagleDistribution(_MinTime, _MaxTime, _Mode);
            #endregion

            foreach (var link in _Topology.Links)
            {
                double totalBw = 0;
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (request.HoldingTime != int.MaxValue && _LinkReleaseTime[link][i] <= (request.IncomingTime + _WindowSize))
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
        }*/

        #endregion

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);

            countRequest++;

            #region Compute Window Size by Triangle Distribution
            if (countRequest == 1)
            {
                // _MinTime = _MaxTime = _Mode = request.IncomingTime; caoth
                _MinTime = _MaxTime = 0;
                lastIncommingTime = request.IncomingTime;
            }
            else
            {
                _Mode = 0;

                sumIncommingTime += request.IncomingTime - lastIncommingTime;
                _Mode = (double)sumIncommingTime / (countRequest - 1);

                long tmp = request.IncomingTime - lastIncommingTime;
                lastIncommingTime = request.IncomingTime;

                if (tmp < _MinTime || _MinTime == 0) // caoth
                {
                    _MinTime = tmp;
                }
                if (tmp > _MaxTime)
                {
                    _MaxTime = tmp;
                }
            }

            TriangularDistribution _TriangularDistribution = new TriangularDistribution();
            _TriangularDistribution.Beta = _MaxTime; // caoth, max 1st
            _TriangularDistribution.Gamma = _Mode;
            _TriangularDistribution.Alpha = _MaxTime > _MinTime ? _MinTime : (_MaxTime - 0.1); // caoth
            // _WindowSize = (long)_TriangularDistribution.NextDouble(); caoth
            _WindowSize = (long)Math.Ceiling(_TriangularDistribution.NextDouble());

            #endregion

            //Get nhung response sap release
            List<NetworkSimulator.SimulatorComponents.Response> listResponseWillRelease = new List<SimulatorComponents.Response>();
            foreach (var _Response in _ResponseManager.ResponsesToRelease)
            {
                if (_Response.ReleasingTime <= (request.IncomingTime + _WindowSize))
                {
                    listResponseWillRelease.Add(_Response);
                }
                else break;
            }

            //Luu nhung link phai reset khi tinh tong bandwidth
            List<NetworkSimulator.NetworkComponents.Link> listLinkNeedToReset = new List<Link>();
            //Tinh tong bandwidth cua moi link se duoc release
            foreach (var _Response in listResponseWillRelease)
            {
                foreach (var _Link in _Response.Path)
                {
                    //Link co the lap lai trong cac _Response.Path khac, dung _NeedToReset[_Link]
                    //de biet can reset gia tri _TotalBandwidth[_Link]
                    if (_NeedToReset[_Link])
                    {
                        listLinkNeedToReset.Add(_Link);
                        _NeedToReset[_Link] = false;
                        _TotalBandwidth[_Link] = _Response.Request.Demand;
                    }
                    else
                    {
                        _TotalBandwidth[_Link] += _Response.Request.Demand;
                    }
                }
            }

            //Tinh _LinkCost[_Link]
            foreach (var _Link in _Topology.Links)
            {
                //_Link khong co bandwidth se duoc release
                if (_NeedToReset[_Link])
                    _LinkCost[_Link] = 1 / _Link.ResidualBandwidth;
                else
                    _LinkCost[_Link] = 1 / (_TotalBandwidth[_Link] + _Link.ResidualBandwidth);
            }

            //Reset lai _NeedToReset[_Link]
            foreach (var _Link in listLinkNeedToReset)
            { _NeedToReset[_Link] = true; }

            path = _Dijkstra.GetShortestPath(request.SourceId, request.DestinationId, _LinkCost);

            RestoreTopology();
            return path;
        }
    }
}