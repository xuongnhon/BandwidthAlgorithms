using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.CommonObjects;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    class POOA : RoutingStrategy
    {
        #region Fields
        private int _K;

        private List<IEPair> _IEList;

        private Dictionary<IEPair, List<List<Link>>> _AllKPaths;

        private Dictionary<IEPair, List<int>> _XValues;

        private Dictionary<Link, double> _LinkCost;

        private Dictionary<Link, double> _Criticality;

        private Dictionary<List<Link>, double> _PathCost;

        private YenAlgorithm _YenAlgorithm;

        private FordFulkerson _FordFulkerson;

        private Dictionary<IEPair, int> _RemainingKTime;
        #endregion

        #region Properties
        public int K
        {
            get { return _K; }
            set { _K = value; }
        }
        #endregion

        public POOA(Topology topology, int K) 
            : base(topology) {
            _K = K;
            Initialize();
        }

        private void Initialize()
        {
            
            _LinkCost = new Dictionary<Link, double>();
            _IEList = new List<IEPair>();
            _AllKPaths = new Dictionary<IEPair, List<List<Link>>>();
            _XValues = new Dictionary<IEPair, List<int>>();
            _FordFulkerson = new FordFulkerson(_Topology);
            _RemainingKTime = new Dictionary<IEPair, int>();
            _PathCost = new Dictionary<List<Link>, double>();
            _Criticality = new Dictionary<Link, double>();

            foreach (Link link in _Topology.Links)
            {
                _LinkCost[link] = 1;
                _Criticality[link] = 0;
            }

            // Ingress Egress list
            _IEList = _Topology.IEPairs;
            foreach (var iepair in _IEList)
            {
                _RemainingKTime[iepair] = 0;
            }

            OfflinePhase();
        }

        public void OfflinePhase()
        {

            #region  Find k shortest paths foreach i-e pair and init for X Value
            foreach (var iepair in _IEList)
            {
                _YenAlgorithm = new YenAlgorithm(_Topology, _LinkCost, _K);
                _AllKPaths[iepair] = _YenAlgorithm.GetKShortestPath(iepair.Ingress, iepair.Egress);
                _XValues[iepair] = new List<int>();
                foreach (var path in _AllKPaths[iepair])
                {
                    _PathCost[path] = 0;
                    
                    (_XValues[iepair]).Add(0);
                }

            }
            #endregion

            #region Find maxflow foreach i-e pair
            double Maxflow = 0;
            foreach (var iepair in _IEList)
            {
                Maxflow += _FordFulkerson.ComputeMaxFlow(iepair.Ingress, iepair.Egress);
            }
            #endregion

            #region Compute Cost of each link
            foreach (var link in _Topology.Links)
            {
                
                foreach (var iepair in _IEList)
                {
                    //TODO: code subflow for ford fulkerson
                    _Criticality[link] += _FordFulkerson.SubFlow(iepair.Ingress,iepair.Egress,link); 
                }
                _Criticality[link] /= Maxflow; 
            }
            #endregion
        }

        private List<Link> LearningStage(IEPair iepair, double Bandwidth)
        {
            List<Link> Result = new List<Link>();

            //Compute Cost of path
            foreach (var path in _AllKPaths[iepair])
            {
                foreach (var link in path)
                {
                    if (_Topology.GetLink(link.Source, link.Destination).ResidualBandwidth > 0)
                    {
                        _PathCost[path] += _Criticality[link] / _Topology.GetLink(link.Source, link.Destination).ResidualBandwidth;
                    }
                    else _PathCost[path] = double.MaxValue;
                }
            }

            //Get path having min Cost and satifying demand
            double minCost = double.MaxValue;
            Result = new List<Link>();
            int count = 0;
            foreach (var path in _AllKPaths[iepair])
                if (_PathCost[path] < minCost && GetBandwidthOfPath(path) >= Bandwidth)
                {
                    Result = path;
                    minCost = _PathCost[path];
                    count++;
                }
            
            //Increase reward value 
            if (Result.Count > 0)
            {
                _XValues[iepair][count-1]++;
            }

            bool flag = true;
            //Sort reward list
            for (int i = 0; i < _XValues[iepair].Count-1; i++)
                for (int j = 0; j < _XValues[iepair].Count; j++)
                    if (_XValues[iepair][i] < _XValues[iepair][j])
                    {
                        flag = false;
                        
                        //swap _AllKPaths
                        List<Link> tmpPath = new List<Link>();
                        tmpPath = _AllKPaths[iepair][i];
                        _AllKPaths[iepair][i] = _AllKPaths[iepair][j];
                        _AllKPaths[iepair][j] = tmpPath;

                        //swap _XValues
                        int tmpValue = _XValues[iepair][i];
                        _XValues[iepair][i] = _XValues[iepair][j];
                        _XValues[iepair][j] = tmpValue;

                    }

            //Increase remaining order time
            if (flag)
            {
                _RemainingKTime[iepair]++;
            }
            else
            {
                _RemainingKTime[iepair] = 0;
                //Reset race
                for (int i = 0; i < _AllKPaths[iepair].Count; i++)
                {
                    _XValues[iepair][i] = 0;
                }
            }
            return Result;
        }

        private List<Link> PostLearningStage(IEPair iepair, double Bandwidth)
        {
            List<Link> Result = new List<Link>();

            for (int i = 0; i < _AllKPaths[iepair].Count; i++)
            {
                if (GetBandwidthOfPath(_AllKPaths[iepair][i]) >= Bandwidth)
                {
                    Result = _AllKPaths[iepair][i];
                    break;
                }
            }

            return Result;
        }

        private double GetBandwidthOfPath(List<Link> Path)
        {
            double Bandwidth = double.MaxValue;
            foreach (var link in Path)
                if (link.ResidualBandwidth < Bandwidth)
                {
                    Bandwidth = _Topology.GetLink(link.Source, link.Destination).ResidualBandwidth;
                }
            return Bandwidth;
        }


        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime)
        //{
        //    throw new NotImplementedException("Not Implemented Exception");
        //}

        //public override List<Link> GetPath(int sourceId, int destinationId, double bandwidth)
        //{
        //    List<Link> Result = new List<Link>();
            
        //    IEPair iepair = (from ie in _IEList
        //                        where ie.Ingress.Key == sourceId && ie.Egress.Key == destinationId
        //                        select ie).First();
            
        //    //Learning stage
        //    if (_RemainingKTime[iepair] < _K)
        //    {
        //        Result = LearningStage(iepair, bandwidth);
        //    }
            
        //    //Post-learning stage
        //    else
        //    {
        //        Result = PostLearningStage(iepair, bandwidth);
        //    }

        //    return Result;
        //}

        public override List<Link> GetPath(SimulatorComponents.Request request)
        {
            List<Link> Result = new List<Link>();

            IEPair iepair = (from ie in _IEList
                             where ie.Ingress.Key == request.SourceId  && ie.Egress.Key == request.DestinationId
                             select ie).First();

            //Learning stage
            if (_RemainingKTime[iepair] < _K)
            {
                Result = LearningStage(iepair, request.Demand);
            }

            //Post-learning stage
            else
            {
                Result = PostLearningStage(iepair, request.Demand);
            }

            return Result;
        }
    }
}
