using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.RoutingStrategies;
using NetworkSimulator.MulticastSimulatorComponents;

namespace NetworkSimulator.RoutingComponents.MulticastRoutingStrategies
{
    public abstract class MulticastRoutingStrategy : RoutingStrategy
    {
        public MulticastRoutingStrategy(Topology topology)
            : base(topology)
        {
        }
        
        public abstract Tree GetTree(MulticastRequest request);

     //   public abstract Tree GetTree(int sourceId, List<int> destinationId, double bandwidth, long incomingTime, long responseTime, long releasingTime);


    }
}
