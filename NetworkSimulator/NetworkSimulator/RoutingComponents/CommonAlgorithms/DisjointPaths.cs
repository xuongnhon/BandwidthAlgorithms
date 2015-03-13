using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.CommonAlgorithms
{
    class DisjointPaths
    {
        private Topology _Topology;

        private BreadthFirstSearch _BFS;

        public DisjointPaths(Topology topology)
        {
            _Topology = topology;
            Initialize();
        }

        private Dictionary<Link, double> _ResidualBandwidthCopy;

        private void Initialize()
        {
            _BFS = new BreadthFirstSearch(_Topology);
            _ResidualBandwidthCopy = new Dictionary<Link, double>();
        }

        private void BackupTopology()
        {
            _ResidualBandwidthCopy.Clear();
            foreach (var link in _Topology.Links)
            {
                _ResidualBandwidthCopy[link] = link.ResidualBandwidth;
            }
        }

        private void RestoreTopology()
        {
            foreach (var link in _Topology.Links)
            {
                link.ResidualBandwidth = _ResidualBandwidthCopy[link];
            }
        }

        /// <summary>
        /// Get Disjoint Paths
        /// </summary>
        /// <param name="source">Node Source</param>
        /// <param name="destination">Node Dest</param>
        /// <returns>List of links</returns>
        public List<Link> GetDisjointPaths(Node source, Node destination)
        {
            BackupTopology();

            List<Link> disjointPaths = new List<Link>();
            var path = _BFS.FindPath(source, destination);

            while (path.Count > 0)
            {
                foreach (var link in path)
                {
                    // Remove link
                    link.ResidualBandwidth = 0;
                    disjointPaths.Add(link);
                }

                path = _BFS.FindPath(source, destination);
            }

            RestoreTopology();
            return disjointPaths;
        }

    }
}