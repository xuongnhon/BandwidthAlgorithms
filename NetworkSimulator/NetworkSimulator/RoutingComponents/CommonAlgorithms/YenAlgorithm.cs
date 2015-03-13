using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class YenAlgorithm
    {
        private Topology _Topology;

        private Topology _BackupTopo;

        private Dictionary<Link, double> _LinkCost;

        private Dictionary<List<Link>, double> _HeapB;

        private List<List<Link>> _ResultA;

        private int _K;

        private Dictionary<Link, double> _ResidualBandwidthCopy;

        //private Dictionary<List<Link>, double> CostPath;

        private Dijkstra _Dijkstra;

        public YenAlgorithm(Topology topology, Dictionary<Link, double> linkCost, int k)
        {
            _BackupTopo = topology;
            _Topology = topology;
            
            _K = k;

            _LinkCost = new Dictionary<Link, double>();
            _LinkCost = linkCost;

            _ResultA = new List<List<Link>>();

            _HeapB = new Dictionary<List<Link>, double>();

            _ResidualBandwidthCopy = new Dictionary<Link, double>();

            foreach (Link link in _Topology.Links)
                _ResidualBandwidthCopy[link] = link.ResidualBandwidth;

            _Dijkstra = new Dijkstra(_Topology);
        }

        private double GetCostOfPath(List<Link> Path)
        {
            double cost = 0;
            foreach (var link in Path)
            {
                cost += _LinkCost[link];
            }
            return cost;
        }


        private Node FindSpurNode(List<Node> NodeList, int position)
        {
            return NodeList[position];
        }

        private List<Node> FindRootPath(List<Node> NodeList, int start, int end)
        {
            List<Node> result = new List<Node>();
            for (int i = start; i <= end; i++)
            {
                result.Add(NodeList[i]);
            }
            return result;
        }

        private void RemoveLink(Link removedLink)
        {
            

            foreach (Link link in _Topology.Links)
            {
                if (link == removedLink)
                    link.ResidualBandwidth = 0;
            }
        }

        private void ResetTopology()
        {
            //_Topology = _BackupTopo.Clone();
            foreach (var link in _Topology.Links)
            {
                link.ResidualBandwidth = _ResidualBandwidthCopy[link];
            }
        }

        private List<Link> Join2Path(List<Node> RootPath, List<Link> SpurPath)
        {
            List<Link> result = new List<Link>();
            if (RootPath.Count>1)
            {
                for (int i = 0; i < RootPath.Count-1; i++)
                {
                    result.Add(_Topology.GetLink(RootPath[i],RootPath[i+1]));
                }
            }

            foreach (var link in SpurPath)
	        {
		        result.Add(link);
	        }

            return result;
        }

        private bool IsNodeListEqual(List<Node> A, List<Node> B)
        {
            if (A.Count == B.Count)
            {
                for (int i = 0; i < A.Count; i++)
                    if (A[i].Key != B[i].Key)
                    {
                        return false;
                    }
            }
            else
            {
                return false;
            }
            return true;

        }


        private List<Node> ConvertFromPathToNodeList(List<Link> Path)
        {
            List<Node> nodeList = new List<Node>();
            foreach (var item in Path)
            {
                nodeList.Add(item.Source);
            }
            if (Path.Count > 0)
            {
                nodeList.Add(Path[Path.Count - 1].Destination);
            }

            return nodeList;
        }

        public List<List<Link>> GetKShortestPath(Node Source, Node Destination)
        {
            List<Link> result = _Dijkstra.GetShortestPath(Source, Destination, _LinkCost);
            _ResultA.Add(result);
            _HeapB[result] = double.MaxValue;
            List<Node> NodeListFromResult = new List<Node>();

            int k = 1;
            while (k < _K)
            {
                
                //convert list link to list node (done)
                NodeListFromResult = ConvertFromPathToNodeList(_ResultA[k - 1]);

                for (int i = 0; i < NodeListFromResult.Count -1 ; i++)
                {
                    //Find SpurNode : _Result[k-1].node[i] (done)
                    Node SpurNode = FindSpurNode(NodeListFromResult, i);

                    //Find RootPath : a list of Nodes from 0 -> i (done)
                    List<Node> RootPath = new List<Node>();
                    RootPath = FindRootPath(NodeListFromResult, 0, i);

                    #region Remove the links that are part of the previous shortest paths which share the same root path.
                    foreach (var path in _ResultA)
                    {
                        //convert List <Link> item -> List<Node> item
                        List<Node> PathInResult = new List<Node>();
                        PathInResult = ConvertFromPathToNodeList(path);

                        List<Node> RootPathOfP = new List<Node>();
                        if (PathInResult.Count - 1< i)
                        {
                            RootPathOfP = FindRootPath(PathInResult, 0, PathInResult.Count-1);
                        }
                        else
                            RootPathOfP = FindRootPath(PathInResult, 0, i);

                        //If p.Nodes(0,i) == RootPath remove (i,i+1) (done)
                        if (IsNodeListEqual(RootPath, RootPathOfP))
                        {
                            RemoveLink(_Topology.GetLink(PathInResult[i], PathInResult[i + 1]));
                        }
                    }
                    #endregion

                    //Find spurPath from spurNode to des
                    _Dijkstra = new Dijkstra(_Topology);
                    List<Link> SpurPath = _Dijkstra.GetShortestPath(SpurNode, Destination, _LinkCost);

                    List<Link> totalPath = new List<Link>();
                    //If have spurpath, totalPath = Join(RootPath, SpurPath)
                    if (SpurPath.Count > 0)
                    {
                        totalPath = Join2Path(RootPath, SpurPath);
                    }

                    bool flag = true;
                    //If B[totalPath] isn't existed, add totalpath with Cost
                    foreach (var item in _HeapB.Keys)
                        if (!item.Except(totalPath).Any() && !totalPath.Except(item).Any())
                        {
                            flag = false;
                            break;
                        }
                    if (flag && totalPath.Count>0)
                    {
                        _HeapB[totalPath] = GetCostOfPath(totalPath);
                    }

                    //Reset everything
                    ResetTopology();
                }

                //Find min cost in HeapB to add ResultA
                double minCost = double.MaxValue;
                List<Link> FinalPathResult = new List<Link>();
                foreach (var item in _HeapB)
                    if (item.Value < minCost)
                    {
                        minCost = item.Value;
                        FinalPathResult = item.Key;
                    }
                if (minCost != double.MaxValue)
                {
                    _ResultA.Add(FinalPathResult);
                    _HeapB[FinalPathResult] = double.MaxValue;
                    k++;
                }
                else break;
            }
            return _ResultA;
        }

    }
}

