using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ScheduleClassBot.StartupServiceSettings;

internal class BotRunService(BotSettingsConfiguration configuration, MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler) : IHostedService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Метод, предоставляющий настройки для запуска бота
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var handler = new MainHandler(messageHandler, callbackQueryHandler);
            var token = configuration.BotToken;
            TelegramBotClient? telegramBot = default;
            if (token is { TelegramBotToken: not null })
                telegramBot = new TelegramBotClient(token.TelegramBotToken);

            if (telegramBot != null)
            {
                Logger.Info(
                    $"Бот {telegramBot.GetMeAsync(cancellationToken).Result.FirstName} успешно запущен!");
                var cts = new CancellationTokenSource();
                cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions();

                telegramBot.StartReceiving(
                    new DefaultUpdateHandler(handler.HandleUpdateAsync, handler.HandleErrorAsync),
                    receiverOptions,
                    cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Bot not started! Error message: {ex.Message}");
            Environment.Exit(100);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Метод, предоставляющий настройки для остановки бота
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Service stopping");
        return Task.CompletedTask;
    }
}