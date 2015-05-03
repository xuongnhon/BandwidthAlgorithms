using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class FordFulkerson
    {
        private Topology _Topology;

        private BreadthFirstSearch _BFS;

        private const double MaxValue = double.MaxValue;

        public FordFulkerson(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        //private Dictionary<Link, double> _UsingBandwidthCopy;
        private Dictionary<Link, double> _backupResidualBandwidthCopy;

        private void Initialize()
        {
            _BFS = new BreadthFirstSearch(_Topology);
            //_UsingBandwidthCopy = new Dictionary<Link, double>();
            _backupResidualBandwidthCopy = new Dictionary<Link, double>();
        }

        private void BackupTopology()
        {
            //_UsingBandwidthCopy.Clear();
            _backupResidualBandwidthCopy.Clear();
            foreach (var link in _Topology.Links)
            {
                //_UsingBandwidthCopy[link] = link.UsingBandwidth;
                _backupResidualBandwidthCopy[link] = link.ResidualBandwidth;
            }
        }

        private void RestoreTopology()
        {
            foreach (var link in _Topology.Links)
            {
                //link.UsingBandwidth = _UsingBandwidthCopy[link];
                link.ResidualBandwidth = _backupResidualBandwidthCopy[link];
            }
        }

        public double ComputeMaxFlow(Node source, Node destination)
        {
            BackupTopology();
            var maxFlow = MaxFlow(source, destination);
            RestoreTopology();

            return maxFlow;
        }

        private double MaxFlow(Node nodeSource, Node nodeTerminal)
        {
            var flow = 0d;

            var path = _BFS.FindPath(nodeSource, nodeTerminal);

            while (path.Count > 0)
            {
                var minCapacity = MaxValue;
                foreach (var link in path)
                {
                    if (link.ResidualBandwidth < minCapacity)
                        minCapacity = link.ResidualBandwidth;
                }

                if (minCapacity == MaxValue || minCapacity < 0)
                    throw new Exception("minCapacity " + minCapacity);

                AugmentPath(path, minCapacity);

                flow += minCapacity;

                path = _BFS.FindPath(nodeSource, nodeTerminal);
            }

            return flow;
        }

        private void AugmentPath(IEnumerable<Link> path, double minCapacity)
        {
            foreach (var link in path)
            {
                link.ResidualBandwidth -= minCapacity;

                //var nodeR = link.Destination;
                //var linkR = nodeR.Links.Where(i => Object.Equals(i.Destination, link.Source)).FirstOrDefault();
                //if (!Object.Equals(linkR, null))
                //    linkR.ResidualBandwidth += minCapacity;//caoth
            }
        }

        //public List<Link> FindMinCutSet(Node source, Node destination) // not the mincut set in the paper
        public List<Link> FindMinCutSetNotInThePaper(Node source, Node destination)
        {
            BackupTopology();
            double flow = MaxFlow(source, destination);

            var queue = new Queue<Node>();
            var discovered = new HashSet<Node>();

            //var minCut = new List<Node>();
            //var tCut = new List<Node>();
            //var minCutSet = new List<Link>();

            var minCutNodes = new List<Node>();
            var minCutEdges = new List<Link>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (discovered.Contains(current))
                    continue;

                minCutNodes.Add(current);
                discovered.Add(current);

                var edges = current.Links;
                foreach (var edge in edges)
                {
                    var next = edge.Destination;
                    if (edge.ResidualBandwidth <= 0 || discovered.Contains(next))
                        continue;
                    queue.Enqueue(next);
                    minCutEdges.Add(edge);
                }
            }

            // bottleneck as a list of arcs
            var minCutResult = new List<Link>();
            List<int> nodeIds = minCutNodes.Select(node => node.Key).ToList();

            var nodeKeys = new HashSet<int>();
            foreach (Node node in minCutNodes)
                nodeKeys.Add(node.Key);

            var edgeKeys = new HashSet<string>();
            foreach (Link edge in minCutEdges)
                edgeKeys.Add(edge.Key);




            //ParseData();// reset the graph

            RestoreTopology();



            foreach (int id in nodeIds)
            {
                var node = _Topology.Nodes[id];
                var edges = node.Links;
                foreach (Link edge in edges)
                {
                    if (nodeKeys.Contains(edge.Destination.Key))
                        continue;

                    if (edge.Capacity > 0 && !edgeKeys.Contains(edge.Key))
                        minCutResult.Add(edge);
                }
            }


            /*foreach (var node in _Topology.Nodes)
            {
                if (_BFS.FindPath(node, destination).Count == 0 && node != destination)
                    sCut.Add(node);
                else
                    tCut.Add(node);

            }

            foreach (var sNode in sCut)
            {
                foreach (var link in sNode.Links)
                {
                    if (link.ResidualBandwidth <= 0 && tCut.Contains(link.Destination))
                        minCutSet.Add(link);
                }
            }*/



            return minCutResult;
        }
        
        public List<Link> FindMinCutSet(Node source, Node destination) // caoth redo
        //public List<Link> FindMinCutSetOld(Node source, Node destination)
        {
            BackupTopology();
            double flow = MaxFlow(source, destination);

            var sCut = new List<Node>();
            var tCut = new List<Node>();
            var minCutSet = new List<Link>();

            // find sCut
            _BFS.FindPathMarkNode(source, source, sCut);

            // find tCut
            _Topology.Invert();
            _BFS.FindPathMarkNode(destination, destination, tCut);
            _Topology.Invert();

            // find mincut sets
            foreach(Link link in _Topology.Links)
            {
                if (link.ResidualBandwidth == 0 && _backupResidualBandwidthCopy[link] > 0)
                    if (!sCut.Contains(link.Destination) && !tCut.Contains(link.Source))
                        if (_BFS.FindPath(link.Source, link.Destination).Count == 0)
                            minCutSet.Add(link);
            }

            //foreach (var node in _Topology.Nodes)
            //{
            //    if (_BFS.FindPath(node, destination).Count == 0 && node != destination)
            //        sCut.Add(node);
            //    else
            //        tCut.Add(node);

            //}

            //foreach (var sNode in sCut)
            //{
            //    foreach (var link in sNode.Links)
            //    {
            //        if (link.ResidualBandwidth <= 0 && tCut.Contains(link.Destination))
            //            minCutSet.Add(link);
            //    }
            //}

            RestoreTopology();

            return minCutSet;
        }

        public double SubFlow(Node source, Node destination, Link subLink)
        {
            double subflow;
            BackupTopology();
            MaxFlow(source, destination);

            //subflow = _Topology.GetLink(subLink.Source, subLink.Destination).UsingBandwidth; caoth: sai
            subflow = _backupResidualBandwidthCopy[subLink] - subLink.ResidualBandwidth;

            RestoreTopology();
            return subflow;
        }

        public Dictionary<Link, double> SubFlowOfAllLinks(Node source, Node destination, ref double maxflow)
        {
            Dictionary<Link, double> subflows = new Dictionary<Link, double>();
            BackupTopology();
            maxflow = MaxFlow(source, destination);

            foreach (var link in _Topology.Links)
            {
                //double subflow = _Topology.GetLink(link.Source, link.Destination).UsingBandwidth;

                double subflow = _backupResidualBandwidthCopy[link] - link.ResidualBandwidth;
                subflows.Add(link, subflow);
            }

            RestoreTopology();
            return subflows;
        }
    }
}
