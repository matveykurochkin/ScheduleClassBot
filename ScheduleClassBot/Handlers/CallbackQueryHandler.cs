using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;

namespace ScheduleClassBot.Handlers;

internal class CallbackQueryHandler : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly GettingSpecialCommands _gettingSpecialCommands;

    public CallbackQueryHandler(GettingSpecialCommands gettingSpecialCommands)
    {
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
            if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.Like))
            {
                var inlineButton = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++CountLike})", BotConstants.Like),
                        InlineKeyboardButton.WithCallbackData(text: "👎🏻", BotConstants.DisLike)
                    }
                });
                await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, inlineButton,
                    cancellationToken: cancellationToken);
                return;
            }

            if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.DisLike))
            {
                var inlineButton = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++CountLike})", BotConstants.Like),
                        InlineKeyboardButton.WithCallbackData(text: "👎🏻", BotConstants.DisLike)
                    }
                });
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                    "Я знал, что ты можешь ошибиться при нажатии на кнопку лайка, поэтому я сразу же исправил эту ошибку! 😊",
                    showAlert: true, cancellationToken: cancellationToken);
                await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, inlineButton,
                    cancellationToken: cancellationToken);
                return;
            }

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

            if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.SpecialCommandForGetLogFile))
            {
                await botClient.SendTextMessageAsync(chatId, BotConstants.SpecialCommandForGetLogFile,
                    cancellationToken: cancellationToken);
                return;
            }

            if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.SpecialCommandForCheckYourProfile))
            {
                await botClient.SendTextMessageAsync(chatId, BotConstants.SpecialCommandForCheckYourProfile,
                    cancellationToken: cancellationToken);
                return;
            }

            if (CheckingMessageText(update.CallbackQuery?.Data!, BotConstants.CommandBack))
            {
                await _gettingSpecialCommands.BackInSpecialCommands(botClient, update, cancellationToken);
                return;
            }

            Logger.Info($"Press Inline button! CallbackQuery: {update.CallbackQuery?.Data}");
        }
    }
}