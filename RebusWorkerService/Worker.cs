using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyHandlers;
using MyMessages;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;
using Rebus.Transport.InMem;

namespace RebusWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        IServiceCollection _services = new ServiceCollection();
        private IBus _bus;


        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false).Build();

            var busConfig = config.GetSection("Bus").Get<BusConfig>();

            _services.AddRebus(rebusConfigurer =>
            {
                return ConfigureRebus(rebusConfigurer, busConfig);
            });

            _services.AutoRegisterHandlersFromAssemblyOf<MyHandler>();
        }


        private RebusConfigurer ConfigureRebus(RebusConfigurer rebusConfigurer, BusConfig busConfig)
        {
            

            rebusConfigurer.Routing(x =>
                x.TypeBased()
                    .MapAssemblyOf<MyMessage>(busConfig.MainQueue)
            );

            rebusConfigurer.Transport(configurer => configurer.UseInMemoryTransport(new InMemNetwork(), busConfig.MainQueue));
            rebusConfigurer.Subscriptions(configurer => configurer.StoreInMemory());
            rebusConfigurer.Sagas(standardConfigurer => standardConfigurer.StoreInMemory());

            return rebusConfigurer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var provider = _services.BuildServiceProvider();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                provider.UseRebus(bus => _bus = bus);
                await _bus.Send(new MyMessage());

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
