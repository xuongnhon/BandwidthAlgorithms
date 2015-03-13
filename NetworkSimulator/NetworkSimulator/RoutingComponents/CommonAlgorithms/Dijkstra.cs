using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class Dijkstra
    {
        private Topology _Topology;

        private Dictionary<Node, Node> _Previous;

        private Dictionary<Node, double> _Distance;

        private const double MaxValue = double.MaxValue;

        public Dijkstra(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            _Distance = new Dictionary<Node, double>();
            _Previous = new Dictionary<Node, Node>();
        }

        public List<Link> GetShortestPath(int sourceId, int destinationId, Dictionary<Link, double> cost)
        {
            return GetShortestPath(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId], cost);
        }

        public List<Link> GetShortestPath(int sourceId, int destinationId)
        {
            return GetShortestPath(_Topology.Nodes[sourceId], _Topology.Nodes[destinationId]);
        }

        public List<Link> GetShortestPath(Node source, Node destination, Dictionary<Link, double> cost)
        {
            // Initialize
            _Distance.Clear();
            _Previous.Clear();
            foreach (var node in _Topology.Nodes)
            {
                _Distance[node] = MaxValue;
                _Previous[node] = null;
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
                        _Previous[v] = u;
                    }
                }
            }

            return GetPath(destination);
        }

        public List<Link> GetShortestPath(Node source, Node destination, Dictionary<Link, double> cost, double demand)
        {
            // Initialize
            _Distance.Clear();
            _Previous.Clear();
            foreach (var node in _Topology.Nodes)
            {
                _Distance[node] = MaxValue;
                _Previous[node] = null;
            }
            _Distance[source] = 0;

            var T = new List<Node>(_Topology.Nodes);
            var DT = new List<Node>(); // DT: destination temp
            DT.Add(destination);

            while (T.Count > 0 && DT.Count > 0)
            {
                // Find node u within T set, where d[u] = min{d[z]:z within T
                var u = T.First();
                foreach (var node in T)
                {
                    if (_Distance[node] < _Distance[u])
                        u = node;
                }

                T.Remove(u);
                DT.Remove(u);
                // Browse all adjacent node to update distance from s.
                foreach (var link in u.Links.Where(l => l.ResidualBandwidth >= demand))
                {
                    var v = link.Destination;
                    if (_Distance[v] > _Distance[u] + cost[link])
                    {
                        _Distance[v] = _Distance[u] + cost[link];
                        _Previous[v] = u;
                    }
                }
            }

            return GetPath(destination);
        }

        public List<Link> GetShortestPath(Node source, Node destination)
        {
            Dictionary<Link, double> cost = new Dictionary<Link, double>();
            foreach (var link in _Topology.Links)
            {
                cost.Add(link, 1);
            }

            return GetShortestPath(source, destination, cost);
        }

        private List<Link> GetPath(Node destination)
        {
            var path = new List<Link>();
            var current = destination;
            while (_Previous[current] != null)
            {
                var link = _Topology.GetLink(_Previous[current], current);
                path.Add(link);
                current = _Previous[current];
            }
            path.Reverse();
            return path;
        }
    }
}
