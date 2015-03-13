using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlType(TypeName = "Algorithms")]
    public class AlgorithmsCfg
    {
        [XmlElement(ElementName = "Algorithm")]
        public List<AlgorithmCfg> algorithms;
    }
}
