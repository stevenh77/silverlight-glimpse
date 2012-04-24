using System;
using System.Collections;
using SilverlightGlimpse.Interfaces;

namespace SilverlightGlimpse.Services
{
    public class LogWriter : ILogWriter
    {
        public void Write(IList log, int maxLogItems, string text, params object[] args)
        {
            var date = DateTime.Now.ToString("HH:mm:ss.ffff");
            log.Add(string.Concat(date, "\t", string.Format(text, args)));
            if (log.Count > maxLogItems) log.RemoveAt(0);
        }
    }
}
