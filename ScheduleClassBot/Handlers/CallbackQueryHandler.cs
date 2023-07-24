using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using NLog;

namespace ScheduleClassBot.Handlers;

internal class CallbackQueryHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly GettingSpecialCommands _gettingSpecialCommands;

    public CallbackQueryHandler(GettingSpecialCommands gettingSpecialCommands)
    {
        _gettingSpecialCommands = gettingSpecialCommands;
    }

    private static ulong CountLike { get; set; }


    public async Task HandlerCallbackQuery(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;

        if (update.CallbackQuery?.Data is not null)
        {
            if (update.CallbackQuery?.Data == "like")
            {
                var inlineButton = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++CountLike})", callbackData: "like"),
                        InlineKeyboardButton.WithCallbackData(text: "👎🏻", callbackData: "dislike")
                    }
                });
                await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, inlineButton,
                    cancellationToken: cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "dislike")
            {
                var inlineButton = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++CountLike})", callbackData: "like"),
                        InlineKeyboardButton.WithCallbackData(text: "👎🏻", callbackData: "dislike")
                    }
                });
                await botClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                    "Я знал, что ты можешь ошибиться при нажатии на кнопку лайка, поэтому я сразу же исправил эту ошибку! 😊",
                    showAlert: true, cancellationToken: cancellationToken);
                await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, inlineButton,
                    cancellationToken: cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "specialcommandforviewlistusers")
            {
                await _gettingSpecialCommands.GetUsersList(botClient, update, cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "specialcommandforviewcountmessages")
            {
                await _gettingSpecialCommands.GetCountMessage(botClient, update, cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "specialcommandforgetlogfile")
            {
                await botClient.SendTextMessageAsync(chatId, "specialcommandforgetlogfile",
                    cancellationToken: cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "specialcommandforcheckyourprofile")
            {
                await botClient.SendTextMessageAsync(chatId, "specialcommandforcheckyourprofile",
                    cancellationToken: cancellationToken);
                return;
            }

            if (update.CallbackQuery?.Data == "back")
            {
                await _gettingSpecialCommands.BackInSpecialCommands(botClient, update, cancellationToken);
                return;
            }

            Logger.Info($"Press Inline button! CallbackQuery: {update.CallbackQuery?.Data}");
        }
    }
}