using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlType(TypeName = "Topology")]
    public class TopologyCfg
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }

        [XmlAttribute(AttributeName = "filepath")]
        public string filepath { get; set; }

        [XmlAttribute(AttributeName = "IEfilepath")]
        public string IEListFilePath { get; set; }

        [XmlAttribute(AttributeName = "requestDir")]
        public string requestDirectory { get; set; }

        [XmlAttribute(AttributeName = "resultDir")]
        public string resultDirectory { get; set; }

        [XmlElement(ElementName = "Requests")]
        public RequestsCfg requestList { get; set; }

    }
}
