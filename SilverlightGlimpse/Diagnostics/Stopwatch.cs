using System;

namespace SilverlightGlimpse.Diagnostics
{
    public class Stopwatch
    {
        private long _start;
        private long _end;

        public void Start()
        {
            _start = DateTime.Now.Ticks;
        }

        public void Stop()
        {
            _end = DateTime.Now.Ticks;
        }

        public TimeSpan ElapsedTime { get { return new TimeSpan(_end - _start); }}

        public static Stopwatch StartNew()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }
    }
}