using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.SimulatorComponents;
using NetworkSimulator.RoutingComponents.MulticastRoutingStrategies;
using System.Threading;

namespace NetworkSimulator.MulticastSimulatorComponents
{
    public class MulticastRouter : Router
    {
        public MulticastRouter(int id, Object topologyLockingObject, ResponseManager responseManager)
            : base(id, topologyLockingObject, responseManager)
        {
            //int debug = 0;
        }

        protected override void ProcessRequest()
        {
            Console.WriteLine("MulticastRouter " + _Id + " started ...");            

            while (_Thread.IsAlive)
            {
                if (_RequestQueue.Count > 0)
                {
                    lock (_TopologyLockingObject)
                    {
                        MulticastRequest request = (MulticastRequest)(_RequestQueue.Dequeue());
                        _Stopwatch.Restart();
                        //Tree tree = ((MulticastRoutingStrategy)_RoutingStrategy).GetTree(request.SourceId, request.Destinations, request.Demand);
                        Tree tree = ((MulticastRoutingStrategy)_RoutingStrategy).GetTree(request);
                        _Stopwatch.Stop();
                        Response response = new MulticastResponse(request, tree, _Stopwatch.Elapsed.TotalMilliseconds);
                        _ResponseManager.ReceiveResponse(response);
                    }
                }
                else
                {
                    _pauseEvent.Reset();
                    _pauseEvent.WaitOne(Timeout.Infinite);
                }
            }
        }
    }
}
