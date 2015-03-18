using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.StatisticsComponents;

namespace NetworkSimulator.SimulatorComponents
{
    public class ResponseManager : ITickerListener
    {
        protected List<Response> _ResponsesToRelease;

        protected RequestDispatcher _RequestDispatcher;

        protected long _ElapsedTime;

        protected List<Response> _ResponsesForStatistics;

        protected Ticker _Ticker;

        protected Object _TopologyLockingObject;

        // caoth
        public ResponseManager()
        {
        }

        public List<Response> ResponsesToRelease { get { return _ResponsesToRelease; } }

        public ResponseManager(RequestDispatcher requestDispatcher, Ticker ticker, Object topologyLockingObject)
        {
            Initialize();
            _RequestDispatcher = requestDispatcher;
            _Ticker = ticker;
            _TopologyLockingObject = topologyLockingObject;
        }

        protected void Initialize()
        {
            _ResponsesToRelease = new List<Response>();
            _ResponsesForStatistics = new List<Response>();
        }

        public void ReceiveResponse(Response response)
        {
            lock (this)
            {
                ReserveBandwidth(response);
                // Old code
                response.ResponseTime = _ElapsedTime;

                // New code
                //response.ResponseTime = response.Request.IncomingTime;

                if (response.HasPath() && response.Request.HoldingTime < int.MaxValue) // co dg & dynamic 
                {
                    _ResponsesToRelease.Add(response);
                    _ResponsesToRelease = _ResponsesToRelease.OrderBy(r => r.ReleasingTime).ToList();
                }


                _ResponsesForStatistics.Add(response);

                // print out for tracking
                Console.WriteLine(response);
            }
        }

        protected virtual void ReserveBandwidth(Response response)
        {
            lock (_TopologyLockingObject)
            {
                foreach (var link in response.Path)
                {
                    link.UsingBandwidth += response.Request.Demand;
                }
            }
        }

        protected virtual void ReleaseBandwidth(Response response)
        {
            lock (_TopologyLockingObject)
            {
                foreach (var link in response.Path)
                {
                    link.UsingBandwidth -= response.Request.Demand;
                }
            }

            Console.WriteLine("REL-" + response);
        }

        public virtual void OnTickerTick(long elapsedTime)
        {
            _ElapsedTime = elapsedTime;

            DoRelease(elapsedTime);

            if (_ResponsesForStatistics.Count == _RequestDispatcher.ReqCount) // lam xong het cac request
            {
                // code for stoping timer and statistics here
                _Ticker.Stop();
                int accepted = _ResponsesForStatistics.Count(r => r.Path.Count > 0);
                Console.WriteLine("/////////////////////////////////");
                Console.WriteLine("// - Accepted: " + accepted);
                Console.WriteLine("/////////////////////////////////");

                // 16/7/2013 ngoctoan
                if (Configuration.GetInstance().LogActivated == true)
                {
                    Log.WriteLine(_ResponsesForStatistics);
                }

                //for Statistics to excel
                if (Configuration.GetInstance().StatisticsActivated == true)
                {
                    Statistics.Ex(_ResponsesForStatistics);
                }

                //Response1.ComputingTime = Response2.ComputingTime
                //Because Response1.ComputingTime is very big :v
                if (_ResponsesForStatistics.Count >= 2)
                    _ResponsesForStatistics.ElementAt(0).ComputingTime = _ResponsesForStatistics.ElementAt(1).ComputingTime;

                Console.WriteLine("Writing result to text file...");
                Statistics.WriteResultToText(_ResponsesForStatistics);

                Console.WriteLine("Writing standard deviation to text file...");
                Statistics.WriteStandardDeviationResultToText(_ResponsesForStatistics, (Topology)_TopologyLockingObject);

                Console.WriteLine("Simulator stopping...");
                SimulatorManager.getInstance().StopSimulate();
            }
        }

        protected void DoRelease(long elapsedTime)
        {
            if (_ResponsesToRelease.Count > 0)
            {
                Response response = _ResponsesToRelease[0];
                while (response.ReleasingTime <= elapsedTime)
                {
                    ReleaseBandwidth(response);
                    _ResponsesToRelease.RemoveAt(0);
                    if (_ResponsesToRelease.Count == 0) break;
                    response = _ResponsesToRelease[0];
                }
            }
        }
    }
}
