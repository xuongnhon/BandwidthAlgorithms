using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkSimulator.RoutingComponents.CommonObjects;

namespace NetworkSimulator.NetworkComponents
{
    public class Topology
    {
        #region Fields
        //private Object _LockingObject;

        private List<Node> _Nodes;

        private List<IEPair> _IEPairs;

        private List<Link> _Links;
        #endregion

        #region Properties
        //public Object LockingObject
        //{
        //    get { return _LockingObject; }
        //}

        public List<Node> Nodes
        {
            get { return _Nodes; }
        }

        public List<Link> Links
        {
            get
            {
                if (_Links == null)
                {
                    _Links = new List<Link>();
                    foreach (var node in _Nodes)
                    {
                        foreach (var link in node.Links)
                        {
                            _Links.Add(link);
                        }
                    }
                }

                return _Links;
            }
            set
            {
                _Links = value;
            }
        }

        public List<IEPair> IEPairs
        {
            get { return _IEPairs; }
            set { _IEPairs = value; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Topology(string filePath)
        {
            Initialize();
            LoadTopologyData(filePath);
        }

        public Topology(string topologyFilePath, string iEPairsFilePath)
        {
            Initialize();
            LoadTopologyData(topologyFilePath);
            LoadIEPairsData(iEPairsFilePath);
        }

        public Topology(Topology topology)
        {
            Initialize();
            foreach (var node in topology._Nodes)
            {
                _Nodes.Add(new Node(node.Key));
            }
            foreach (var node in topology._Nodes)
            {
                foreach (var link in node.Links)
                {
                    Node source = _Nodes[link.Source.Key];
                    Node destination = _Nodes[link.Destination.Key];
                    Link newLink = new Link(source, destination, link.Capacity);
                    newLink.UsingBandwidth = link.UsingBandwidth;
                    source.Links.Add(newLink);
                }
            }
            foreach (var ie in topology.IEPairs)
            {
                _IEPairs.Add(new IEPair(this._Nodes[ie.Ingress.Key], this._Nodes[ie.Egress.Key]));
            }
        }

        private void Initialize()
        {
            //_LockingObject = new Object();
            _Nodes = new List<Node>();
            _IEPairs = new List<IEPair>();
            _Links = null;
        }

        private void LoadIEPairsData(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            StreamReader reader = new StreamReader(file);

            while (!reader.EndOfStream)
            {
                string[] values = reader.ReadLine().Split('\t');
                Node ingress = _Nodes[int.Parse(values[0])];
                Node egress = _Nodes[int.Parse(values[1])];
                _IEPairs.Add(new IEPair(ingress, egress));
            }
            file.Close();
        }

        private void LoadTopologyData(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            StreamReader reader = new StreamReader(file);

            int n = int.Parse(reader.ReadLine());
            for (int i = 0; i < n; i++)
                _Nodes.Add(new Node(i));

            while (!reader.EndOfStream)
            {
                string[] values = reader.ReadLine().Split('\t');
                Node nodeA = _Nodes[int.Parse(values[0])];
                Node nodeB = _Nodes[int.Parse(values[1])];
                string mode = values[2];
                double capacity = double.Parse(values[3]);

                switch (mode)
                {
                    case "single":
                        {
                            Link link = new Link(nodeA, nodeB, capacity);
                            nodeA.Links.Add(link);
                            break;
                        }
                    case "double":
                        {
                            Link link = new Link(nodeA, nodeB, capacity);
                            Link rLink = new Link(nodeB, nodeA, capacity);
                            nodeA.Links.Add(link);
                            nodeB.Links.Add(rLink);
                            break;
                        }
                }
            }
            file.Close();
        }

        /// <summary>
        /// Get link by source node and destination node.
        /// </summary>
        /// <param name="source">Source node</param>
        /// <param name="destination">Destination node</param>
        /// <returns>Link</returns>
        public Link GetLink(Node source, Node destination)
        {
            return source.Links
            .Where(l => l.Destination.Key == destination.Key)
            .FirstOrDefault();
        }

        public Link GetLink(int sKey, int dKey)
        {
            return this.Nodes[sKey].Links
            .Where(l => l.Destination.Key == dKey)
            .FirstOrDefault();
        }

        public List<Link> AdjacentEdges(Node node)
        {
            return node.Links
                .Where(l => l.Destination.Key == node.Key || l.Source.Key == node.Key)
                .ToList<Link>();
        }

        // Hien HC
        // To get topology capacity
        public double Capacity
        {
            get
            {
                double result = 0;
                foreach (var link in Links)
                {
                    result += link.Capacity;
                }
                return result;
            }
        }

        // Hien HC
        // To get topology Using capacity
        public double UsingCapacity
        {
            get
            {
                double result = 0;
                foreach (var link in Links)
                {
                    result += link.UsingBandwidth;
                }
                return result;
            }
        }

        public override string ToString()
        {
            string s = "";
            foreach (var node in _Nodes)
            {
                foreach (var link in node.Links)
                {
                    s += link + "\n";
                }
            }
            return s;
        }

        public void CalculatePercentOfBandwidthUsedPerLink(NetworkSimulator.SimulatorComponents.Response _Response)
        {
            foreach (var _Link in this.Links)
            {
                _Link.AddPercentOfBandwidthUsed(_Response, _Link.UsingBandwidth / _Link.Capacity * 100);
            }
        }

        // caoth
        public void Invert()
        {
            foreach (var link in this.Links)
            {
                Node oldSource = link.Source;

                link.Source.Links.Remove(link);
                link.Destination.Links.Add(link);

                link.Source = link.Destination;                
                link.Destination = oldSource;                                
            }
        }
    }
}
