using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using System.IO;
using NetworkSimulator.SimulatorComponents;
using NetworkSimulator.RoutingComponents.MulticastRoutingStrategies;

namespace NetworkSimulator.MulticastSimulatorComponents
{
    public class MulticastRequestDispatcher : RequestDispatcher
    {

        public MulticastRequestDispatcher(string requestFilePath, Object topologyLockingObject)
            : base()
        {
            LoadMulticastRequestData(requestFilePath);
            _TopologyLockingObject = topologyLockingObject;
        }

        private void LoadMulticastRequestData(string requestFilePath)
        {
            FileStream file = new FileStream(requestFilePath, FileMode.Open);
            StreamReader reader = new StreamReader(file);

            _RequestList = new List<Request>();

            while (!reader.EndOfStream)
            {
                string[] value = reader.ReadLine().Split('\t');
                MulticastRequest package = MakeMulticastRequest(value);
                _RequestList.Add(package);
                _ReqCount++;
            }
            reader.Close();
            _RequestList = _RequestList.OrderBy(o => o.IncomingTime).ToList();
        }

        private MulticastRequest MakeMulticastRequest(string[] value)
        {
            int id = int.Parse(value[0]);
          
            int source = int.Parse(value[1]);
           //Add
            List<int> destination = new List<int>();
            int i = 0; 
            string[] des = value[2].Split(':');

            //Console.WriteLine("id  {0} --- {1}", id,des[3]);
            foreach(var d in des)
            {
                int des1 = int.Parse(des[i]);
                destination.Add(des1);
                i++;
            }
                                 
            double banwidth = double.Parse(value[3]);
            long incomingTime = long.Parse(value[4]);
            long holdingTime = long.Parse(value[5]);
            
            return new MulticastRequest(id, source, destination, banwidth, incomingTime, holdingTime);
        }

        public override void OnTickerTick(long elapsedTime)
        {
            if (_RequestList.Count > 0)// && _Routers.Count > 0)
            {
                MulticastRequest request = (MulticastRequest)_RequestList[0];
                while (request.IncomingTime <= elapsedTime)
                {
                    //_Routers[request.SourceId].RecieveRequest(request);
                    lock (_TopologyLockingObject)
                    {
                        //Request request = _RequestQueue.Dequeue();
                        _Stopwatch.Restart();

                        //Tree tree = ((MulticastRoutingStrategy)_RoutingStrategy).GetTree(request.SourceId, request.Destinations, request.Demand);
                        Tree tree = ((MulticastRoutingStrategy)_RoutingStrategy).GetTree(request);

                        _Stopwatch.Stop();

                        Response response = new MulticastResponse(request, tree, _Stopwatch.Elapsed.TotalMilliseconds);
                        _ResponseManager.ReceiveResponse(response);
                    }

                    _RequestList.RemoveAt(0);
                    if (_RequestList.Count == 0) break;
                    request = (MulticastRequest)_RequestList[0];
                }
            }
        }

    }
}
