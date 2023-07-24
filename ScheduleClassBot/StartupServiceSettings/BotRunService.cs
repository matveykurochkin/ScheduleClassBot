using Microsoft.Extensions.Hosting;
using NLog;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Handlers;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ScheduleClassBot.StartupServiceSettings;

internal class BotRunService : IHostedService
{
    private readonly BotSettingsConfiguration _configuration;
    private readonly MessageHandler _messageHandler;
    private readonly CallbackQueryHandler _callbackQueryHandler;

    public BotRunService(BotSettingsConfiguration configuration, MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler)
    {
        _configuration = configuration;
        _messageHandler = messageHandler;
        _callbackQueryHandler = callbackQueryHandler;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var handler = new MainHandler(_messageHandler, _callbackQueryHandler);
            var token = _configuration.BotToken;
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

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        Logger.Info("Service stopping");
        return Task.CompletedTask;
    }
}