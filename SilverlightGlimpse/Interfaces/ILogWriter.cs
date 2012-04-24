using System.Collections;

namespace SilverlightGlimpse.Interfaces
{
    public interface ILogWriter
    {
        void Write(IList log, int maxLogItems, string text, params object[] args);
    }
}