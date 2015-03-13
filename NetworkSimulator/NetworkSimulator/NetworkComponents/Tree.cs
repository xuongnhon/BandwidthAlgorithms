using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.NetworkComponents;

namespace NetworkSimulator.NetworkComponents
{
    public class Tree
    {
        #region fields

        private Node _root;
        public Node Root
        {
            get { return _root; }
            set { _root = value; }
        }


        private List<List<Link>> _paths;
        public List<List<Link>> Paths
        {
            get { return _paths; }
            set { _paths = value; }
        }
        
        #endregion

        public Tree()
        {
            _paths = new List<List<Link>>();
        }
       
        public override string ToString()
        {
            return "Trees ";
        }
       
    }
}
