using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.config.automation
{
    [XmlType(TypeName = "Simulator")]
    public class SimulatorCfg
    {
        [XmlAttribute(AttributeName = "workingDir")]
        public string WorkingDir { get; set; }

        [XmlAttribute(AttributeName = "executorName")]
        public string ExecutorName { get; set; }

        [XmlAttribute(AttributeName = "configFile")]
        public string ConfigFile { get; set; }

        [XmlAttribute(AttributeName = "showUI")]
        public bool ShowUI { get; set; }
    }
}
