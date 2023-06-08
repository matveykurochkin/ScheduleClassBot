using NLog;
using ScheduleClassBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Configuration;

namespace ScheduleClassBot.Internal;

internal class ProcessingMessage
{
    private static IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static readonly string _projectPath = AppDomain.CurrentDomain.BaseDirectory;
    private static ulong countLike { get; set; }
    private static readonly long[]? idUser = configuration.GetSection("UserID:IdUser").Get<long[]>();

    private static void UserList(string name, string surname, string username, long? id)
    {
        try
        {
            var path = Path.Combine(_projectPath, "ListUsers.txt");
            var userInfo = $"User Info: {name} {surname} (@{username}) ID: {id}\n";

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, userInfo);
                _logger.Info("Saved First user!");
            }
            else
            {
                var fileContent = System.IO.File.ReadAllText(path);
                if (!fileContent.Contains(userInfo))
                {
                    System.IO.File.AppendAllText(path, userInfo);
                    _logger.Info("Saved new user!");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error saved user. Error message: {ex.Message}");
        }
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            await HandleUpdateAsyncInternal(botClient, update, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error("Error in {method}: {error}", nameof(HandleUpdateAsyncInternal), ex);
        }
    }

    private static async Task HandleUpdateAsyncInternal(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;

            if (message!.Text!.StartsWith(
                    $"@{botClient.GetMeAsync(cancellationToken: cancellationToken).Result.Username}"))
                message.Text = message.Text.Split(' ')[1];
            
            _logger.Info(
                $"Пользователь || {message?.From?.FirstName} {message?.From?.LastName} || написал сообщение боту!\n\tТекст сообщения: {message?.Text}\n\tID Пользователя: {message?.From?.Id}\n\tUsername: @{message?.From?.Username}");

            UserList(message?.From?.FirstName!, message?.From?.LastName!, message?.From?.Username!, message?.From?.Id);
            SpecialCommands.countMessage++;

            if (message?.Text is not null)
            {
                if (SpecialCommands.countMessage % 150 == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"{update.Message?.From?.FirstName}, поздравляю! Тебе повезло! Ты выиграл набор крутых стикеров! 🎁\nhttps://t.me/addstickers/BusyaEveryDay",
                        cancellationToken: cancellationToken);
                    _logger.Info($"!!!PRESENT!!! Best Stickers BusyaEveryDay!");
                }

                if (message?.Text == "/start"
                    || message?.Text == "Назад ⬅")
                {
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"{update.Message?.From?.FirstName}, смотри мои возможности!",
                        replyMarkup: BotButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!\n\n" +
                        $"Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас идет!\n\n" +
                        $"Доступные команды:\n" +
                        $"/start - обновление бота\n" +
                        $"/todaypmi - расписание на сегодня группы ПМИ-120\n" +
                        $"/tomorrowpmi - расписание на завтра группы ПМИ-120\n" +
                        $"/sessionpmi - расписание сессии группы ПМИ-120\n" +
                        $"/todaypri - расписание на сегодня группы ПРИ-121\n" +
                        $"/tomorrowpri - расписание на завтра группы ПРИ-121\n" +
                        $"/sessionpri - расписание сессии группы ПРИ-121",
                        replyMarkup: InlineButtons.InlineButtonOnBot(), cancellationToken: cancellationToken);
                    return;
                }

                if (message?.Text == "ПМИ-120"
                    || message?.Text == "ПРИ-121")
                {
                    await GetSchedule.GetButtonForGroup(botClient, message, update, message?.Text!);
                    return;
                }

                if (GetSchedule.dayOfWeekPMI.Contains(message!.Text)
                    || message?.Text == "Расписание на сегодня ПМИ-120"
                    || message?.Text == "/todaypmi"
                    || message?.Text == "Расписание на завтра ПМИ-120"
                    || message?.Text == "/tomorrowpmi")
                {
                    await GetSchedule.GetScheduleForGroupPMI(botClient, message, message!.Text);
                    return;
                }

                if (GetSchedule.dayOfWeekPRI.Contains(message!.Text)
                    || message?.Text == "Расписание на сегодня ПРИ-121"
                    || message?.Text == "/todaypri"
                    || message?.Text == "Расписание на завтра ПРИ-121"
                    || message?.Text == "/tomorrowpri")
                {
                    await GetSchedule.GetScheduleForGroupPRI(botClient, message, message!.Text);
                    return;
                }

                if (message?.Text == "Расписание сессии ПМИ-120"
                    || message?.Text == "/sessionpmi")
                {
                    await GetSessionSchedule.GetSessionOnPMI(botClient, update, message, cancellationToken);
                    return;
                }

                if (message?.Text == "Расписание сессии ПРИ-121"
                    || message?.Text == "/sessionpri")
                {
                    await GetSessionSchedule.GetSessionOnPRI(botClient, update, message, cancellationToken);
                    return;
                }

                if (message!.Text.StartsWith("specialcommandforviewbuttonwithlistallspecialcommands"))
                {
                    await SpecialCommands.GetButtonWithSpecialCommands(botClient, update, message, cancellationToken);
                    return;
                }

                if (message!.Text.StartsWith("specialcommandforgetlogfile"))
                {
                    await SpecialCommands.GetLogFile(botClient, update, message, cancellationToken);
                    return;
                }

                if (message!.Text.StartsWith("specialcommandforcheckyourprofile"))
                {
                    await SpecialCommands.GetInfoYourProfile(botClient, update, message, cancellationToken);
                    return;
                }

                if (idUser!.Any(x => x == message?.From?.Id))
                {
                    await SpecialCommands.GetQuestionsFromChatGPT(botClient, update, message, cancellationToken);
                    return; 
                }
                
                await botClient.SendTextMessageAsync(message!.Chat,
                    $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!\nВозможно ты используешь старую команду, попробуй обновить бота, нажав сюда: /start!",
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.SendTextMessageAsync(message!.Chat, $"👍", cancellationToken: cancellationToken);
        }

        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
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
                            InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++countLike})", callbackData: "like"),
                            InlineKeyboardButton.WithCallbackData(text: $"👎🏻", callbackData: "dislike")
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
                            InlineKeyboardButton.WithCallbackData(text: $"👍🏻 ({++countLike})", callbackData: "like"),
                            InlineKeyboardButton.WithCallbackData(text: $"👎🏻", callbackData: "dislike")
                        }
                    });
                    await botClient.AnswerCallbackQueryAsync(callbackQuery.Id,
                        "Я знал, что ты можешь ошибиться при нажатии на кнопку лайка, поэтому я сразу же исправил эту ошибку! 😊",
                        showAlert: true);
                    await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, inlineButton,
                        cancellationToken: cancellationToken);
                    return;
                }

                if (update.CallbackQuery?.Data == "specialcommandforviewlistusers")
                {
                    await SpecialCommands.GetUsersList(botClient, update, update.Message!, cancellationToken);
                    return;
                }

                if (update.CallbackQuery?.Data == "specialcommandforviewcountmessages")
                {
                    await SpecialCommands.GetCountMessage(botClient, update, update.Message!, cancellationToken);
                    return;
                }

                if (update.CallbackQuery?.Data == "specialcommandforgetlogfile")
                {
                    await botClient.SendTextMessageAsync(chatId, $"specialcommandforgetlogfile",
                        cancellationToken: cancellationToken);
                    return;
                }

                if (update.CallbackQuery?.Data == "specialcommandforcheckyourprofile")
                {
                    await botClient.SendTextMessageAsync(chatId, $"specialcommandforcheckyourprofile",
                        cancellationToken: cancellationToken);
                    return;
                }

                if (update.CallbackQuery?.Data == "back")
                {
                    await SpecialCommands.Back(botClient, update, cancellationToken);
                    return;
                }

                _logger.Info($"Press Inline button! CallbackQuery: {update.CallbackQuery?.Data}");
            }
        }
    }

    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var me = await botClient.GetMeAsync(cancellationToken: cancellationToken);
            _logger.Error("Error received in telegram bot, name of bot: {firstName}, Error: {error}", me.FirstName,
                exception);
        }
        catch (Exception ex)
        {
            _logger.Error("Error in {method}: {error}", nameof(HandleErrorAsync), ex);
        }
    }
}