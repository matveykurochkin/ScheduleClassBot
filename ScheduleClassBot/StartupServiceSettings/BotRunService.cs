using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ScheduleClassBot.StartupServiceSettings;

internal class BotRunService : IHostedService
{
    private readonly IConfiguration _cfg;

    public BotRunService(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var handler = new MainHandler();
            var token = _cfg.GetSection("Tokens")["TelegramBotToken"];
            var telegramBot = new TelegramBotClient(token!);

            Logger.Info(
                $"Бот {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.FirstName} успешно запущен!");
            var cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions();

            telegramBot.StartReceiving(
                new DefaultUpdateHandler(handler.HandleUpdateAsync, handler.HandleErrorAsync),
                receiverOptions,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            Logger.Error($"Bot not started! Error message: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Service stopping");
        return Task.CompletedTask;
    }
}