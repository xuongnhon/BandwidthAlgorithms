using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents;
using System.Threading;
using NetworkSimulator.RoutingComponents.RoutingStrategies;
using System.Diagnostics;

namespace NetworkSimulator.SimulatorComponents
{
    public class Router
    {
        protected int _Id;

        protected Queue<Request> _RequestQueue;

        protected Thread _Thread;

        protected Object _TopologyLockingObject;

        protected ResponseManager _ResponseManager;

        protected RoutingStrategy _RoutingStrategy;

        protected Stopwatch _Stopwatch;

        protected ManualResetEvent _pauseEvent;

        public RoutingStrategy RoutingStrategy
        {
            get { return _RoutingStrategy; }
            set { _RoutingStrategy = value; }
        }

        public Router()
        {

        }

        public void Stop()
        {
            this._Thread.Abort();
        }

        public Router(int id, Object topologyLockingObject, ResponseManager responseManager)
        {
            _Id = id;
            _TopologyLockingObject = topologyLockingObject;
            _ResponseManager = responseManager;
            Initialize();
        }

        protected void Initialize()
        {
            _pauseEvent = new ManualResetEvent(true);
            _RequestQueue = new Queue<Request>();
            _Stopwatch = new Stopwatch();
            _Thread = new Thread(new ThreadStart(this.ProcessRequest));
            _Thread.Name = "Router " + _Id;
            _Thread.Start();
        }

        public void RecieveRequest(Request request)
        {
            Console.WriteLine(request);
            _RequestQueue.Enqueue(request);
            _pauseEvent.Set();
        }

        protected virtual void ProcessRequest()
        {
            Console.WriteLine("Router " + _Id + " started ...");
            while (_Thread.IsAlive)
            {
                if (_RequestQueue.Count > 0)
                {
                    lock (_TopologyLockingObject)
                    {
                        Request request = _RequestQueue.Dequeue();
                        _Stopwatch.Restart();

                        // 12/7 ngoctoan
                        List<Link> path;// = new List<Link>();
                        //if (RoutingStrategy.GetType().Name == "PBWA" || RoutingStrategy.GetType().Name == "NewPBWA")
                        //{
                        //    path = _RoutingStrategy.GetPath(request.SourceId, request.DestinationId, request.Demand, request.IncomingTime, _Stopwatch.ElapsedTicks + request.IncomingTime, request.HoldingTime);
                        //}
                        //else
                        //{
                        //    path = _RoutingStrategy.GetPath(request.SourceId, request.DestinationId, request.Demand);
                        //}

                        path = _RoutingStrategy.GetPath(request);

                        _Stopwatch.Stop();
                        Response response = new Response(request, path, _Stopwatch.Elapsed.TotalMilliseconds);
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
