using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class BGHT2 : RoutingStrategy
    {
        long _WindowSize;

        Dijkstra _Dijkstra;

        Dictionary<Link, List<long>> _LinkReleaseTime;
        Dictionary<Link, List<double>> _LinkReleaseBandwidth;



        Dictionary<Link, double> _LinkCost;

        public BGHT2(Topology topology)
            : base(topology)
        {
            _LinkReleaseTime = new Dictionary<Link, List<long>>();
            _LinkReleaseBandwidth = new Dictionary<Link, List<double>>();
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
        }


        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);


            //long minreleasetime = incomingTime + holdingTime;
            long minreleasetime = long.MaxValue;

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
                    else
                    {
                        if (_LinkReleaseTime[link][i] < minreleasetime)
                        {
                            minreleasetime = _LinkReleaseTime[link][i];
                        }
                    }
                }
            }
            #endregion

            _WindowSize = minreleasetime;

        //   Console.WriteLine("_WindowSize = " + _WindowSize);

            foreach (var link in _Topology.Links)
            {
                double totalBw = 0;
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    if (request.HoldingTime != int.MaxValue && _LinkReleaseTime[link][i] <= request.IncomingTime + _WindowSize)
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
                _LinkReleaseBandwidth[link].Add(request.Demand);
            }


            RestoreTopology();
            return path;
        }


    }
}
