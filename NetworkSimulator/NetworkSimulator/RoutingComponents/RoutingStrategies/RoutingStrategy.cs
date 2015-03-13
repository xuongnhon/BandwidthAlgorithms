using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.RoutingComponents.RoutingStrategies
{
    public abstract class RoutingStrategy
    {
        protected Topology _Topology;
        protected Dictionary<Link, double> _ResidualBandwidthCopy;

        public RoutingStrategy(Topology topology)
        {
            _Topology = topology;
            _ResidualBandwidthCopy = new Dictionary<Link, double>();
        }

        // caoth: review :((
        public abstract List<Link> GetPath(SimulatorComponents.Request request); //--> MUST DO
        
        //public abstract List<Link> GetPath(int sourceId, int destinationId, double bandwidth);

        //public abstract List<Link> GetPath(int sourceId, int destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime);

        //public abstract void Reroute();

        /// <summary>
        /// Eliminate all links which have residual bandwidth less than demand and from a reduced network.
        /// </summary>
        /// <param name="demand">Bandwidth demand</param>
        protected void EliminateAllLinksNotSatisfy(double demand)
        {
            foreach (Link link in _Topology.Links)
            {
                _ResidualBandwidthCopy[link] = link.ResidualBandwidth;
            }

            foreach (Link link in _Topology.Links)
            {
                if (link.ResidualBandwidth < demand)
                    link.ResidualBandwidth = 0;
            }
        }

        /// <summary>
        /// Restore topology 
        /// </summary>
        protected void RestoreTopology()
        {
            foreach (Link link in _Topology.Links)
            {
                link.ResidualBandwidth = _ResidualBandwidthCopy[link];
            }
        }
    }
}
