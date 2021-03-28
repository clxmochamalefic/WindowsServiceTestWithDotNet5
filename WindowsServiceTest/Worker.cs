using Common;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        /// <summary>
        /// GRPCのサービス
        /// </summary>
        private CommanderService _grpcService;

        /// <summary>
        /// GRPCのサーバ
        /// </summary>
        private Server _grpcServer;

        private Dictionary<DateTime, string> _weathers = new();

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            // grpcServiceとgrpcServerを立ち上げる
            _grpcService = new();
            _grpcService.commanderServiceDelegate = FetchWeather;

            _grpcServer = new()
            {
                Services = { Commander.BindService(_grpcService) },
                Ports = { new("127.0.0.1", 50000, ServerCredentials.Insecure) }
            };
            _grpcServer.Start();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = new HttpClient();
                var response = await client.SendAsync(new(HttpMethod.Get, @"https://www.jma.go.jp/bosai/forecast/data/forecast/130000.json"));
                var body = await response?.Content.ReadAsStringAsync() ?? "";
                _weathers.Add(DateTime.Now, body);
                _logger.LogInformation("response content: " + body);
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(360000, stoppingToken);
            }
        }

        public Response FetchWeather(int hourAgo)
        {
            var orderedKeys = _weathers.Keys.OrderByDescending(x => x);
            if (orderedKeys.Count() < hourAgo)
            {
                return new()
                {
                    ReportedAt = new(),
                    Weather = string.Empty
                };
            }

            var findKey = orderedKeys.ElementAt(hourAgo);
            return new()
            {
                ReportedAt = Timestamp.FromDateTime(findKey.ToUniversalTime()),
                Weather = _weathers[findKey]
            };
        }
    }
}
