using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Handlers;

internal class MainHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly MessageHandler _messageHandler;
    private readonly CallbackQueryHandler _callbackQueryHandler;

    public MainHandler(MessageHandler messageHandler, CallbackQueryHandler callbackQueryHandler)
    {
        _messageHandler = messageHandler;
        _callbackQueryHandler = callbackQueryHandler;
    }
    
    

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
#pragma warning disable CS4014
            await Task.Run(() => { HandleUpdateAsyncInternal(botClient, update, cancellationToken); }, cancellationToken);
#pragma warning restore CS4014
        }
        catch (Exception ex)
        {
            Logger.Error("Error in {method}: {error}", nameof(HandleUpdateAsyncInternal), ex);
        }
    }

    private async Task HandleUpdateAsyncInternal(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            await _messageHandler.HandlerMessage(botClient, update, cancellationToken);

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            await _callbackQueryHandler.HandlerCallbackQuery(botClient, update, cancellationToken);
    }

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