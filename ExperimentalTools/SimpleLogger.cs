using System;
using System.IO;

namespace ExperimentalTools
{
    public static class SimpleLogger
    {
        private const string fileName = @"";

        public static void WriteLine(string content, bool writeToDebug = false)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                File.AppendAllText(fileName, $"{DateTimeOffset.UtcNow} - {content}\r\n");
            }

            if (writeToDebug)
            {
                System.Diagnostics.Debug.WriteLine(content);
            }
        }
    }
}
