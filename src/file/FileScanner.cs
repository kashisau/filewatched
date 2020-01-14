using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.ComponentModel;

namespace filewatched
{
  // A class that is used to scan the contents of a directory, making a list of
  // all the existing files and returning them as a flat List.
  class FileScanner
  {
    private string path;
    private List<string> files;
    private ILogger _logger;
    public FileScanner(string path, ILogger logger)
    {
      this._logger = logger;
      this.path = path;
    }
    BackgroundWorker fileScanWorker;

    public async Task<List<string>> ScanAsync(CancellationToken cancellationToken) {
      fileScanWorker = new BackgroundWorker();
      fileScanWorker.WorkerReportsProgress = false;
      fileScanWorker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => files = (List<string>) e.Result;
      fileScanWorker.DoWork += Scan;
      fileScanWorker.RunWorkerAsync();

      while (files == null) {
        if (cancellationToken.IsCancellationRequested) {
          _logger.LogInformation($"Cancellation requested during file scan.");
          fileScanWorker.CancelAsync();
          fileScanWorker.Dispose();
          return null;
        }
        await Task.Delay(100);
      }
      return files;
    }
    private void Scan(object sender, DoWorkEventArgs e)
    {
      try 
      {
          // Obtain the file system entries in the directory path.
          string[] directoryEntries =
              Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories); 
          var files = new List<string>(directoryEntries);
          e.Result = files;
      }
      catch (ArgumentNullException) 
      {
          System.Console.WriteLine("Path is a null reference.");
      }
      catch (System.Security.SecurityException) 
      {
          System.Console.WriteLine("The caller does not have the " +
              "required permission.");
      }
      catch (ArgumentException) 
      {
          System.Console.WriteLine("Path is an empty string, " +
              "contains only white spaces, " + 
              "or contains invalid characters.");
      }
      catch (System.IO.DirectoryNotFoundException) 
      {
          System.Console.WriteLine("The path encapsulated in the " + 
              "Directory object does not exist.");
      }
    }
  }
}