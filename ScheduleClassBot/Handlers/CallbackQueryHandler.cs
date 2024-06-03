using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;

namespace ScheduleClassBot.Handlers;

internal class CallbackQueryHandler : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly BotSettingsConfiguration _configuration;
    private readonly GettingSpecialCommands _gettingSpecialCommands;

    public CallbackQueryHandler(BotSettingsConfiguration configuration, GettingSpecialCommands gettingSpecialCommands)
    {
        _configuration = configuration;
        _gettingSpecialCommands = gettingSpecialCommands;
    }

    private static ulong CountLike { get; set; }

    /// <summary>
    /// Метод, сравнивающий полученный текст с необходимым, без учета регистра
    /// </summary>
    /// <param name="receivedText">полученный на вход текст</param>
    /// <param name="necessaryText">необходимый текст</param>
    /// <returns>true, если полученный на вход текст = необходимому тексу, false - во всех остальных случаях</returns>
    public bool CheckingMessageText(string receivedText, string necessaryText)
    {
        return string.Equals(receivedText, necessaryText, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Метод, который обрабатывает событие CallbackQuery
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleCallbackQuery(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;

        if (update.CallbackQuery?.Data is not null)
        {
            if (_configuration.UserId?.IdUser!.Contains(chatId) == true)
            {
                if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.SpecialCommandForViewListUsers))
                {
                    await _gettingSpecialCommands.GetUsersList(botClient, update, cancellationToken);
                    return;
                }

                if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.SpecialCommandForViewCountMessages))
                {
                    await _gettingSpecialCommands.GetCountMessage(botClient, update, cancellationToken);
                    return;
                }

                if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.CommandBack))
                {
                    await _gettingSpecialCommands.BackInSpecialCommands(botClient, update, cancellationToken);
                    return;
                }

                Logger.Info($"Press Inline button! CallbackQuery: {update.CallbackQuery?.Data}");
            }

            await botClient.EditMessageTextAsync(chatId.ToString(), update.CallbackQuery!.Message!.MessageId, 
                "Извините, но возможность использования специальных команд отозвана. Обратитесь к администратору, если у вас есть вопросы.", cancellationToken: cancellationToken);

            Logger.Info("The ability to use special commands has been revoked for your profile, profile ID : {0}", chatId);
        }
    }
}