using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetworkSimulator.SimulatorComponents;

namespace NetworkSimulator.NetworkComponents
{
    public class Link
    {
        #region Fields
        //private string _Key;

        private double _Capacity;

        private double _UsingBandwidth;

        private Node _Source;

        private Node _Destination;

        private Dictionary<NetworkSimulator.SimulatorComponents.Response, double> _PercentOfBandwidthUsed;

        #endregion

        #region Properties
        public string Key
        {
            //get { return _Key; }
            get { return _Source.Key + "|" + _Destination.Key; }
        }

        public double Capacity
        {
            get { return _Capacity; }
            set { _Capacity = value; }
        }

        public double ResidualBandwidth
        {
            get { return _Capacity - _UsingBandwidth; }
            set { _UsingBandwidth = _Capacity - value; }
        }

        public double UsingBandwidth
        {
            get { return _UsingBandwidth; }
            set { _UsingBandwidth = value; }
        }

        public Node Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        public Node Destination
        {
            get { return _Destination; }
            set { _Destination = value; }
        }

        public Dictionary<NetworkSimulator.SimulatorComponents.Response, double> PercentOfBandwidthUsed
        {
            get { return _PercentOfBandwidthUsed; }
        }
        #endregion

        public Link(Node source, Node destination, double capacity)
        {
            //_Key = source.Key + "|" + destination.Key;
            this._Source = source;
            this._Destination = destination;
            this._Capacity = capacity;
            _PercentOfBandwidthUsed = new Dictionary<Response, double>();
        }

        public override string ToString()
        {
            //string[] v = _Key.Split('|');
            //return "LNK-(" + v[0] + "-" + v[1] + ") CAP=" + _Capacity + " RSD=" + this.ResidualBandwidth;
            return "LNK-(" + Key + ") CAP=" + _Capacity + " RSD=" + this.ResidualBandwidth;
        }

        public void AddPercentOfBandwidthUsed(NetworkSimulator.SimulatorComponents.Response _Response, double _Value)
        {
            this._PercentOfBandwidthUsed.Add(_Response, _Value);
        }
    }
}
