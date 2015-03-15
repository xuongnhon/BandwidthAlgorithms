using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Troschuetz.Random;

namespace RequestGenerator
{
    class StaticScenario : Screnario
    {
        private int periodIncomingTime;
        private RequestGenerator.Program.Configuration _Configuration;

        public StaticScenario(int[,] D, int[] P, int[] B, int timeUnit, int numberOfRequest, int periodIncomingTime)
            : base(D, P, B, timeUnit, numberOfRequest)
        {
            this.periodIncomingTime = periodIncomingTime;
        }

        public override void Generate(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(file);

            //StandardGenerator generator = new StandardGenerator();

            DiscreteUniformDistribution randomForB =
                new DiscreteUniformDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForB.Beta = B.Length - 1;
            randomForB.Alpha = 0;

            DiscreteUniformDistribution randomForD =
                new DiscreteUniformDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForD.Beta = 99;
            randomForD.Alpha = 0;

            int d, b;

            for (int i = 0; i < numberOfRequest; i++)
            {
                d = GetDemand(randomForD.Next());
                b = randomForB.Next();

                Request req = new Request(i, D[d, 0], D[d, 1], B[b], periodIncomingTime * i, int.MaxValue);

                wr.WriteLine(req);
                Console.WriteLine(req);
            }

            wr.Close();
            file.Close();
        }

        public StaticScenario(RequestGenerator.Program.Configuration Cf, int periodIncomingTime)
        {
            this._Configuration = Cf;
            this.D = _Configuration.IE;
            this.P = _Configuration.PercentUse.P;
            this.numberOfRequest = _Configuration.NumberOfRequest;
            this.periodIncomingTime = periodIncomingTime;
        }

        public void GenerateNEW(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(file);

            DiscreteUniformDistribution randomForBandWidth =
                new DiscreteUniformDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            switch (_Configuration.Bandwidth.TypeOfBandwidth)
            {
                case 1:
                    randomForBandWidth.Beta = _Configuration.Bandwidth.Last;
                    randomForBandWidth.Alpha = _Configuration.Bandwidth.First;
                    break;
                case 2:
                case 3:
                    randomForBandWidth.Beta = _Configuration.Bandwidth.Array.Length - 1;
                    randomForBandWidth.Alpha = 0;
                    break;
            }


            DiscreteUniformDistribution randomForD =
                new DiscreteUniformDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForD.Beta = 99;
            randomForD.Alpha = 0;

            int d, b;

            for (int i = 0; i < numberOfRequest; i++)
            {
                d = GetDemand(randomForD.Next());//Chon I-E
                b = randomForBandWidth.Next();
                int intBandwidth = 0;
                switch (_Configuration.Bandwidth.TypeOfBandwidth)
                {
                    case 1:
                        intBandwidth = b;
                        break;
                    case 2:
                    case 3:
                        intBandwidth = _Configuration.Bandwidth.Array[b];
                        break;
                }

                Request req = new Request(i, D[d, 0], D[d, 1], intBandwidth, periodIncomingTime * i, int.MaxValue);

                wr.WriteLine(req);
                //Console.WriteLine(req);
            }

            wr.Close();
            file.Close();
        }
    }
}