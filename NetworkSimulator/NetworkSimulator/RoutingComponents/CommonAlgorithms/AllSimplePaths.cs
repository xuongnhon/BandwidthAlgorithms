using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using System.Collections;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class AllSimplePaths
    {
        private Topology _Topology;

        public AllSimplePaths(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        private void Initialize()
        {

        }

        public List<List<Link>> GetPaths(Node source, Node destination)
        {
            var paths = new List<List<Link>>();

            Stack<Node> vististed = new Stack<Node>();

            vististed.Push(source);

            AllSimplePathsDepthFirstSearch(vististed, destination, paths);

            return paths;
        }


        private void AllSimplePathsDepthFirstSearch(Stack<Node> vististed, Node destination, List<List<Link>> paths)
        {
            Node lastVisited = vististed.Peek();

            IEnumerable<Link> edges = _Topology.AdjacentEdges(lastVisited);

            foreach (Link edge in edges)
            {
                Node linkedVertice = lastVisited == edge.Source ? edge.Destination : edge.Source;

                if (vististed.Contains(linkedVertice))
                {
                    continue;
                }
                else if (linkedVertice.Equals(destination))
                {
                    vististed.Push(linkedVertice);

                    List<Link> tmp = new List<Link>();

                    for (int i = vististed.Count - 1; i >= 0; i--)
                    {
                        if (i == 0) continue;

                        var link = _Topology.GetLink(vististed.ElementAt(i), vististed.ElementAt(i - 1));

                        tmp.Add(link);
                    }

                    paths.Add(tmp);

                    vististed.Pop();

                    continue;
                }

                vististed.Push(linkedVertice);

                AllSimplePathsDepthFirstSearch(vististed, destination, paths);

                vististed.Pop();
            }
        }
    }
}