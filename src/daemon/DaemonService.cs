using System;
 using System.Threading;
 using System.Threading.Tasks;
 using Microsoft.Extensions.Hosting;
 using Microsoft.Extensions.Logging;
 using Microsoft.Extensions.Options;

namespace filewatched
{
    public class DaemonService : IHostedService, IDisposable
    {
      private readonly ILogger _logger;
      private readonly IOptions<DaemonConfig> _config;
      private FileWatcher _watcher;
      public DaemonService(ILogger<DaemonService> logger, IOptions<DaemonConfig> config)
      {
          _logger = logger;
          _config = config;
      }

      public async Task StartAsync(CancellationToken cancellationToken)
      {
          var path = _config.Value.Path;
          var daemonName = _config.Value.DaemonName;
          if (_watcher == null) _watcher = new FileWatcher(path, _logger, cancellationToken);
          var scanner = new FileScanner(path, _logger);
          var files = await scanner.Scan();
          Thread t = new Thread(delegate ()
              {
                  // replace the IP with your system IP Address...
                  Server myserver = new Server("0.0.0.0", 13000, _logger, files, cancellationToken);
              }
          );
          t.Start();
          _logger.LogInformation($"Starting daemon: {daemonName} on directory {path}.");
          var watchTask = Task.Run(() => _watcher.StartWatch());
          return;
      }

      public Task StopAsync(CancellationToken cancellationToken)
      {
          _logger.LogInformation("Stopping daemon.");
          return Task.CompletedTask;
      }

      public void Dispose()
      {
          _logger.LogInformation("Disposing....");

      }
    }
}