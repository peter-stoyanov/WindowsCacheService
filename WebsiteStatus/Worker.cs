using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Syroot.Windows.IO;

namespace CacheService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient client;
        private FileSystemWatcher _folderWatcher;
        private readonly string _cacheFolder;
        private readonly IServiceProvider _services;

        private string _downloadsFolderPath = KnownFolders.Downloads.Path;

        public Worker(ILogger<Worker> logger, IOptions<AppSettings> settings, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
            _cacheFolder = settings.Value.CacheFolder;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //client = new HttpClient();
            //return base.StartAsync(cancellationToken);

            _logger.LogInformation("Service Starting");
            _logger.LogInformation($"Downloads folder: {_downloadsFolderPath}");
            if (!Directory.Exists(_downloadsFolderPath))
            {
                Directory.CreateDirectory(_downloadsFolderPath);
                _logger.LogWarning($"Creating the Downloads Folder [{_downloadsFolderPath}] since it doesn't exist.");
            }
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
                _logger.LogWarning($"Creating the CacheFolder [{_cacheFolder}] since it doesn't exist.");
            }

            _logger.LogInformation($"Binding Events from Downloads Folder: {_downloadsFolderPath}");
            _folderWatcher = new FileSystemWatcher(_downloadsFolderPath, "*")
            {
                NotifyFilter = NotifyFilters.CreationTime
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName,
                IncludeSubdirectories = false
            };

            _folderWatcher.Created += Input_OnCreated;
            // _folderWatcher.Changed += Input_OnChanged;
            // _folderWatcher.Deleted += Input_OnCreated;
            // _folderWatcher.Renamed += Input_OnChanged;
            _folderWatcher.Error += Input_OnError;

            _folderWatcher.EnableRaisingEvents = true;

            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // client.Dispose();
            _logger.LogInformation("The service has been stopped...");
            _folderWatcher.EnableRaisingEvents = false;
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    var result = await client.GetAsync("https://www.google.com");

            //    if (result.IsSuccessStatusCode)
            //    {
            //        _logger.LogInformation("The website is up. Status code {StatusCode}", result.StatusCode);
            //    }
            //    else
            //    {
            //        _logger.LogError("The website is down. Status code {StatusCode}", result.StatusCode);
            //    }

            //    await Task.Delay(5000, stoppingToken);
            //}
            await Task.CompletedTask;
        }

        protected void Input_OnCreated(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                try
                {
                    _logger.LogInformation($"InBound {nameof(FileSystemWatcher.Created)} Event Triggered by [{e.FullPath}]");

                    //do some work
                    //using (var scope = _services.CreateScope())
                    //{
                    //    var serviceA = scope.ServiceProvider.GetRequiredService<IServiceA>();
                    //    serviceA.Run();
                    //}

                    bool isDir = File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory);
                    if (isDir)
                    {
                        IOManager.CopyDir(e.FullPath, _cacheFolder);
                    }
                    else
                    {
                        IOManager.CopyFile(e.FullPath, _cacheFolder);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error with Inbound {nameof(FileSystemWatcher.Created)} Event");
                    _logger.LogError($"Exception: {Environment.NewLine}{ex.ToString()}");
                }
                
                _logger.LogInformation($"Done with Inbound {nameof(FileSystemWatcher.Created)} Event");
            }
        }

        protected void Input_OnError(object sender, ErrorEventArgs e)
        {
            _logger.LogInformation($"InBound {nameof(FileSystemWatcher.Error)} Event Triggered");
            _logger.LogError($"Exception: {Environment.NewLine}{e.GetException().ToString()}");
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing Service");
            if (_folderWatcher != null)
            {
                _folderWatcher.Dispose();
            }
            base.Dispose();
        }
    }
}
