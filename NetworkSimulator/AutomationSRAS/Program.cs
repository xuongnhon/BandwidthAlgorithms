using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationSRAS.Config.Automation;
using System.Diagnostics;
using AutomationSRAS.config.simulator;
using System.IO;

namespace AutomationSRAS
{
    class Program
    {
        static void Main(string[] args)
        {
            AutomationConfigurator cfg = AutomationConfigurator.getInstance();
            SimulationConfigurator scfg = SimulationConfigurator.Deserialize(cfg.Simulator.WorkingDir + cfg.Simulator.ConfigFile);

            foreach (TopologyCfg topology in cfg.topologyList.topologies)
            {
                //scfg.TopologyFilePath = cfg.Simulator.WorkingDir + topology.filepath;
                scfg.TopologyFilePath = topology.filepath;
                //scfg.IEListFilePath = cfg.Simulator.WorkingDir + topology.IEListFilePath;
                scfg.IEListFilePath = topology.IEListFilePath;

                List<FileInfo> listFileRequest = new List<FileInfo>();
                if (Directory.Exists(topology.requestDirectory))
                {
                    DirectoryInfo _DirectoryInfo = new DirectoryInfo(topology.requestDirectory);
                    foreach (FileInfo _FileInfo in _DirectoryInfo.GetFiles())
                    {
                        if (_FileInfo.Extension != null)
                        {
                            if (_FileInfo.Extension.Length > 1)
                            {
                                string strExtention = _FileInfo.Extension.Substring(1);
                                if (Object.Equals(strExtention.ToUpper(), "TXT"))
                                {
                                    listFileRequest.Add(_FileInfo);
                                }
                            }
                        }
                    }
                }
                foreach (FileInfo _FileInfo in listFileRequest)
                {
                    scfg.RequestFilePath = topology.requestDirectory + _FileInfo.Name;
                    scfg.StatisticsFilepath = topology.resultDirectory + _FileInfo.Name;
                    foreach (AutomationSRAS.Config.Automation.AlgorithmCfg algorithm in cfg.algorithmList.algorithms)
                    {

                        scfg.UnicastRoutingAlgorithms.setSelectedAlgorithm(algorithm.name);
                        SimulationConfigurator.Serialize(cfg.Simulator.ConfigFile, scfg);

                        Console.Write("Topology [" + topology.name + "] \nRequest [" + _FileInfo.Name + "] \nAlgorithm [" + algorithm.name + "] \nProcessing... ");
                        // Run simulator
                        Process simulator = new Process();
                        simulator.StartInfo.FileName = cfg.Simulator.WorkingDir + cfg.Simulator.ExecutorName;
                        simulator.EnableRaisingEvents = true;

                        if (!cfg.Simulator.ShowUI)
                        {
                            simulator.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        }
                        //simulator.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        simulator.Start();
                        simulator.WaitForExit();
                        
                        Console.WriteLine("[DONE]\n");
                    }
                }
            }
            Console.WriteLine("Automation Job DONE.");
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
