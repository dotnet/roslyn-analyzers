using System.Runtime.CompilerServices;
using log4net;

namespace FileIssues
{
    public static class LogHelper
    {
        public static ILog GetLogger([CallerFilePath] string fileName = "")
        {
            return LogManager.GetLogger(fileName);
        }
    }
}
