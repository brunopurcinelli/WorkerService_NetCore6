using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using WorkerService_NetCore6_Example.Application.Interfaces;
using WorkerService_NetCore6_Example.Application.Services;
using WorkerService_NetCore6_Example.Domain.Models;

/*
    to test
    https://www.sftp.net/public-online-sftp-servers
    https://www.rebex.net/getfile/47054bd760534fd6a20a29a5247cabd4/RebexTinySftpServer-Binaries-Latest.zip/

    Example to start build a service:
    https://learn.microsoft.com/en-us/dotnet/core/extensions/windows-service

    To connect on FTP use:WINSCP
    https://winscp.net/eng/docs/introduction
 */

namespace WorkerService_NetCore6_Example
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly WorkerParameters _workerParameters;
        private readonly IWorkerAppService _workerAppService;
        private bool _inProccess = false;

        public Worker(IConfiguration configuration, ILogger<Worker> logger, IWorkerAppService workerAppService)
        {
            _logger = logger;

            _workerAppService = workerAppService;
            _workerParameters = new WorkerParameters();
            new ConfigureFromConfigurationOptions<WorkerParameters>(configuration.GetSection("WorkerParameters")).Configure(_workerParameters);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            InsertLog($"Worker startup in: {DateTimeOffset.Now.ToLocalTime()}");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_inProccess)
                    continue;

                InsertLog(message: $"Worker in: {DateTimeOffset.Now.ToLocalTime()}");

                try
                {
                    _inProccess = true;
                    await _workerAppService.Process();
                    _inProccess = false;
                }
                catch (AggregateException e)
                {
                    var errors = new StringBuilder();

                    for (int j = 0; j < e.InnerExceptions.Count; j++)
                    {
                        errors.Append($"{e.InnerExceptions[j].ToString()} / ");
                    }
                    InsertLog(message: $"Worker Exception: {errors.ToString()}", exception: e);
                }

                await Task.Delay(TimeSpan.FromMinutes(_workerParameters.LoopService), stoppingToken);
            }
            InsertLog($"Worker finish in: {DateTimeOffset.Now.ToLocalTime()}");
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            InsertLog(message: $"Worker Start: {DateTimeOffset.Now.ToLocalTime()}");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            InsertLog(message: $"Worker Stop: {DateTimeOffset.Now.ToLocalTime()}");
            return base.StopAsync(cancellationToken);
        }

        protected void InsertLog(string message = "", Exception exception = null)
        {
            if (exception == null)
            {
                var logJson = JsonConvert.SerializeObject(message);
                _logger.LogInformation(1000, logJson);
            }
            else
            {
                var logExJson = JsonConvert.SerializeObject(exception.InnerException == null ? $"{exception.Message}|" : $"{exception.InnerException.Message}|");
                _logger.LogCritical(1001, new Exception($"{exception.Message} : {exception}"), logExJson);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}