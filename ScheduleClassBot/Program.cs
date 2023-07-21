using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.StartupServiceSettings;

Logger logger = LogManager.GetCurrentClassLogger();

try
{
    using IHost host = Host.CreateDefaultBuilder()
        .ConfigureHostConfiguration(cfgBuilder => { cfgBuilder.AddJsonFile("appsettings.json"); })
        .ConfigureServices(
            services => { services.AddHostedService<BotRunService>(); })
        .UseWindowsService(options => { options.ServiceName = ".NET StockBot"; })
        .Build();

    await host.RunAsync();
    logger.Info("Stopping app success");
}
catch (Exception ex)
{
    logger.Error("Error running app. {method}: {error}", nameof(Program), ex);
}