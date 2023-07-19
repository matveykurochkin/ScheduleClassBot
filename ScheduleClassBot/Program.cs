﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.StartupServiceSettings;

namespace ScheduleClassBot;

static class Program
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private static async Task Main()
    {
        try
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(cfgBuilder => { cfgBuilder.AddJsonFile("appsettings.json"); })
                .ConfigureServices(
                    services => { services.AddHostedService<BotRunService>(); })
                .UseWindowsService(options => { options.ServiceName = ".NET StockBot"; })
                .Build();

            await host.RunAsync();
            Logger.Info("Stopping app success");
        }
        catch (Exception ex)
        {
            Logger.Error("Error running app. {method}: {error}", nameof(Main), ex);
        }
    }
}