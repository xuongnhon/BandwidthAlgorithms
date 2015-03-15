using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Troschuetz.Random;

namespace RequestGenerator
{
    class DynamicScrenario : Screnario
    {
        private double lamda;
        private double mu;
        private RequestGenerator.Program.Configuration _Configuration;

        public DynamicScrenario(int[,] D, int[] P, int[] B, int timeUnit, int numberOfRequest, double lamda, double mu)
            : base(D, P, B, timeUnit, numberOfRequest)
        {
            this.lamda = lamda;
            this.mu = mu;
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

            ExponentialDistribution randomForHoldingTime =
                new ExponentialDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForHoldingTime.Lambda = 1 / mu;

            PoissonDistribution randomForNumberOfReq =
                new PoissonDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForNumberOfReq.Lambda = lamda;

            int d, b, reqCount = 0, numOfReqPerTimeUnit, time = 0;
            double holdingTime, incomingTime;

            while (reqCount < numberOfRequest)
            {
                numOfReqPerTimeUnit = randomForNumberOfReq.Next();
                if (reqCount + numOfReqPerTimeUnit > numberOfRequest)
                    numOfReqPerTimeUnit = numberOfRequest - reqCount;

                for (int i = 0; i < numOfReqPerTimeUnit; i++)
                {
                    d = GetDemand(randomForD.Next());
                    b = randomForB.Next();
                    holdingTime = randomForHoldingTime.NextDouble() * timeUnit;
                    incomingTime = time * timeUnit + i * (timeUnit / numOfReqPerTimeUnit);
                    Request req = new Request(reqCount, D[d, 0], D[d, 1], B[b], (long)incomingTime, (long)holdingTime);
                    Console.WriteLine(req);
                    wr.WriteLine(req);
                    reqCount++;
                }
                time++;
            }
            wr.Close();
            file.Close();
        }

        public DynamicScrenario(RequestGenerator.Program.Configuration Cf, int timeUnit, double lamda, double mu)
        {
            this._Configuration = Cf;
            this.D = _Configuration.IE;
            this.P = _Configuration.PercentUse.P;
            this.timeUnit = timeUnit;
            this.lamda = lamda;
            this.mu = mu;
            this.numberOfRequest = _Configuration.NumberOfRequest;
        }

        public void GenerateNEW(string filename)
        {
            FileStream file = new FileStream(filename, FileMode.Create);
            StreamWriter wr = new StreamWriter(file);

            //StandardGenerator generator = new StandardGenerator();

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

            ExponentialDistribution randomForHoldingTime =
                new ExponentialDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForHoldingTime.Lambda = 1 / mu;

            PoissonDistribution randomForNumberOfReq =
                new PoissonDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForNumberOfReq.Lambda = lamda;

            int d, b, reqCount = 0, numOfReqPerTimeUnit, time = 0;
            double holdingTime, incomingTime;

            while (reqCount < numberOfRequest)
            {
                numOfReqPerTimeUnit = randomForNumberOfReq.Next();
                if (reqCount + numOfReqPerTimeUnit > numberOfRequest)
                    numOfReqPerTimeUnit = numberOfRequest - reqCount;

                for (int i = 0; i < numOfReqPerTimeUnit; i++)
                {
                    d = GetDemand(randomForD.Next());
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

                    holdingTime = randomForHoldingTime.NextDouble() * timeUnit;
                    incomingTime = time * timeUnit + i * (timeUnit / numOfReqPerTimeUnit);
                    Request req = new Request(reqCount, D[d, 0], D[d, 1], intBandwidth, (long)incomingTime, (long)holdingTime);
                    //Console.WriteLine(req);
                    wr.WriteLine(req);
                    reqCount++;
                }
                time++;
            }
            wr.Close();
            file.Close();
        }
    }
}