using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.config.simulator
{
    [XmlType(TypeName = "Params")]
    public class ParamsCfg
    {
        [XmlElement(ElementName = "Param")]
        public List<ParamCfg> Params;

        public T GetParam<T>(string paramName)
        {
            string value = Params.Single(p => p.Name == paramName).Value;
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
        }
    }
}
