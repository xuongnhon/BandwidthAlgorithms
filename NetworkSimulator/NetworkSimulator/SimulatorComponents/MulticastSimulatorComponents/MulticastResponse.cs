using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.SimulatorComponents;

namespace NetworkSimulator.MulticastSimulatorComponents
{
    public class MulticastResponse : Response
    {
        #region Fields        

        private Tree _Tree;
        private int _RejectedDes = 0;
        private bool _AcceptedReq = false;
        private string _Links = "";

        #endregion

        #region Properties
        
        public Tree Tree
        {
            get { return _Tree; }
            set { _Tree = value; }
        }

        public int RejectedDes
        {
            get { return _RejectedDes; }
          //  set { _RejectedDes = value; }
        }
                
        #endregion

        public MulticastResponse(MulticastRequest multicastrequest, Tree tree, double computingTime)
        {
            _Request = multicastrequest;
            _Tree = tree;
            _ComputingTime = computingTime;
            this.Statistic();
        }

        public override bool HasPath()
        {
            return _AcceptedReq;
        }

        public override string ToString()
        {
            string str = (MulticastRequest)Request + " RPT=" + _ResponseTime + " RLT=" + ReleasingTime + " COT=" + _ComputingTime + "ms" + "\n";
            
            str += "==================== PATH FROM SOURCE TO DESTINATION ===================\n\n";
            // caoth: todo

            if (_AcceptedReq)
            {
                str += _Links;
            }
            else
            {
                str += "\t\t\t REQUEST WAS REJECTED \n\n";
            }
          
            str += "========================================================================\n";

            return str;
        }

        private void Statistic()
        {
           
            foreach (List<Link> path in _Tree.Paths)
            {
                if (path.Count > 0)
                {
                    foreach (Link link in path)
                    {
                        _Links += "\t" + link + "\n";
                    }
                    _Links += "\n\n";
                }
                else
                {
                    _RejectedDes++;
                }
                
            }

            if (_RejectedDes < ((MulticastRequest)_Request).Destinations.Count)
                _AcceptedReq = true;
        }
    }
}
