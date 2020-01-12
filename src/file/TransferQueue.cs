using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace filewatched
{
    class TransferQueue
    {
      private static LinkedList<FileReport> files = null;
      private static ILogger _logger;

      private TransferQueue() { }

      public static void SetLogger(ILogger logger = null) {
        if (_logger == null) _logger = logger;
      }

      public static LinkedList<FileReport> GetQueue() {
        if (files == null) files = new LinkedList<FileReport>();
        return files;
      }

      public static FileReport AddFile(string file) {
        var queue = GetQueue();
        if (queue.Contains(new FileReport(file))) return null;
          _logger.LogInformation($"Adding {file}...");
        var fileReport = new FileReport(file, _logger);
        queue.AddLast(fileReport);
        return fileReport;
      }

      public static FileReport CancelFile(string filePath) {
        var fileReportNode = files.Find(new FileReport(filePath));
        if (fileReportNode == null) {
          _logger.LogWarning($"Tried to remove {filePath} but it was not in the queue.");
          return null;
        }
        files.Remove(fileReportNode);
        var fileReport = fileReportNode.Value;
        fileReport.CancelReport();
        return fileReport;
      }
    }
}
