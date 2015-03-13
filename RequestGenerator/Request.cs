using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RequestGenerator
{
    public class Request
    {
        private int id;
        private int source;
        private int destination;
        private double bandwidth;
        private long incomingTime;
        private long holdingTime;

        public Request(int id, int source, int destination, double bandwidth, long incomingTime, long holdingTime)
        {
            this.id = id;
            this.source = source;
            this.destination = destination;
            this.bandwidth = bandwidth;
            this.incomingTime = incomingTime;
            this.holdingTime = holdingTime;
        }

        public override string ToString()
        {
            return id + "\t" + source + "\t" + destination + "\t" + bandwidth + "\t" + incomingTime + "\t" + holdingTime;
        }
    }
}
