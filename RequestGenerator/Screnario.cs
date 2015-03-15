using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Troschuetz.Random;

namespace RequestGenerator
{
    abstract class Screnario
    {
        protected int[,] D;
        protected int[] B;
        protected int[] P;
        protected int timeUnit;
        protected int numberOfRequest;

        public Screnario(int[,] D, int[] P, int[] B, int timeUnit, int numberOfRequest)
        {
            this.P = P;
            this.D = D;
            this.B = B;
            this.timeUnit = timeUnit;
            this.numberOfRequest = numberOfRequest;
        }

        protected int GetDemand(int randomNumber)
        {
            int alpha = 0;
            int beta = 0;
            int result = -1;
            for (int i = 0; i < P.Length; i++)
            {
                beta = alpha + P[i];
                if (randomNumber >= alpha && randomNumber < beta)
                {
                    result = i;
                    break;
                }
                alpha = beta;
            }

            return result;
        }

        public virtual void Generate(string filename) { }

        public Screnario() { }
    }
}
