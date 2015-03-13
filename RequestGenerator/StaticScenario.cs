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

        public StaticScenario(int[,] D, int[] P,  int[] B, int timeUnit, int numberOfRequest, int periodIncomingTime)
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
    }
}