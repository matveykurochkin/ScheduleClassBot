using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace ScheduleClassBot.Internal;
internal class Run : IHostedService
{
    private readonly IConfiguration _cfg;

    public Run(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    // ReSharper disable once InconsistentNaming
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var token = _cfg.GetSection("Tokens")["TelegramBotToken"];
            var telegramBot = new TelegramBotClient(token!);

            _logger.Info($"Бот {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.FirstName} успешно запущен!");
            var cts = new CancellationTokenSource();
            cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            telegramBot.StartReceiving(
                new DefaultUpdateHandler(ProcessingMessage.HandleUpdateAsync, ProcessingMessage.HandleErrorAsync),
                receiverOptions,
                cancellationToken
            );
            _logger.Info($"Start Async method for starting bot");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error message: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.Info("Service stopping");
        return Task.CompletedTask;
    }

}
