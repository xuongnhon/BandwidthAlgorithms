using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.SimulatorComponents
{
    public class Response
    {
        #region Fields
        protected Request _Request;

        private List<Link> _Path;

        protected long _ResponseTime;

        protected double _ComputingTime;
        #endregion

        #region Properties
        public Request Request
        {
            get { return _Request; }
            set { _Request = value; }
        }

        public List<Link> Path
        {
            get { return _Path; }
            set { _Path = value; }
        }

        public long ResponseTime
        {
            get { return _ResponseTime; }
            set { _ResponseTime = value; }
        }

        public long ReleasingTime
        {
            get { return _Request.IncomingTime + _Request.HoldingTime; }
        }

        public double ComputingTime
        {
            get { return _ComputingTime; }
            set { _ComputingTime = value; }
        }
        #endregion

        // caoth
        public Response()
        {
        }

        public Response(Request request, List<Link> path, double computingTime)
        {
            _Request = request;
            _Path = path;
            _ComputingTime = computingTime;
        }

        public override string ToString()
        {
            string str = "RSP-" + Request + " RPT=" + _ResponseTime + " RLT=" + ReleasingTime + " COT=" + _ComputingTime + "ms" + "\n";
            str += "==================== PATH ===================\n";
            foreach (var link in _Path)
            {
                str += "\t" + link + "\n";
            }
            str += "=============================================";
            return str;
        }

        public virtual bool HasPath()
        {
            return Path.Count > 0;
        }
    }
}
