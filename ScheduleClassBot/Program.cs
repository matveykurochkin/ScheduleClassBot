using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Handlers;
using ScheduleClassBot.ProcessingMethods;
using ScheduleClassBot.StartupServiceSettings;

Logger logger = LogManager.GetCurrentClassLogger();

try
{
    using IHost host = Host.CreateDefaultBuilder()
        .ConfigureHostConfiguration(cfgBuilder => { cfgBuilder.AddJsonFile("appsettings.json"); })
        .ConfigureServices(
            (hostContext, services) =>
            {
                var botSettingsConfiguration = new BotSettingsConfiguration();
                hostContext.Configuration.Bind(botSettingsConfiguration);

                services.AddSingleton(botSettingsConfiguration);
                services.AddSingleton<MessageHandler>();
                services.AddSingleton<MainHandler>();
                services.AddSingleton<GettingSpecialCommands>();
                services.AddSingleton<CallbackQueryHandler>();

                services.AddHostedService<BotRunService>();
            })
        .UseWindowsService(options => { options.ServiceName = ".NET ScheduleClassBot"; })
        .Build();

    await host.RunAsync();
    logger.Info("Stopping app success");
}
catch (Exception ex)
{
    logger.Error("Error running app. {method}: {error}", nameof(Program), ex);
}