using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlType(TypeName = "Request")]
    public class RequestCfg
    {
        [XmlAttribute(AttributeName = "filename")]
        public string filename { get; set; }
    }
}
