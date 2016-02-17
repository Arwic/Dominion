using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArwicEngine.Core
{
    public static class RandomHelper
    {
        public static Random Random { get; private set; }

        public static void Init()
        {
            Random = new Random();
        }

        public static int Next()
        {
            return Random.Next();
        }

        public static int Next(int maxValue)
        {
            return Random.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            Random.NextBytes(buffer);
        }

        public static double NextDouble()
        {
            return Random.NextDouble();
        }

        public static bool Roll(double chance)
        {
            return NextDouble() <= chance;
        }
    }
}
