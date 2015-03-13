using AutomationSRAS.config.automation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.Config.Automation
{
    [XmlRoot(ElementName = "AutomationConfig")]
    public class AutomationConfigurator
    {
        public static readonly string CONFIG_PATH = "AutomationConfig.xml";

        private static AutomationConfigurator instance;

        public static AutomationConfigurator getInstance()
        {
            if (instance == null)
            {
                instance = Deserialize(CONFIG_PATH);
            }
            return instance;
        }

        [XmlElement(ElementName = "Simulator")]
        public SimulatorCfg Simulator { get; set; }

        [XmlElement(ElementName = "Algorithms")]
        public AlgorithmsCfg algorithmList { get; set; }

        [XmlElement(ElementName = "Topologies")]
        public TopologiesCfg topologyList { get; set; }


        public static void Serialize(string file, AutomationConfigurator c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }

        public static AutomationConfigurator Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(AutomationConfigurator));
            StreamReader reader = File.OpenText(file);
            AutomationConfigurator c = (AutomationConfigurator)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
    }
}
