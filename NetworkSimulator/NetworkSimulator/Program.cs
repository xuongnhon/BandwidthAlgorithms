using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetworkSimulator.SimulatorComponents;
using NetworkSimulator.NetworkComponents;
using NetworkSimulator.RoutingComponents.CommonAlgorithms;
using NetworkSimulator.RoutingComponents.RoutingStrategies;
using NetworkSimulator;
using System.ComponentModel;

namespace NetworkSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Topology t = new Topology("Map/mira_map.dat");

            //DORA d = new DORA(t);

            //d.Offline();



            //ShortestPaths sp = new ShortestPaths(t);

            //var paths = sp.GetShortestPath(t.Nodes[0], t.Nodes[9]);

            //foreach (var path in paths)
            //{
            //    foreach (var link in path)
            //    {
            //        Console.WriteLine(link);
            //    }
            //    Console.WriteLine("----------------------------------------");
            //}

            //STRA stra = new STRA(t);
            //stra.GetPath(0, 12, 50);

            //SRRA ssra = new SRRA(t);

            //foreach (var link in ssra.GetPath(0, 12, 50))
            //{
            //    Console.WriteLine(link);
            //}

            //int[] arr = new int[] { 3, 2, 2, 3, 4, 4, 6, 7, 8, 3, 2, 4, 6, 7, 5, 3, 4, 5, 6, 7, 8, 6, 5, 4, 4 };

            //double a = Math.Sqrt((arr.Sum(o => Math.Pow(o, 2) / (double)arr.Length) - Math.Pow(arr.Sum() / arr.Length, 2)));

            //Console.WriteLine(a);

            //double a = Math.Sqrt(t.Links.Sum(o => Math.Pow(o.ResidualBandwidth, 2))

            //Ticker a = new Ticker();
            //a.Start();
            //a.Stop();

            //NNRA aaa = new NNRA(t);

            //aaa.GetPath(0, 5, 10);

            // caoth
            SimulatorManager sm = SimulatorManager.getInstance();
            try
            {
                sm.Start();
                // sm.StartMulticast();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
