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
    }
}