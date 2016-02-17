using System.Diagnostics;

namespace ArwicEngine.Core
{
    public class Timer
    {
        private Stopwatch stopwatch;
        public float Length { get; set; }

        public Timer(float length)
        {
            stopwatch = new Stopwatch();
                stopwatch.Start();
            Length = length;
        }

        public void Start()
        {
            stopwatch.Restart();
        }

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
