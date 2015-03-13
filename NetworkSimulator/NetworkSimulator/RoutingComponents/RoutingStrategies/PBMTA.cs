using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;


namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public class PBMTA : RoutingStrategy
    {
        Configuration config = Configuration.GetInstance();

        private long _TMax;
        private long _OldTMax;

        private Dijkstra _Dijkstra;

        private Dictionary<Link, List<long>> _LinkReleaseTime;
        private Dictionary<Link, List<double>> _LinkReleaseBandwidth;

        private Dictionary<Link, double> _LinkCost;

        public PBMTA(Topology topology)
            : base(topology)
        {
            _LinkReleaseTime = new Dictionary<Link, List<long>>();
            _LinkReleaseBandwidth = new Dictionary<Link, List<double>>();

            _LinkCost = new Dictionary<Link, double>();

            _Dijkstra = new Dijkstra(topology);

            Initialize();

            //Compute TMax;
            Thread t = new Thread(new ThreadStart(ComputeTMax));
            t.Start();
            
        }

        public void ComputeTMax()
        {
            while (true)
            {
                _OldTMax = _TMax;
                if (_TMax != 0)
                {
                    lock (_LinkReleaseTime)
                    {
                        foreach (var link in _Topology.Links)
                        {
                            if (_LinkReleaseTime.Keys.Contains(link))
                            {

                                long minreleasetime = int.MaxValue;

                                //find min released time of each link
                                foreach (var time in _LinkReleaseTime[link])
                                {
                                    //bỏ những request đã release: time > _TMax
                                    if (time >= _TMax && minreleasetime > time)
                                    {
                                        minreleasetime = time;
                                    }
                                }

                                //Select max released time of all min released time found
                                if (_TMax < minreleasetime)
                                {
                                    _TMax = minreleasetime;
                                }
                            }
                        }
                    }                    
                }
                //Console.WriteLine("aa" + _TMax);
                
                int waittime = (int)(_TMax - _OldTMax) * config.TimerInterval;
                Thread.Sleep(waittime);
            }
        }

        private void Initialize()
        {
            foreach (var link in _Topology.Links)
            {
                _LinkReleaseTime[link] = new List<long>();
                _LinkReleaseBandwidth[link] = new List<double>();
            }
            _TMax = int.MaxValue;
            _OldTMax = 0;
        }

        
        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> path = new List<Link>();
            EliminateAllLinksNotSatisfy(request.Demand);
            //Console.WriteLine(incomingTime * config.TimerInterval);
            //Console.WriteLine(_TMax);

            if (_TMax == 0)
            {
                //_TMax = incomingTime + holdingTime;
                _TMax = int.MaxValue;
            }

            foreach (var link in _Topology.Links)
            {
                double totalBw = 0;
                for (int i = 0; i < _LinkReleaseTime[link].Count; i++)
                {
                    //bỏ những request đã releaase _LinkReleaseTime[link][i] >= incomingTime
                    if (_LinkReleaseTime[link][i] >= request.IncomingTime && _LinkReleaseTime[link][i] <= _TMax)
                    {
                        totalBw += _LinkReleaseBandwidth[link][i];
                    }
                }
                //if (totalBw != 0)
                //{
                //    _LinkCost[link] = 1 / totalBw + 1 / link.ResidualBandwidth;
                //}
                //else
                //    _LinkCost[link] = 1 / link.ResidualBandwidth;    

                _LinkCost[link] = 1 / (totalBw + link.ResidualBandwidth);
            }

            path = _Dijkstra.GetShortestPath(request.SourceId, request.DestinationId, _LinkCost);

            //Save info new path
            lock (_LinkReleaseTime)
            {
                foreach (var link in path)
                {
                    _LinkReleaseTime[link].Add(request.IncomingTime + request.HoldingTime);
                    _LinkReleaseBandwidth[link].Add(request.Demand);
                }
            }


            RestoreTopology();
            return path;
        }
    }
}
