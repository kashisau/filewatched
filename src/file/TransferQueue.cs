using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace filewatched
{
    class TransferQueue
    {
      private static List<FileReport> files = null;
      private static ILogger _logger;

      private TransferQueue() { }

      public static void SetLogger(ILogger logger = null) {
        if (_logger == null) _logger = logger;
      }

      public static List<FileReport> GetQueue() {
        if (files == null) files = new List<FileReport>();
        return files;
      }

      public static FileReport AddFile(string file) {
        var queue = GetQueue();
        if (queue.Contains(new FileReport(file))) return null;
          _logger.LogInformation($"Adding {file}...");
        var fileReport = new FileReport(file, _logger);
        queue.Add(fileReport);
        return fileReport;
      }

      public static FileReport CancelFile(string filePath) {
        var fileReport = files.Find(x => x.FilePath == filePath);
        if (fileReport == null) {
          _logger.LogWarning($"Tried to remove {filePath} but it was not in the queue.");
          return null;
        }
        fileReport.CancelReport();
        files.Remove(fileReport);
        return fileReport;
      }
    }
}
