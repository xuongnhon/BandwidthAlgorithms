using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomationSRAS.config.simulator
{
    [XmlRoot(ElementName = "Configuration")]
    public class SimulationConfigurator
    {

        [XmlElement(ElementName = "TopologyFilePath")]
        public string TopologyFilePath { get; set; }

        [XmlElement(ElementName = "IEListFilePath")]
        public string IEListFilePath { get; set; }

        [XmlElement(ElementName = "RequestFilePath")]
        public string RequestFilePath { get; set; }

        [XmlElement(ElementName = "TimerInterval")]
        public int TimerInterval { get; set; }

        [XmlElement(ElementName = "LogFilePath")]
        public string LogFilePath { get; set; }

        [XmlElement(ElementName = "LogFileName")]
        public string LogFileName { get; set; }

        [XmlElement(ElementName = "LogActivated")]
        public bool LogActivated { get; set; }

        [XmlElement(ElementName = "StatisticsActivated")]
        public bool StatisticsActivated { get; set; }

        [XmlElement(ElementName = "StatisticsFilepath")]
        public string StatisticsFilepath { get; set; }

        [XmlElement(ElementName = "ResultPrefix")]
        public string ResultPrefix { get; set; }

        [XmlElement(ElementName = "ComputingPrefix")]
        public string ComputingPrefix { get; set; }

        //[XmlElement(ElementName = "AcceptedBandwidthRatioPrefix")]
        //public string AcceptedBandwidthRatioPrefix { get; set; }

        [XmlElement(ElementName = "StandardDeviationPrefix")]
        public string StandardDeviationPrefix { get; set; }

        [XmlElement(ElementName = "NumberOfSplit")]
        public int NumberOfSplit { get; set; }

        [XmlElement(ElementName = "UnicastRoutingAlgorithms")]
        public AlgorithmsCfg UnicastRoutingAlgorithms { get; set; }

        [XmlElement(ElementName = "MulticastRoutingAlgorithms")]
        public AlgorithmsCfg MulticastRoutingAlgorithms { get; set; }

        public static void Serialize(string file, SimulationConfigurator c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }

        public static SimulationConfigurator Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(SimulationConfigurator));
            StreamReader reader = File.OpenText(file);
            SimulationConfigurator c = (SimulationConfigurator)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
    }
}
