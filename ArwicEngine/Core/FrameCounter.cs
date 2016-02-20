// Dominion - Copyright (C) Timothy Ings
// FrameCounter.cs
// This file defines classes that calculate framerate

using System.Collections.Generic;
using System.Linq;

namespace ArwicEngine.Core
{
    public class FrameCounter
    {
        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }
        private const int MAXIMUSAMPLES = 100;
        private Queue<float> SampleBuffer = new Queue<float>();

        /// <summary>
        /// Updates the framerate counter
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public bool Update(float deltaTime)
        {
            // calculate the current frame rate
            CurrentFramesPerSecond = 1.0f / deltaTime;
            // add the current frame rate to the sample buffer
            SampleBuffer.Enqueue(CurrentFramesPerSecond);
            // if the sample buffer has enough samples, calculate an average from it
            if (SampleBuffer.Count > MAXIMUSAMPLES)
            {
                SampleBuffer.Dequeue();
                AverageFramesPerSecond = SampleBuffer.Average(i => i);
            }
            // if there are not enough samples yet, use the current frame rate
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }
            // keep track of values
            TotalFrames++;
            TotalSeconds += deltaTime;
            return true;
        }
    }
}
