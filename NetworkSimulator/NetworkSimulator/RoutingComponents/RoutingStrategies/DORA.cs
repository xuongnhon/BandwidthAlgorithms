using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.CommonObjects;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class DORA : RoutingStrategy
    {        
        private Dijkstra _Dijkstra;
        private DisjointPaths _DisjointPaths;        

        List<IEPair> _IEList = new List<IEPair>();
        public Dictionary<IEPair, List<Link>> _LinkSet;
        private Dictionary<IEPair, Dictionary<Link, int>> _Criticality;
        private Dictionary<Link, double> _CostList;
        
        private int _MAX;
        private double _BWP;

       

        public double BWP
        {
            set { this._BWP = value; }
        }

        public DORA(Topology topology)
            : base(topology)
        {
            _MAX = 1000;
            _BWP = 0.5;
            Initialize();
        }

        
        private void Initialize()
        {            
            _Dijkstra = new Dijkstra(_Topology);
            _DisjointPaths = new DisjointPaths(_Topology);

            // Get IE List
            _IEList = _Topology.IEPairs;

            _Criticality = new Dictionary<IEPair, Dictionary<Link, int>>();
            _LinkSet = new Dictionary<IEPair, List<Link>>();
            _CostList = new Dictionary<Link, double>();

            foreach (var ie in _IEList)
            {
                _LinkSet[ie] = new List<Link>();
            }

            Offline();
        }

        public void Offline()
        {
            foreach (var iepair in _IEList)
            {
                _LinkSet[iepair] = new List<Link>();
                _Criticality[iepair] = new Dictionary<Link,int>();
                _LinkSet[iepair] = _DisjointPaths.GetDisjointPaths(iepair.Ingress, iepair.Egress);
                foreach (var link in _Topology.Links)
                {
                    _Criticality[iepair][link] = 0;
                }
            }

           
            #region Compute Criticality[l] foreach i-e pair
            foreach (var iepair in _IEList)
            {
                //Console.WriteLine("AA");
                List<Link> tmp = new List<Link>();
                tmp = _LinkSet[iepair];
                foreach (var link in tmp)
                {
                    _Criticality[iepair][link] -= 1;

                    foreach (var ie in _IEList)
                    {
                        if (ie != iepair)
                        {
                            if (_LinkSet[ie].Contains(link))
                            {
                                _Criticality[iepair][link] += 1;
                            }
                        }
                    }
                }
        
            }
            #endregion
        }

        private double NormalizeCriticality(IEPair iepair, int value)
        {
            int Min = int.MaxValue;
            int Max = int.MinValue;
            foreach (var val in _Criticality[iepair].Values)
            {
                if (val > Max)
                {
                    Max = val;
                }
                if (val < Min)
                {
                    Min = val;
                }
            }
            
            int Delta = Max - Min;
            if (Delta != 0)
            {
                return ((double)(value - Min) * _MAX) / Delta;    
            }
            return _MAX;
        }


        private double NormalizeResidualBw(IEPair iepair, double value)
        { 
            //Find Min value residual bw of list link iepair
            double MinBw = double.MaxValue;
            double MaxBw = 0;
            foreach (var link in _Topology.Links)
            {
                if (link.ResidualBandwidth < MinBw )
                {
                    MinBw = link.ResidualBandwidth;
                }
                if (link.ResidualBandwidth > MaxBw)
                {
                    MaxBw = link.ResidualBandwidth;
                }
            }

            double delta = MaxBw - MinBw;
            if (delta != 0)
            {
                return (MinBw * (MaxBw - value) * _MAX) / (value * delta);   
            }
            return 1;
            
        }


        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}


        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    EliminateAllLinksNotSatisfy(bandwidth);

        //    IEPair iepair = (from ie in _IEList
        //                     where ie.Ingress.Key == sourceId && ie.Egress.Key == destinationId
        //                     select ie).First();

        //    #region Compute Cost
        //        foreach (var link in _Topology.Links)
        //        {
        //            _CostList[link] = (1 - _BWP) * NormalizeCriticality(iepair,_Criticality[iepair][link]) + _BWP * NormalizeResidualBw(iepair, link.ResidualBandwidth);
        //        }
        //    #endregion

        //    // Use dijsktra to get path
        //    var resultPath = _Dijkstra.GetShortestPath(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId], _CostList);

        //    RestoreTopology();
           
        //    return resultPath;
        //}

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            EliminateAllLinksNotSatisfy(request.Demand);

            IEPair iepair = (from ie in _IEList
                             where ie.Ingress.Key == request.SourceId && ie.Egress.Key == request.DestinationId
                             select ie).First();

            #region Compute Cost
            foreach (var link in _Topology.Links)
            {
                _CostList[link] = (1 - _BWP) * NormalizeCriticality(iepair, _Criticality[iepair][link]) + _BWP * NormalizeResidualBw(iepair, link.ResidualBandwidth);
            }
            #endregion

            // Use dijsktra to get path
            var resultPath = _Dijkstra.GetShortestPath(_Topology.Nodes[request.SourceId], _Topology.Nodes[request.DestinationId], _CostList);

            RestoreTopology();

            return resultPath;
        }

    }
}

