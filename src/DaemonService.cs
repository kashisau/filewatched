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
      private Watcher _watcher;
      public DaemonService(ILogger<DaemonService> logger, IOptions<DaemonConfig> config)
      {
          _logger = logger;
          _config = config;
      }

      public Task StartAsync(CancellationToken cancellationToken)
      {
          _logger.LogInformation("Starting daemon: " + _config.Value.DaemonName);
          _logger.LogInformation("Starting daemon: " + _config.Value.Path);
          if (_watcher == null) _watcher = new Watcher(_config.Value.Path, _logger, cancellationToken);
          Task.Run(() => _watcher.StartWatch());
          return Task.CompletedTask;
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