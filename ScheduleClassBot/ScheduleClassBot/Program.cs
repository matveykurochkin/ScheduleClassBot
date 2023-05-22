using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.Internal;

namespace ScheduleClassBot;

class Program
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    static async Task Main(string[] args)
    {
        try
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(cfgBuilder => { cfgBuilder.AddJsonFile("appsettings.json"); })
                .ConfigureServices(
                    services => { services.AddHostedService<Run>(); })
                .UseWindowsService(options => { options.ServiceName = ".NET StockBot"; })
                .Build();

            await host.RunAsync();
            _logger.Info("Running app success");
        }
        catch (Exception ex)
        {
            _logger.Error("Error running app. {method}: {error}",nameof(Main), ex);
        }
    }
}
