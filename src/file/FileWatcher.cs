using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace filewatched {
  public class FileWatcher
  {
    private readonly ILogger _logger;
    private readonly Queue<string> _changedFiles;
    private readonly string _path;
    private readonly CancellationToken _cancellationToken;

    public FileWatcher(string Path, ILogger logger, CancellationToken cancellationToken)
    {
      _changedFiles = new Queue<string>();
      _path = Path;
      _logger = logger;
      _cancellationToken = cancellationToken;
      TransferQueue.SetLogger(logger);
    }

    public void StartWatch()
    {
        // Create a new FileSystemWatcher and set its properties.
        using (FileSystemWatcher watcher = new FileSystemWatcher())
        {
            watcher.Path = _path;

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;

            // Only watch text files.
            watcher.Filter = "*";

            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            // watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            // watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            while ( ! _cancellationToken.IsCancellationRequested) ;
        }
    }

    // Define the event handlers.
    private void OnChanged(object source, FileSystemEventArgs e)
    {
        var fullPath = e.FullPath;
        var changeType = e.ChangeType;

        // Ignore system files
        var fileName = fullPath.Split('/').Last();
        if (Regex.IsMatch(fileName, @"^\.", RegexOptions.IgnoreCase)) return;
        switch (changeType)
        {
          case WatcherChangeTypes.Created:
            AddFileToQueue(fullPath);
            break;
          case WatcherChangeTypes.Deleted:
            RemoveFileFromQueue(fullPath);
            break;
        }
    }

    private void AddFileToQueue(string filePath)
    {
      _logger.LogInformation($"Adding {filePath} to the transfer queue.");
      TransferQueue.AddFile(filePath);

    }

    private void RemoveFileFromQueue(string filePath)
    {
      _logger.LogInformation($"Removing {filePath} from the transfer queue.");
      TransferQueue.CancelFile(filePath);
    }

    private void OnRenamed(object source, RenamedEventArgs e) =>
        // Specify what is done when a file is renamed.
        _logger.LogInformation($"File: {e.OldFullPath} renamed to {e.FullPath}");
}
}