using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.SimulatorComponents;

namespace NetworkSimulator.MulticastSimulatorComponents
{
    public class MulticastResponseManager : ResponseManager
    {
        // caoth
        public MulticastResponseManager(RequestDispatcher requestDispatcher, Ticker ticker, Topology topology)
            : base(requestDispatcher, ticker, topology)
        {
        }

        protected override void ReserveBandwidth(Response response)
        {
            MulticastResponse multicastReponse = (MulticastResponse)response;
            lock (_TopologyLockingObject)
            {
                List<Link> links = new List<Link>();

                foreach (List<Link> path in multicastReponse.Tree.Paths)
                {
                    links.AddRange(path);
                }

                List<Link> distinct = links.Distinct().ToList();
                foreach (Link link in distinct)
                {
                    link.UsingBandwidth += response.Request.Demand;
                }
            }                       
        }

        protected override void ReleaseBandwidth(Response response)
        {
            MulticastResponse multicastReponse = (MulticastResponse)response;
            lock (_TopologyLockingObject)
            {
                foreach (List<Link> path in multicastReponse.Tree.Paths)
                {
                    foreach (Link link in path)
                    {
                        link.UsingBandwidth -= response.Request.Demand;
                    }
                }
            }
                                  
            Console.WriteLine("REL-" + response);
        }

        public override void OnTickerTick(long elapsedTime)
        {
            _ElapsedTime = elapsedTime;
            DoRelease(elapsedTime);

            if (_ResponsesForStatistics.Count == _RequestDispatcher.ReqCount) // lam xong het cac request
            {
                // code for stoping timer and statistics here
                _Ticker.Stop();
                
                // caoth: ToDo evaluation for multicast routing
             //   int accepted = _ResponsesForStatistics.Count(r => ((MulticastResponse)r).Tree.Paths.Count(links => links.Count > 0) > 0);

                int softAccepted = 0;
                int rejectedDes = 0;
                int hardAccepted = 0;
                double computingTime = 0;
                double softAcceptedBand = 0;
                double hardAcceptedBand = 0;
                double totalBand = 0;
                foreach (MulticastResponse res in _ResponsesForStatistics)
                {
                    if (res.HasPath())
                    {
                        softAccepted++;
                        if (res.RejectedDes == 0)
                        {
                            hardAccepted++;
                            hardAcceptedBand += res.Request.Demand;
                        }
                        rejectedDes += res.RejectedDes;
                        computingTime += res.ComputingTime;
                        softAcceptedBand += res.Request.Demand;
                    }

                    totalBand += res.Request.Demand;
                }

                Console.WriteLine("=============================== STATISTIC ===============================\n");

                Console.WriteLine("   Hard accepted:  {0} , Soft accepted: {1} Requests and Rejected: {2} destinations! Computing time: {3}\n", hardAccepted, softAccepted, rejectedDes,computingTime);
                Console.WriteLine("bandwidth of hard accepted: {0} , soft accepted: {1} , total: {2}\n",hardAcceptedBand,softAcceptedBand,totalBand);
                Console.WriteLine("=========================================================================\n");

                //for log
                // Log.WriteLine(_ResponsesForStatistics);
            }

           
        }
        
    }
}
