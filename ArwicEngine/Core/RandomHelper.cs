// Dominion - Copyright (C) Timothy Ings
// RandomHelper.cs
// This file defines utility methods that provide pseudo random number generation with a single Random object for the best results

using System;

namespace ArwicEngine.Core
{
    public static class RandomHelper
    {
        /// <summary>
        /// The pseudo random number generator
        /// </summary>
        public static Random Random { get; private set; }

        /// <summary>
        /// Initialises the pseudo random number generator
        /// </summary>
        public static void Init()
        {
            Random = new Random();
        }

        /// <summary>
        /// Returns a non-negative random integer
        /// </summary>
        /// <returns></returns>
        public static int Next()
        {
            return Random.Next();
        }

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int maxValue)
        {
            return Random.Next(maxValue);
        }

        /// <summary>
        /// Returns a non-negative random integer that is within a specified range
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static int Next(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers
        /// </summary>
        /// <param name="buffer"></param>
        public static void NextBytes(byte[] buffer)
        {
            Random.NextBytes(buffer);
        }

        /// <summary>
        /// Returns a random floating point number that is greater than or equal to 0.0, and less than 1.0
        /// </summary>
        /// <returns></returns>
        public static double NextDouble()
        {
            return Random.NextDouble();
        }

        /// <summary>
        /// Has a given chance to return true
        /// </summary>
        /// <param name="chance">the chance of success, between 0.0 and 1.0</param>
        /// <returns></returns>
        public static bool Roll(double chance)
        {
            return NextDouble() <= chance;
        }
    }
}
