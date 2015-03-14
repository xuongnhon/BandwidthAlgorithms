using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace NetworkSimulator
{
    public class Param
    {
        [XmlAttribute(AttributeName="Name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public string Value { get; set; }
    }

    public class RoutingAlgorithm
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        public List<Param> Params { get; set; }
        [XmlAttribute(AttributeName = "Selected")]
        public bool Selected { get; set; }

        public T GetParam<T>(string paramName)
        {
            string value = Params.Single(p => p.Name == paramName).Value;
            return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
        }

        public RoutingAlgorithm()
        {
            Params = new List<Param>();
            Selected = false;
        }
    }

    [Serializable]
    public class Configuration
    {
        private static Configuration SingleTonObject = null;

        public string TopologyFilePath { get; set; }

        public string IEListFilePath { get; set; }

        public string RequestFilePath { get; set; }

        public int TimerInterval { get; set; }

        // log
        public string LogFilePath { get; set; }

        public string LogFileName { get; set; }

        public bool LogActivated { get; set; }


        // statistics

        public bool StatisticsActivated { get; set; }

        public string StatisticsFilepath { get; set; }

        public string ResultPrefix { get; set; }

        public string ComputingPrefix { get; set; }

        //public string AcceptedBandwidthRatioPrefix { get; set; }
        public string StandardDeviationPrefix { get; set; }

        public int NumberOfSplit { get; set; }

        public List<RoutingAlgorithm> UnicastRoutingAlgorithms { get; set; }

        //caoth
        public List<RoutingAlgorithm> MulticastRoutingAlgorithms { get; set; }

        public RoutingAlgorithm getSelectedUnicastAlgorithm()
        {
            foreach (RoutingAlgorithm algorithm in UnicastRoutingAlgorithms)
            {
                if (algorithm.Selected)
                {
                    return algorithm;
                }
            }
            return null;
        }

        public static Configuration GetInstance()
        {
            if (SingleTonObject == null)
                SingleTonObject = Deserialize("config.xml");
            return SingleTonObject;
        }

        public Configuration()
        {
            UnicastRoutingAlgorithms = new List<RoutingAlgorithm>();
            MulticastRoutingAlgorithms = new List<RoutingAlgorithm>();
        }

        public static void Serialize(string file, Configuration c)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(c.GetType());
            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, c);
            writer.Flush();
            writer.Close();
        }

        public static Configuration Deserialize(string file)
        {
            System.Xml.Serialization.XmlSerializer xs
               = new System.Xml.Serialization.XmlSerializer(
                  typeof(Configuration));
            StreamReader reader = File.OpenText(file);
            Configuration c = (Configuration)xs.Deserialize(reader);
            reader.Close();
            return c;
        }
    }
}
