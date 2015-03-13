using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RequestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] IE_MIRA = new int[,] { { 0, 12 }, { 4, 8 }, { 3, 1 }, { 4, 14 } };
            //int[] P_IE_MIRA = new int[] { 20, 20, 20, 40 };
            //int[] P_IE_MIRA = new int[] { 10, 10, 40, 40 };
            int[] P_IE_MIRA = new int[] { 10, 20, 30, 40 };
            //int[] P_IE_MIRA = new int[] { 15, 20, 30, 35 };
            //int[] BW_MIRA = new int[] { 5, 10, 15, 20 };
            //int[] BW_MIRA = new int[] { 13, 17, 23, 29 };
           // int[] BW_MIRA = new int[] { 5, 11, 17, 23 };
           //int[] BW_MIRA = new int[] { 30, 35, 45, 50 };
            //int[] BW_MIRA = new int[] { 10, 20, 30, 40 };
            int[] BW_MIRA = new int[] { 5, 10, 20, 30 };

            int[,] IE_CESNET = new int[,] { { 0, 18 }, { 1, 11 }, { 3, 16 }, { 4, 7 }, { 5, 13 }, { 6, 19 }, { 15, 0 }, { 19, 8 } };
            //int[] P_IE_CESNET = new int[] { 12, 13, 12, 13, 12, 13, 12, 13 };
            //int[] P_IE_CESNET = new int[] { 5, 5, 10, 10, 15, 15, 20, 20 };
            int[] P_IE_CESNET = new int[] { 5, 5, 10, 10, 10, 10, 10, 40 };
            //int[] BW_CESNET = new int[] { 100, 130, 160, 200 };
            int[] BW_CESNET = new int[] { 40, 60, 120, 200 };
            // TODO: int[] P_IE_CESNET = new int[] { 10, 20, 30, 40 };

            //Screnario staticScenario = new StaticScenario(IE_MIRA, P_IE_MIRA, BW_MIRA, 1, 1000, 5);
            //staticScenario.Generate("21_static_MIRA_P10-20-30-40_bw5-10-20-30_1000.txt");

            /*Screnario staticScenario = new StaticScenario(IE_CESNET, P_IE_CESNET, BW_CESNET, 1, 1000, 5);
            staticScenario.Generate("22_static_CESNET_P5-10-40_bw40-60-120-200_1000.txt");*/


            int[,] IE_ANSNET = new int[,] { { 0, 28 }, { 1, 13 }, { 2, 30 }, { 3, 22 }, { 4, 10 }, { 6, 30 }, { 8, 23 }, { 21, 5 }, { 17, 5}, { 20, 16} };
            int[] P_IE_ANSNET = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 };
            int[] BW_ANSNET = new int[] { 20, 30, 40, 50 };
            
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // λ=40 request per (timeUnit * TimerInterval) and μ=10
            // timeUnit = 400 --> runtime = 400 * TimerInterval (lần tick)            

            //DynamicScrenario dynamicScenario = new DynamicScrenario(IE_MIRA, P_IE_MIRA, BW_MIRA, 400, 2000, 40, 30);
            //dynamicScenario.Generate("15_dynamic_MIRA_P10-20-30-40_bw13-17-23-29_400_2000_40_30.txt");

            //DynamicScrenario dynamicScenario = new DynamicScrenario(IE_ANSNET, P_IE_ANSNET, BW_ANSNET, 400, 2000, 80, 30);
            //dynamicScenario.Generate("03_dynamic_ANSNET_bw20-30-40-50_400_2000_60_20.txt");

            Screnario staticScenario = new StaticScenario(IE_ANSNET, P_IE_ANSNET, BW_ANSNET, 1, 1000, 5);
            staticScenario.Generate("Thao_static_ANSNET_bw20-30-40-50_1000.txt");

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            // λ=40 and μ=10

            //MixScenario mixScenario = new MixScenario(IE_MIRA, P_IE_MIRA, BW_MIRA, 300, 200, 1800, 40, 10, 2);
            //mixScenario.Generate("mix_MIRA_bw30-35-45-50_300_200+1800_40_10.txt");

            //MixScenario mixScenario = new MixScenario(IE_CESNET, P_IE_CESNET, BW_CESNET, 300, 200, 1800, 40, 10, 2);
            //mixScenario.Generate("mix_CESNET_bw40-80-1200-1600_300_200+1800_40_10.txt");



            //int[] arr = new int[4];
            //Troschuetz.Random.DiscreteUniformDistribution r = new Troschuetz.Random.DiscreteUniformDistribution();
            //r.Alpha = 0;
            //r.Beta = 99;
            //for (int i = 0; i < 1000; i++)
            //{
            //    arr[GetIEId(r.Next(), P)]++;
            //}

            //foreach (var item in arr)
            //{
            //    Console.WriteLine(item);
            //}

            //TestDistribution();
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        static void TestDistribution()
        {
            FileStream f = new FileStream("Exponential_2000_10.txt", FileMode.Create);
            StreamWriter wr = new StreamWriter(f);

            Troschuetz.Random.ExponentialDistribution r = new Troschuetz.Random.ExponentialDistribution();
            //double mu = 10; // mean
            r.Lambda = 0.15;//1 / mu;
            for (int i = 0; i < 2000; i++) // 2000
            {
                wr.WriteLine(r.NextDouble());
            }

            wr.Close();
        }
    }
}
