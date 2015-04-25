using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RequestGenerator
{
    class Program
    {
        public class Configuration
        {
            public string TopologyName { get; private set; }
            public int[,] IE { get; private set; }
            public int TypeOfRequest { get; private set; }
            public int NumberOfRequest { get; private set; }
            public string RequestTypeName { get; private set; }
            public PercentUse PercentUse { get; private set; }
            public Bandwidth Bandwidth { get; private set; }

            public Configuration(string TopologyName, int[,] IE, int TypeOfRequest, int NumberOfRequest, PercentUse PercentUse, Bandwidth Bandwidth)
            {
                if (Object.Equals(IE, null))
                    throw new NullReferenceException("IE Is Null");
                if (!(IE.Length > 0))
                    throw new ArgumentException("IE Length Must Be Lagre Than 1");
                this.TopologyName = TopologyName;
                this.IE = IE;
                this.TypeOfRequest = TypeOfRequest;
                this.NumberOfRequest = NumberOfRequest;
                RequestTypeName = this.TypeOfRequest == 1 ? "Static" : "Dynamic";
                this.PercentUse = PercentUse;
                this.Bandwidth = Bandwidth;
            }
        }

        public class PercentUse
        {
            public int[] P { get; set; }
            public string Name { get; private set; }

            public PercentUse(int[] P, string Name)
            {
                if (!(P.Length > 0))
                    throw new ArgumentException("Number Of IE Must Be Larger Than 0");
                this.P = P;
                this.Name = Name;
            }
        }

        public class Bandwidth
        {
            public int TypeOfBandwidth { get; private set; }
            public int[] Array { get; private set; }
            public int First { get; private set; }
            public int Last { get; private set; }
            public string Name { get; private set; }

            public Bandwidth(int First, int Last)
            {
                if (First > Last)
                    throw new ArgumentException("First Argument Is Larger Last Argument");
                TypeOfBandwidth = 1;
                this.First = First;
                this.Last = Last;
                Name = string.Format("BW-Section[{0}-{1}]", this.First, this.Last);
            }

            public Bandwidth(int[] ArrayOfBandwidth, int Jumb)
            {
                if (Object.Equals(ArrayOfBandwidth, null))
                    throw new NullReferenceException("ArrayOfBandwidth Is Null");
                if (!(ArrayOfBandwidth.Length > 0))
                    throw new ArgumentException("ArrayOfBandwidth Length Must Be Lagre Than 1");
                if (Object.Equals(Jumb, null))
                    throw new NullReferenceException("Jumb Is Null");
                for (int i = 0; i < ArrayOfBandwidth.Length - 1; i++)
                {
                    if (ArrayOfBandwidth[i + 1] - ArrayOfBandwidth[i] != Jumb)
                        throw new ArgumentException("ArrayOfBandwidth Error");
                }
                TypeOfBandwidth = 2;
                Array = ArrayOfBandwidth;
                Name = string.Format("BW-SegmentArray[{0}][{1}-{2}]", Jumb, Array[0], Array[Array.Length - 1]);
            }

            public Bandwidth(int[] ArrayOfBandwidth)
            {
                if (Object.Equals(ArrayOfBandwidth, null))
                    throw new NullReferenceException("ArrayOfBandwidth is null");
                if (!(ArrayOfBandwidth.Length > 0))
                    throw new ArgumentException("ArrayOfBandwidth length must be lagre 1");
                TypeOfBandwidth = 3;
                Array = ArrayOfBandwidth;
                Name = "BW-Array[";
                foreach (var Item in Array)
                {
                    Name += string.Format("{0}-", Item);
                }
                Name = Name.Substring(0, Name.Length - 1);
                Name += "]";
            }
        }

        static void Main(string[] args)
        {
            /*
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

            //Screnario staticScenario = new StaticScenario(IE_CESNET, P_IE_CESNET, BW_CESNET, 1, 1000, 5);
            //staticScenario.Generate("22_static_CESNET_P5-10-40_bw40-60-120-200_1000.txt");


            int[,] IE_ANSNET = new int[,] { { 0, 28 }, { 1, 13 }, { 2, 30 }, { 3, 22 }, { 4, 10 }, { 6, 30 }, { 8, 23 }, { 21, 5 }, { 17, 5 }, { 20, 16 } };
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
            */
            int[,] IE_MIRA = new int[,] { { 0, 12 }, { 4, 8 }, { 3, 1 }, { 4, 14 } };
            //int[,] IE_MIRA = new int[,] { { 0, 12 }, { 4, 8 }, { 3, 1 } };//3IE
            //int[,] IE_MIRA = new int[,] { { 0, 12 }, { 4, 8 }, { 3, 1 }, { 4, 14 }, { 7, 11 }, { 1, 14 } };//6IE
            string NAME_MIRA = "MIRA";

            int[,] IE_CESNET = new int[,] { { 0, 18 }, { 1, 11 }, { 3, 16 }, { 4, 7 }, { 5, 13 }, { 6, 19 }, { 15, 0 }, { 19, 8 } };
            //int[,] IE_CESNET = new int[,] { { 0, 18 }, { 1, 11 }, { 3, 16 }, { 4, 7 }, { 5, 13 }, { 6, 19 } };//6IE
            //int[,] IE_CESNET = new int[,] { { 0, 18 }, { 1, 11 }, { 3, 16 }, { 4, 7 }, { 5, 13 }, { 6, 19 }, { 15, 0 }, { 19, 8 }, { 1, 13 }, { 8, 19 } };//10IE
            string NAME_CESNET = "CESNET";

            //ANSNET tu 1-32
            //int[,] IE_ANSNET = new int[,] { { 1, 29 }, { 18, 6 }, { 3, 23 }, { 7, 31 }, { 21, 17 } };
            //int[,] IE_ANSNET = new int[,] { { 1, 29 }, { 18, 6 }, { 3, 23 } };//3IE
            //int[,] IE_ANSNET = new int[,] { { 1, 29 }, { 18, 6 }, { 3, 23 }, { 7, 31 }, { 21, 17 }, { 6, 21 }, { 4, 25 } };//IE7

            //ANSNET tu 0-31
            int[,] IE_ANSNET = new int[,] { { 0, 28 }, { 17, 5 }, { 2, 22 }, { 6, 30 }, { 20, 16 } };
            //int[,] IE_ANSNET = new int[,] { { 0, 28 }, { 17, 5 }, { 2, 22 } };//3IE
            //int[,] IE_ANSNET = new int[,] { { 0, 28 }, { 17, 5 }, { 2, 22 }, { 6, 30 }, { 20, 16 }, { 5, 20 }, { 3, 24 } };//IE7
            string NAME_ANSNET = "ANSNET";

            #region Get Input
            string Input = "";
            bool isOk = false;
            char[] Chars = new char[] { '/', '\\', ':', '*', '?', '\"', '<', '>', '|', ' ' };

            Console.Write("Chose Topology (MIRA: 1; CESNET: 2; ANSNET: 3)\nYour Chose: ");
            int intTopology = 0;
            while (!isOk)
            {
                Input = Console.ReadLine();
                try { intTopology = int.Parse(Input); }
                catch { Console.Write("Type Only 1, 2 Or 3!!!\nYour Chose: "); continue; }
                if (intTopology > 3 || intTopology < 1)
                { Console.Write("Type Only 1, 2 Or 3!!!\nYour Chose: "); continue; }
                isOk = true;
            }

            Console.Clear();
            Console.Write("Number Of Files Will Be Created [1 - 100]: ");
            isOk = false;
            int NumberOfFiles = 0;
            while (!isOk)
            {
                Input = Console.ReadLine();
                try { NumberOfFiles = int.Parse(Input); }
                catch { Console.Write("Type Only Number!!!\nNumber Of Files Will Be Created [1 - 100]: "); continue; }
                if (NumberOfFiles > 100 || NumberOfFiles < 1)
                { Console.Write("Number Of Files Between 1 And 100!!!\nNumber Of Files Will Be Created [1 - 100]: "); continue; }
                isOk = true;
            }

            Console.Clear();
            Console.Write("Type Of Request (Static: 1; Dynamic: 2)\nYour Chose: ");
            isOk = false;
            int intTypeOfRequest = 0;
            while (!isOk)
            {
                Input = Console.ReadLine();
                try { intTypeOfRequest = int.Parse(Input); }
                catch { Console.Write("Type Only 1 Or 2!!!\nYour Chose: "); continue; }
                if (intTypeOfRequest > 2 || intTypeOfRequest < 1)
                { Console.Write("Type Only 1 Or 2!!!\nYour Chose: "); continue; }
                isOk = true;
            }

            int intLamda = 0;
            int intMy = 0;
            string _DynamicRequestName = "";
            if (intTypeOfRequest == 2)
            {
                Console.Clear();
                Console.Write("Lamda: ");
                isOk = false;
                while (!isOk)
                {
                    Input = Console.ReadLine();
                    try { intLamda = int.Parse(Input); }
                    catch { Console.Write("Type Only Number!!!\nLamda: "); continue; }
                    if (intLamda < 0)
                    { Console.Write("Lamda > 0!!!\nLamda: "); continue; }
                    isOk = true;
                }

                Console.Clear();
                Console.Write("My: ");
                isOk = false;
                while (!isOk)
                {
                    Input = Console.ReadLine();
                    try { intMy = int.Parse(Input); }
                    catch { Console.Write("Type Only Number!!!\nMy: "); continue; }
                    if (intMy < 0)
                    { Console.Write("My > 0!!!\nMy: "); continue; }
                    isOk = true;
                }

                Console.Clear();
                Console.Write("Name Of Dynamic Request: ");
                isOk = false;
                while (!isOk)
                {
                    Input = Console.ReadLine();
                    bool Out = false;
                    foreach (var Char in Chars)
                    {
                        if (Input.Contains(Char))
                        { Console.Write(string.Format("Name Of Dynamic Request Does Not Contain '{0}'!!!\nName Of Dynamic Request: ", Char)); Out = true; break; }
                    }
                    if (Out)
                        continue;
                    _DynamicRequestName = Input;
                    isOk = true;
                }
            }

            Console.Clear();
            Console.Write("Number Of Requests Per File Will Be Created: ");
            isOk = false;
            int intNumberOfRequest = 0;
            while (!isOk)
            {
                Input = Console.ReadLine();
                try { intNumberOfRequest = int.Parse(Input); }
                catch { Console.Write("Type Only Number!!!\nNumber Of Requests Per File Will Be Created: "); continue; }
                if (intNumberOfRequest < 1)
                { Console.Write("Number Of Requests Must Be Larger Than 0!!!\nNumber Of Requests Per File Will Be Created: "); continue; }
                isOk = true;
            }

            string TopologyName = "";
            int[,] IE = new int[5, 2];
            int[] P_IE_Topology = new int[5];
            switch (intTopology)
            {
                case 1:
                    TopologyName = NAME_MIRA;
                    P_IE_Topology = new int[IE_MIRA.GetLength(0)];
                    IE = IE_MIRA;
                    break;
                case 2:
                    TopologyName = NAME_CESNET;
                    P_IE_Topology = new int[IE_CESNET.GetLength(0)];
                    IE = IE_CESNET;
                    break;
                case 3:
                    TopologyName = NAME_ANSNET;
                    P_IE_Topology = new int[IE_ANSNET.GetLength(0)];
                    IE = IE_ANSNET;
                    break;
            }

            Console.Clear();
            Console.Write(string.Format("Percent Of Per IE In {0}\n", TopologyName));
            bool ArrayIsOk = false;
            do
            {
                for (int i = 0; i < P_IE_Topology.Length; i++)
                {
                    Console.Write(string.Format("Percent IE-{0}: ", i + 1));
                    isOk = false;
                    int intTemp = 0;
                    while (!isOk)
                    {
                        Input = Console.ReadLine();
                        try { intTemp = int.Parse(Input); }
                        catch { Console.Write(string.Format("Type Only Number!!!\nPercent IE-{0}: ", i + 1)); continue; }
                        if (intTemp > 100 || intTemp < 1)
                        { Console.Write(string.Format("Type Only Between 1 And 100!!!\nPercent IE-{0}: ", i + 1)); continue; }
                        P_IE_Topology[i] = intTemp;
                        isOk = true;
                    }
                }
                if (CheckPercentArray(P_IE_Topology))
                    ArrayIsOk = true;
                else
                {
                    ArrayIsOk = false;
                    Console.Write("Percent Of Per IE Error, Try Again!!!\n");
                }
            }
            while (!ArrayIsOk);

            Console.Clear();
            Console.Write("(Percent Of Per IE) Name: ");
            isOk = false;
            string NameOfP = "";
            while (!isOk)
            {
                Input = Console.ReadLine();
                bool Out = false;
                foreach (var Char in Chars)
                {
                    if (Input.Contains(Char))
                    { Console.Write(string.Format("(Percent Of Per IE) Name Does Not Contain '{0}'!!!\n(Percent Of Per IE) Name: ", Char)); Out = true; break; }
                }
                if (Out)
                    continue;
                NameOfP = Input;
                isOk = true;
            }
            PercentUse _PercentUse = new PercentUse(P_IE_Topology, NameOfP);

            Bandwidth _Bandwidth = null;
            Console.Clear();
            Console.Write("Type Of Bandwith (Section: 1; Segment Array: 2; Array: 3)\nYour Chose: ");
            isOk = false;
            int intTypeOfBandwidth = 0;
            while (!isOk)
            {
                Input = Console.ReadLine();
                try { intTypeOfBandwidth = int.Parse(Input); }
                catch { Console.Write("Type Only 1, 2 Or 3!!!\nYour Chose: "); continue; }
                if (intTypeOfBandwidth > 3 || intTypeOfBandwidth < 1)
                { Console.Write("Type Only 1, 2 Or 3!!!\nYour Chose: "); continue; }
                isOk = true;
            }

            int intFirst = 0, intLast = 0, intJumb = 0;
            int[] BWArray = new int[5];
            switch (intTypeOfBandwidth)
            {
                case 1:
                    Console.Clear();
                    Console.Write("First: ");
                    isOk = false;
                    while (!isOk)
                    {
                        Input = Console.ReadLine();
                        try { intFirst = int.Parse(Input); }
                        catch { Console.Write("Type Only Number!!!\nFirst: "); continue; }
                        if (intFirst < 1)
                        { Console.Write("First Must Be Larger Than 0!!!\nFirst: "); continue; }
                        isOk = true;
                    }

                    Console.Clear();
                    Console.Write("Last: ");
                    isOk = false;
                    while (!isOk)
                    {
                        Input = Console.ReadLine();
                        try { intLast = int.Parse(Input); }
                        catch { Console.Write("Type Only Number!!!\nLast: "); continue; }
                        if (intLast < intFirst)
                        { Console.Write("Last Is Larger Than First!!!\nLast: "); continue; }
                        isOk = true;
                    }
                    _Bandwidth = new Bandwidth(intFirst, intLast);
                    break;
                case 2:
                    ArrayIsOk = false;

                    do
                    {
                        Console.Clear();
                        Console.Write("First: ");
                        isOk = false;
                        while (!isOk)
                        {
                            Input = Console.ReadLine();
                            try { intFirst = int.Parse(Input); }
                            catch { Console.Write("Type Only Number!!!\nFirst: "); continue; }
                            isOk = true;
                        }

                        Console.Clear();
                        Console.Write("Last: ");
                        isOk = false;
                        while (!isOk)
                        {
                            Input = Console.ReadLine();
                            try { intLast = int.Parse(Input); }
                            catch { Console.Write("Type Only Number!!!\nLast: "); continue; }
                            if (intLast < intFirst)
                            { Console.Write("Last Is Larger Than First!!!\nLast: "); continue; }
                            isOk = true;
                        }

                        Console.Clear();
                        Console.Write("Jumb: ");
                        isOk = false;
                        while (!isOk)
                        {
                            Input = Console.ReadLine();
                            try { intJumb = int.Parse(Input); }
                            catch { Console.Write("Type Only Number!!!\nJumb: "); continue; }
                            isOk = true;
                        }

                        ArrayIsOk = CreateSegmentArray(intFirst, intLast, intJumb, ref BWArray);
                        if (!ArrayIsOk)
                        {
                            Console.Clear();
                            Console.Write(string.Format("First = {0}, Last = {1}, Jumb = {2} Are Not Incorrect!!!\nPress Any Key To Try Again\n", intFirst, intLast, intJumb));
                            Console.ReadKey();
                        }
                    }
                    while (!ArrayIsOk);
                    _Bandwidth = new Bandwidth(BWArray, intJumb);
                    break;
                case 3:
                    Console.Clear();
                    Console.Write("Length Of Array: ");
                    int intLength = 0;
                    isOk = false;
                    while (!isOk)
                    {
                        Input = Console.ReadLine();
                        try { intLength = int.Parse(Input); }
                        catch { Console.Write("Type Only Number!!!\nLength Of Array: "); continue; }
                        isOk = true;
                    }

                    BWArray = new int[intLength];
                    for (int i = 0; i < intLength; i++)
                    {
                        Console.Write(string.Format("BW-{0}: ", i + 1));
                        isOk = false;
                        int intTemp = 0;
                        while (!isOk)
                        {
                            Input = Console.ReadLine();
                            try { intTemp = int.Parse(Input); }
                            catch { Console.Write(string.Format("Type Only Number!!!\nBW-{0}: ", i + 1)); continue; }
                            if (intTemp < 1)
                            { Console.Write(string.Format("Value Must Be Larger Than 0!!!\nBW-{0}: ", i + 1)); continue; }
                            BWArray[i] = intTemp;
                            isOk = true;
                        }
                    }
                    _Bandwidth = new Bandwidth(BWArray);
                    break;
            }

            Console.Clear();
            Console.WriteLine("###############\n# INFORMATION #\n###############\n");
            Console.WriteLine(string.Format("Topology: {0}\nNumber Of Files: {1}\nType Of Request: {2}", TopologyName, NumberOfFiles, intTypeOfRequest == 1 ? "Static" : "Dynamic"));
            if (intTypeOfRequest == 2)
                Console.WriteLine(string.Format("Name Of Dynamic Request: {0}\nLamda: {1}\nMy: {2}", _DynamicRequestName, intLamda, intMy));
            Console.WriteLine(string.Format("Number Of Requests Per File: {0}", intNumberOfRequest));

            Input = "";
            switch (intTopology)
            {
                case 1:
                    for (int i = 0; i < IE_MIRA.GetLength(0); i++)
                    {
                        Input += string.Format("({0}-{1}); ", IE_MIRA[i, 0], IE_MIRA[i, 1]);
                    }
                    break;
                case 2:
                    for (int i = 0; i < IE_CESNET.GetLength(0); i++)
                    {
                        Input += string.Format("({0}-{1}); ", IE_CESNET[i, 0], IE_CESNET[i, 1]);
                    }
                    break;
                case 3:
                    for (int i = 0; i < IE_ANSNET.GetLength(0); i++)
                    {
                        Input += string.Format("({0}-{1}); ", IE_ANSNET[i, 0], IE_ANSNET[i, 1]);
                    }
                    break;
            }
            if (Input.Length > 2)
                Input = Input.Substring(0, Input.Length - 2);
            Console.WriteLine(string.Format("I-E: {0}", Input));

            Input = "";
            for (int i = 0; i < P_IE_Topology.Length; i++)
            {
                Input += string.Format("{0}%; ", P_IE_Topology[i]);
            }
            if (Input.Length > 2)
                Input = Input.Substring(0, Input.Length - 2);
            Console.WriteLine(string.Format("Percent Of Per IE In {0}: {1}", TopologyName, Input));
            Console.WriteLine(string.Format("(Percent Of Per IE) Name: {0}", _PercentUse.Name));

            Input = "";
            switch (intTypeOfBandwidth)
            {
                case 1:
                    Input = string.Format("Section [{0}-{1}]", intFirst, intLast);
                    break;
                case 2:
                    Input = string.Format("Segment Array [{0}][{1}-{2}]", intJumb, intFirst, intLast);
                    break;
                case 3:
                    Input = "Array ";
                    for (int i = 0; i < BWArray.Length; i++)
                        Input += string.Format("{0}; ", BWArray[i]);
                    if (Input.Length > 2)
                        Input = Input.Substring(0, Input.Length - 2);
                    break;
            }
            Console.WriteLine(string.Format("Bandwidth: {0}", Input));
            #endregion

            Console.Write("\nType Y To Create Files, Type N To Exit Program\nYour Chose: ");
            isOk = false;
            while (!isOk)
            {
                Input = Console.ReadLine();
                Input = Input.ToLower();
                if (!(Object.Equals(Input, "y") || Object.Equals(Input, "n")))
                { Console.Write("Only Y Or N!!!\nYour Chose: "); continue; }
                isOk = true;
            }
            if (Object.Equals(Input, "y"))
            {
                Console.Clear();
                Console.WriteLine("Processing...");
                Configuration _Configuration = new Configuration(TopologyName, IE, intTypeOfRequest, intNumberOfRequest, _PercentUse, _Bandwidth);

                //string strDirectory = string.Format("KQ\\{0}_{1}IE", TopologyName, IE.GetLength(0));
                string strDirectory = string.Format("KQ\\{0}", TopologyName);
                if (!Directory.Exists(strDirectory))
                {
                    Directory.CreateDirectory(strDirectory);
                }
                string FILENAME = "";
                if (intTypeOfRequest == 1)//Static
                {
                    for (int i = 0; i < NumberOfFiles; i++)
                    {
                        //FILENAME = string.Format("{0}\\{1}_{2}IE_{3}_{4}_{5}_{6}.txt", strDirectory, TopologyName, IE.GetLength(0), _Configuration.RequestTypeName, _Configuration.PercentUse.Name, _Configuration.Bandwidth.Name, i + 1);
                        FILENAME = string.Format("{0}\\{1}_{2}_{3}_{4}_{5}.txt", strDirectory, TopologyName, _Configuration.RequestTypeName, _Configuration.PercentUse.Name, _Configuration.Bandwidth.Name, i + 1);
                        Console.WriteLine(string.Format("Creating {0}", FILENAME));
                        StaticScenario _StaticScenario = new StaticScenario(_Configuration, 5);
                        _StaticScenario.GenerateNEW(FILENAME);
                    }
                }
                else if (intTypeOfRequest == 2)//Dynamic
                {
                    for (int i = 0; i < NumberOfFiles; i++)
                    {
                        //FILENAME = string.Format("{0}\\{1}_{2}IE_{3}_{4}_{5}_{6}_{7}.txt", strDirectory, TopologyName, IE.GetLength(0), _Configuration.RequestTypeName, _DynamicRequestName, _Configuration.PercentUse.Name, _Configuration.Bandwidth.Name, i + 1);
                        FILENAME = string.Format("{0}\\{1}_{2}_{3}_{4}_{5}_{6}.txt", strDirectory, TopologyName, _Configuration.RequestTypeName, _DynamicRequestName, _Configuration.PercentUse.Name, _Configuration.Bandwidth.Name, i + 1);
                        Console.WriteLine(string.Format("Creating {0}", FILENAME));
                        DynamicScrenario _DynamicScrenario = new DynamicScrenario(_Configuration, 400, intLamda, intMy);
                        _DynamicScrenario.GenerateNEW(FILENAME);
                    }
                }
                Console.WriteLine("All Done!!!, Press Any Key To Exit!!!");
                Console.ReadKey();
            }
        }

        public static bool CheckPercentArray(int[] Array)
        {
            int Count = 0;
            for (int i = 0; i < Array.Length; i++)
            {
                Count += Array[i];
                if (Count > 100)
                    return false;
            }
            if (Count == 100)
                return true;
            return false;
        }

        public static bool CreateSegmentArray(int First, int Last, int Jumb, ref int[] Array)
        {
            if (First < 0 || Last < 0 || Jumb < 0)
                return false;
            if (First > Last)
                return false;
            if (Jumb != 0)
            {
                if (((Last - First) % Jumb) != 0)
                    return false;
                else
                {
                    Array = new int[((Last - First) / Jumb) + 1];
                    for (int i = 0; i < Array.Length; i++)
                    {
                        if (i == 0)
                            Array[i] = First;
                        else
                            Array[i] = Array[i - 1] + Jumb;
                    }
                    return true;
                }
            }
            else
            {
                if (First != Last)
                    return false;
                else
                {
                    Array = new int[1];
                    Array[0] = First;
                    return true;
                }
            }
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
