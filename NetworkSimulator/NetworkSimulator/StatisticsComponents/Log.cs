using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NetworkSimulator.NetworkComponents;
using System.Reflection;
using NetworkSimulator.SimulatorComponents;

namespace NetworkSimulator.StatisticsComponents
{
    public static class Log
    {
        //for write
        private static string _SpaceTab = "\t";
        private static string _UndersCore = "_";
        private static string _NoPath = "No_path";

        private static string _RoutingStrategyName = Configuration.GetInstance().UnicastRoutingAlgorithms.SingleOrDefault(a => a.Selected == true).Name.ToLower();

        private static string _LogFilePath = Configuration.GetInstance().LogFilePath;

        private static string _LogFileName = Configuration.GetInstance().LogFileName;

        private static string _FileName = _LogFilePath + "\\" + _LogFileName;


        private static FileStream file = new FileStream(_FileName, FileMode.Create);
        private static StreamWriter wr = new StreamWriter(file);

        internal static void WriteLine(List<Response> _ResponsesForStatistics)
        {
            foreach (Response _Response in _ResponsesForStatistics)
            {
                wr.Write(_Response.Request.Id + _SpaceTab + _Response.Request.IncomingTime + _SpaceTab + _Response.ComputingTime + _SpaceTab + _Response.ReleasingTime + _SpaceTab);
                wr.Write("(" + _Response.Request.SourceId + _UndersCore + _Response.Request.DestinationId + _UndersCore + _Response.Request.Demand + "b" + ")" + _SpaceTab);
                wr.Write(" = " + _SpaceTab);
                if (_Response.Path.Count == 0)
                {
                    wr.Write(_NoPath);
                }
                else
                {
                    foreach (var path in _Response.Path)
                    {
                        wr.Write(path.Source.Key + _UndersCore + path.Destination.Key + ", ");
                    }
                }
                wr.WriteLine();
            }
            wr.Write("Accept : " + _ResponsesForStatistics.Count(r => r.Path.Count > 0));
            Console.WriteLine("Complete!");

            wr.Close();
            file.Close();
        }
    }
}