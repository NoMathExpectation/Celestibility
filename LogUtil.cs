using Celeste.Mod;

namespace NoMathExpectation.Celeste.Celestibility
{
    internal class LogUtil
    {
        public static void log(string message, LogLevel level = LogLevel.Info, bool stacktrace = false, string prefix = "Celestibility")
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
    }
}
