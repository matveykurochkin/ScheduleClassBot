using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ScheduleClassBot.Internal;

internal class BotRun : IHostedService
{
    private readonly IConfiguration _cfg;

    public BotRun(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var handleUpdate = new ProcessingMessage();
            var token = _cfg.GetSection("Tokens")["TelegramBotToken"];
            var telegramBot = new TelegramBotClient(token!);

            Logger.Info(
                $"Бот {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.FirstName} успешно запущен!");
            var cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions();

            telegramBot.StartReceiving(
                new DefaultUpdateHandler(handleUpdate.HandleUpdateAsync, handleUpdate.HandleErrorAsync),
                receiverOptions,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            Logger.Error($"Error message: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Service stopping");
        return Task.CompletedTask;
    }
}