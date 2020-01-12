using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace filewatched
{
  class FileReport: IEquatable<FileReport>
  {
    public string FilePath { get; }
    private ReportStatus status { get; set; }
    private Task report;
    private CancellationTokenSource cancellationToken;
    private readonly ILogger _logger;


    public FileReport(string filePath) {
      FilePath = filePath;
    }

    public FileReport(string filePath, ILogger logger)
    {
      FilePath = filePath;
      _logger = logger;
      cancellationToken = new CancellationTokenSource();

      report = Task.Run(() => {
        Thread.Sleep(5000);
        if (cancellationToken.IsCancellationRequested)
        {
          status = ReportStatus.Cancelled;
          _logger.LogInformation($"Cancelled report for {filePath}.");
          return;
        }
        _logger.LogInformation($"Sending report for {filePath}...");
      });
    }

    public void CancelReport() {
      status = ReportStatus.CancelRequested;
      cancellationToken.Cancel();
    }

    public ReportStatus GetStatus() {
      return status;
    }

    public bool Equals(FileReport other)
    {
      if (other == null) return false;
      return this.FilePath.Equals(other.FilePath);
    }

    public bool Equals(string filePath)
    {
      if (filePath == null) return false;
      return this.FilePath.Equals(filePath);
    }
  }
}