using System;
using System.IO;
using Troschuetz.Random;

namespace RequestGenerator
{
    class MixScenario : Screnario
    {
        private double lamda;
        private double mu;
        private int periodIncomingTime;
        private int numberOfDynamicRequest;
        private int mixOfRequest;


        public MixScenario(int[,] D, int[] P, int[] B, int timeUnit, int numberOfStaticRequest, int numberOfDynamicRequest, double lamda, double mu, int periodIncomingTime)
            : base(D, P, B, timeUnit, numberOfStaticRequest)
        {
            this.lamda = lamda;
            this.mu = mu;
            this.periodIncomingTime = periodIncomingTime;
            this.numberOfDynamicRequest = numberOfDynamicRequest;

            this.mixOfRequest = numberOfDynamicRequest + numberOfStaticRequest;
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
            randomForD.Beta = D.Length / 2 - 1;
            randomForD.Alpha = 0;

            int d, b;

            int[] a = new int[4];

            int i = 0;
            while (i < numberOfRequest)
            {
                d = randomForD.Next();
                b = randomForB.Next();

                a[b]++;

                Request req = new Request(i, D[d, 0], D[d, 1], B[b], periodIncomingTime * i, int.MaxValue);

                wr.WriteLine(req);
                Console.WriteLine(req);
                i++;
            }

            ExponentialDistribution randomForHoldingTime =
               new ExponentialDistribution(new StandardGenerator(Guid.NewGuid().GetHashCode()));
            randomForHoldingTime.Lambda = 1 / mu;

            double holdingTime;

            for (int j = i; j < numberOfDynamicRequest + i; j++)
            {
                d = randomForD.Next();
                b = randomForB.Next();
                holdingTime = randomForHoldingTime.NextDouble() * timeUnit;

                Request req = new Request(j, D[d, 0], D[d, 1], B[b], periodIncomingTime * j, (long)holdingTime);
                Console.WriteLine(req);
                wr.WriteLine(req);
            }

            wr.Close();
            file.Close();

        }
    }
}