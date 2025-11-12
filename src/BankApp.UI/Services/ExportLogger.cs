using System;
using System.IO;

namespace BankApp.UI.Services
{
    /// <summary>
    /// Writes debug and error messages to a log file in the application directory.
    /// </summary>
    public static class ExportLogger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export.log");

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            var fullMessage = ex == null ? message : $"{message}\nException: {ex}";
            WriteLog("ERROR", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
                File.AppendAllLines(LogFilePath, new[] { line });
            }
            catch
            {
                // Swallow any logging errors to avoid interfering with UI.
            }
        }
    }
}
