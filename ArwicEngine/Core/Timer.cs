// Dominion - Copyright (C) Timothy Ings
// SerializationHelper.cs
// This file contains classes that define a timer

using System.Diagnostics;

namespace ArwicEngine.Core
{
    public class Timer
    {
        private Stopwatch stopwatch;

        /// <summary>
        /// Gets or sets a value that indicates the length of time before the timer will expire
        /// </summary>
        public float Length { get; set; }

        /// <summary>
        /// Creates a new timer with a given length
        /// </summary>
        /// <param name="length"></param>
        public Timer(float length)
        {
            stopwatch = new Stopwatch();
                stopwatch.Start();
            Length = length;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        public void Start()
        {
            stopwatch.Restart();
        }

        /// <summary>
        /// Returns true is the time rhas expired
        /// </summary>
        /// <returns></returns>
        public bool Expired()
        {
            if (stopwatch.ElapsedMilliseconds >= Length)
            {
                stopwatch.Stop();
                return true;
            }
            return false;
        }
    }
}
