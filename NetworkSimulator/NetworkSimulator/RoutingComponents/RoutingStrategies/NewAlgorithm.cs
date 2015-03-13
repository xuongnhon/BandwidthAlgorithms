using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class NewAlgorithm : RoutingStrategy
    {
        long _WindowSize;
        long _MaxTime;
        long _MinTime;
        long _Mode;

        Random r_troj;
        Dijkstra _Dijkstra;

        Dictionary<Link, List<long>> _LinkReleaseTime;
        Dictionary<Link, List<double>> _LinkReleaseBandwidth;

        List<long> _RequestICT;
        List<double> _RequestBandwidth;


        Dictionary<Link, double> _LinkCost; 

        public NewAlgorithm(Topology topology)
            : base(topology)
        {
            r_troj = new Random();
            _LinkReleaseTime = new Dictionary<Link, List<long>>();
            _LinkReleaseBandwidth = new Dictionary<Link, List<double>>();
            _RequestICT = new List<long>();
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

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long holdingTime)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(bandwidth);

            _RequestICT.Add(incomingTime);
            _RequestBandwidth.Add(bandwidth);

            #region Remove value of released requests
            foreach (var link in _Topology.Links)
            {
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (_LinkReleaseTime[link][i] <= incomingTime)
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
                _MinTime = _MaxTime = _Mode = incomingTime;
            }
            else
            {
                for (int i = 0; i < _RequestICT.Count - 1; i++)
                {
                    _Mode += _RequestICT[i + 1] - _RequestICT[i];
                }
                _Mode /= _RequestICT.Count - 1;
                if (_RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2] <  _MinTime )
                {
                    _MinTime = _RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2];
                }
                if (_RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2] >  _MaxTime )
                {
                    _MaxTime = _RequestICT[_RequestICT.Count - 1] - _RequestICT[_RequestICT.Count - 2];
                }
            }

            _WindowSize = (long)GetTriagleDistribution(_MinTime, _MaxTime,  _Mode);
            #endregion

            foreach (var link in _Topology.Links)
            {
                double totalBw = 0;
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (_LinkReleaseTime[link][i] <= incomingTime + _WindowSize)
                    {
                        totalBw += _LinkReleaseBandwidth[link][i];
                    }
                }
                _LinkCost[link] = 1 / (totalBw + link.ResidualBandwidth);
            }

            path = _Dijkstra.GetShortestPath(sourceId, destinationId, _LinkCost);
            
            //Save info new path
            foreach (var link in path)
            {
                _LinkReleaseTime[link].Add(incomingTime + holdingTime);
                _LinkReleaseBandwidth[link].Add(bandwidth);
            }

            
            RestoreTopology();
            return path;
        }

        public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        {
            throw new NotImplementedException("Not Implemented Exception");
        }
    }
}
