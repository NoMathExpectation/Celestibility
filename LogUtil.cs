using Celeste.Mod;
using System;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class LogUtil
    {
        internal static void Log(string message, LogLevel level = LogLevel.Info, bool stacktrace = false, string prefix = "Celestibility")
        {
            if (stacktrace)
            {
                Logger.LogDetailed(level, prefix, message);
            }
            else
            {
                Logger.Log(level, prefix, message);
            }
        }

        internal static void Log(Exception exception, string prefix = "Celestibility")
        {
            Logger.LogDetailed(exception, prefix);
        }
    }
}
