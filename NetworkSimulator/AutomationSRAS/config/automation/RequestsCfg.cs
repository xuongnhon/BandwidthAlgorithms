using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlType(TypeName = "Requests")]
    public class RequestsCfg
    {
        [XmlElement(ElementName = "Request")]
        public List<RequestCfg> requests;
    }
}
