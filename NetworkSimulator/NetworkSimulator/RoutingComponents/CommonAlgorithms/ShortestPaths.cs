using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class ShortestPaths
    {
        private Topology _Topology;

        private Dictionary<Node, List<Node>> _Previous;

        private Dictionary<Node, double> _Distance;

        private const double MaxValue = double.MaxValue;

        public ShortestPaths(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            _Distance = new Dictionary<Node, double>();
            _Previous = new Dictionary<Node, List<Node>>();
        }

        public List<List<Link>> GetShortestPaths(Node source, Node destination, Dictionary<Link, double> cost)
        {
            // Initialize
            _Distance.Clear();
            _Previous.Clear();
            foreach (var node in _Topology.Nodes)
            {
                _Distance[node] = MaxValue;
                _Previous[node] = new List<Node>();
            }
            _Distance[source] = 0;

            var T = new List<Node>(_Topology.Nodes);

            while (T.Count > 0)
            {
                // Find node u within T set, where d[u] = min{d[z]:z within T
                var u = T.First();
                foreach (var node in T)
                {
                    if (_Distance[node] < _Distance[u])
                        u = node;
                }

                T.Remove(u);

                // Browse all adjacent node to update distance from s.
                foreach (var link in u.Links.Where(l => l.ResidualBandwidth > 0))
                {
                    var v = link.Destination;
                    if (_Distance[v] > _Distance[u] + cost[link])
                    {
                        _Distance[v] = _Distance[u] + cost[link];
                        _Previous[v].Clear();
                        _Previous[v].Add(u);
                    }
                    else if (_Distance[v] == _Distance[u] + cost[link])
                    {
                        _Distance[v] = _Distance[u] + cost[link];
                        _Previous[v].Add(u);
                    }
                }
            }

            return GetPaths( source, destination);
        }

        public List<List<Link>> GetShortestPaths(Node source, Node destination)
        {
            Dictionary<Link, double> costs = new Dictionary<Link, double>();
            foreach (var link in _Topology.Links)
            {
                costs.Add(link, 1);
            }

            return GetShortestPaths(source, destination, costs);
        }

        private List<List<Link>> GetPaths(Node source, Node destination)
        {
            var discovered = new HashSet<Node>();
            var stack = new Stack<Node>();
            var paths = new List<List<Link>>();
            var next = new Dictionary<Node, Node>();
            stack.Push(destination);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                discovered.Add(current);
                var path = new List<Link>();
                if (current == source)
                {
                    next[destination] = null;
                    var n = source;
                    while (next[n] != null)
                    {
                        path.Add(_Topology.GetLink(n, next[n]));
                        n = next[n];
                    }
                    if (path.Count > 0)
                        paths.Add(path);
                    discovered.Clear();
                }
                foreach (var preNode in _Previous[current])
                {
                    if (!discovered.Contains(preNode))
                        stack.Push(preNode);
                    next[preNode] = current;
                }
            }

            return paths;
        }
    }
}
