using System.Runtime.CompilerServices;

namespace Terminal.Services
{
    public static class Logger
    {
        public static void Log(string message, [CallerFilePath] string file = "", [CallerMemberName] string func = "", [CallerLineNumber] int line = 0)
        {
            Tizen.Log.Debug(Constants.LogTag, message, file, func, line);
        }
    }
}
