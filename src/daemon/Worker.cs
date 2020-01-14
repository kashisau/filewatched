using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace filewatched
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerConfig _options;

        public Worker(ILogger<Worker> logger,  WorkerConfig options)
        {
            _logger = logger;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Starting {_options.DaemonName} server. Path: {_options.Path}");
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                // Enumerate the current list of files
                var watch = new Stopwatch();
                watch.Start();
                var fileScanner = new FileScanner(_options.Path, _logger);
                var files = await fileScanner.ScanAsync(stoppingToken);
                watch.Stop();
                _logger.LogInformation($"Found {files.Count} files in {watch.ElapsedMilliseconds}ms.");
                // Start the server
                var server = new Server("0.0.0.0", (Int32) 13000, _logger, files);
                await server.StartListener(stoppingToken);
                // Watch the file system
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
        }
    }
}
