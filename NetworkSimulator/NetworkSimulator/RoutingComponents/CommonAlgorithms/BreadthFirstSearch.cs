using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class BreadthFirstSearch
    {
        private Dictionary<Node, Node> _Previous;

        private Topology _Topology;

        public BreadthFirstSearch(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {
            _Previous = new Dictionary<Node, Node>();
        }
        
        /// <summary>
        /// Find the path from a node to another.
        /// </summary>
        /// <param name="source">Source node</param>
        /// <param name="destination">Destination node</param>
        /// <returns>List of links</returns>
        public List<Link> FindPath(Node source, Node destination)
        {
            _Previous.Clear();
            _Previous[source] = null;
            _Previous[destination] = null;
            var queue = new Queue<Node>();
            var discovered = new HashSet<Node>();
            queue.Enqueue(source);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                discovered.Add(current);

                if (current.Key == destination.Key)
                    break;

                var links = current.Links;
                foreach (var link in links)
                {
                    var next = link.Destination;
                    var c = _Topology.GetLink(current, next).ResidualBandwidth;
                    if (c > 0 && !discovered.Contains(next))
                    {
                        _Previous[next] = current;
                        queue.Enqueue(next);
                        discovered.Add(next);
                    }
                }
            }

            return GetPath(destination);
        }

        // caoth
        public List<Link> FindPathMarkNode(Node source, Node destination, List<Node> markedNode)
        {
            _Previous.Clear();
            _Previous[source] = null;
            _Previous[destination] = null;
            var queue = new Queue<Node>();
            var discovered = new HashSet<Node>();
            queue.Enqueue(source);

            bool found = false;

            while (queue.Count > 0 && !found)
            {
                var current = queue.Dequeue();
                discovered.Add(current);
                
                // caoth
                if (!markedNode.Contains(current))
                    markedNode.Add(current);   

                //if (current.Key == destination.Key)
                //    break;

                var links = current.Links;
                foreach (var link in links)
                {
                    var next = link.Destination;
                    var c = _Topology.GetLink(current, next).ResidualBandwidth;
                    if (c > 0 && !discovered.Contains(next))
                    {
                        queue.Enqueue(next);

                        _Previous[next] = current;
                        if (Object.Equals(next, destination))
                        {
                            found = true;
                            break;
                        }
                        //discovered.Add(next);
                    }
                }
            }

            return GetPath(destination);
        }

        private List<Link> GetPath(Node node)
        {
            var path = new List<Link>();
            var current = node;
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
