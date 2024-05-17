using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using TransportHub;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = CreateHostBuilder(args).Build();
await host.RunAsync();
return;


static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(config => config.AddEnvironmentVariables())
        .UseSerilog()
        .ConfigureServices((host, services) =>
        {
            var rabbitTopologyBuilder = new RabbitMqTransportConfigurator();
            rabbitTopologyBuilder.AddExternalConfigurations(x =>
            {
                x.AddConsumer<CriticalNotificationsConsumer>();
                x.AddConsumer<HighNotificationsConsumer>();
                x.AddConsumer<MediumNotificationsConsumer>();
                x.AddConsumer<LowNotificationsConsumer>();
            });

            rabbitTopologyBuilder.UseExternalConfigurations((ctx, cfg) =>
            {
                cfg.ReceiveEndpoint("delivery-critical", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 16;
                    e.UseConcurrencyLimit(8);

                    e.ConfigureConsumer<CriticalNotificationsConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("delivery-high", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 12;
                    e.UseConcurrencyLimit(6);

                    e.ConfigureConsumer<HighNotificationsConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("delivery-medium", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 8;
                    e.UseConcurrencyLimit(4);

                    e.ConfigureConsumer<MediumNotificationsConsumer>(ctx);
                });

                cfg.ReceiveEndpoint("delivery-low", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 4;
                    e.UseConcurrencyLimit(2);

                    e.ConfigureConsumer<LowNotificationsConsumer>(ctx);
                });
            });

            services.AddConfiguredMassTransit(host.Configuration, rabbitTopologyBuilder);
        });
}
