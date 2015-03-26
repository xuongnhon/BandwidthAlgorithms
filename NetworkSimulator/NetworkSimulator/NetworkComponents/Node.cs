using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkSimulator.NetworkComponents
{
    public class Node
    {
        #region Fields
        private int _Key;

        protected List<Link> _Links;

        // caoth
        protected List<Node> _Children;
        #endregion

        #region Properties
        public int Key
        {
            get { return _Key; }
        }

        public List<Link> Links
        {
            get { return _Links; }
            //set { _Links = value; }
        }

        public List<Node> Children
        {
            get { return _Children; }
            set { _Children = value; }
        }
        #endregion

        public Node(int key)
        {
            _Key = key;
            _Links = new List<Link>();

            _Children = null;
        }

        public override string ToString()
        {
            return "NODE " + _Key;
        }
    }
}
