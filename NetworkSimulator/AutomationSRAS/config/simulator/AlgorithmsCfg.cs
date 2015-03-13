using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.config.simulator
{
    [XmlType(TypeName = "UnicastRoutingAlgorithms")]
    public class AlgorithmsCfg
    {
        [XmlElement(ElementName = "RoutingAlgorithm")]
        public List<AlgorithmCfg> algorithms;

        public void setSelectedAlgorithm(String name)
        {
            foreach (AlgorithmCfg algorithm in algorithms)
            {
                if (algorithm.Name.ToLower() == name.ToLower())
                {
                    algorithm.Selected = true;
                }
                else
                {
                    algorithm.Selected = false;
                }
            }
        }
    }
}
