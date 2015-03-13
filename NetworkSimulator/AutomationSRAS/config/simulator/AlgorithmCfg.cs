using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.config.simulator
{
    [XmlType(TypeName = "RoutingAlgorithm")]
    public class AlgorithmCfg
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "Selected")]
        public bool Selected { get; set; }

        [XmlElement(ElementName = "Params")]
        public ParamsCfg ParamList { get; set; }
    }
}
