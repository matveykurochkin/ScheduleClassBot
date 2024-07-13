using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Handlers;

internal class MainHandler(MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler)
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Второстепенный обработчик
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await Task.Run(() => { HandleUpdateAsyncInternal(botClient, update, cancellationToken); }, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        catch (Exception ex)
        {
            Logger.Error("Error in {method}: {error}", nameof(HandleUpdateAsyncInternal), ex);
        }
    }

    /// <summary>
    /// Главный обработчик сообщений, в него входят остальные обработчики сообщений, такие как:
    /// CallbackQueryHandler и MessageHandler
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    private async Task HandleUpdateAsyncInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message
            && update.Type != Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
        {
            Logger.Info("Type {0} not supported!", update.Type);
            return;
        }

        switch (update.Type)
        {
            case Telegram.Bot.Types.Enums.UpdateType.Message:
                await messageHandler.HandleMessage(botClient, update, cancellationToken);
                break;
            case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                await callbackQueryHandler.HandleCallbackQuery(botClient, update, cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Главный обработчик ошибок
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            var me = await botClient.GetMeAsync(cancellationToken: cancellationToken);
            Logger.Error("Error received in telegram bot, name of bot: {firstName}, Error: {error}", me.FirstName,
                exception);
        }
        catch (Exception ex)
        {
            Logger.Error("Error in {method}: {error}", nameof(HandleErrorAsync), ex);
        }
    }
}