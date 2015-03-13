using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlType(TypeName = "Algorithm")]
    public class AlgorithmCfg
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }
    }
}
