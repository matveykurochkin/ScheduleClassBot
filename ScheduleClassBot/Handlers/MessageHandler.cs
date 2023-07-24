using NLog;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Handlers;

internal class MessageHandler : ICheckMessage
{
    private readonly BotSettingsConfiguration _configuration;
    private readonly GettingSpecialCommands _gettingSpecialCommands;

    public MessageHandler(BotSettingsConfiguration configuration, GettingSpecialCommands gettingSpecialCommands)
    {
        _configuration = configuration;
        _gettingSpecialCommands = gettingSpecialCommands;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly GettingSessionSchedule _gettingSession = new();
    private readonly GettingSchedule _gettingSchedule = new();
    private readonly InlineButtons _inlineButtons = new();
    private readonly ReplyButtons _replyButtons = new();

    private static void SaveNewUser(Message message)
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ListUsers.txt");
            var userInfo =
                $"User Info: {message.From?.FirstName!} {message.From?.LastName!} (@{message.From?.Username!}) ID: {message.From?.Id}\n";

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, userInfo);
                Logger.Info($"Saved First user!" +
                            $"\t\nMessage Id: {message.MessageId}" +
                            $"\t\nMore information on {path}");
            }
            else
            {
                var fileContent = System.IO.File.ReadAllText(path);
                if (!fileContent.Contains(userInfo))
                {
                    System.IO.File.AppendAllText(path, userInfo);
                    Logger.Info("Saved new user!" +
                                $"\t\nMessage Id: {message.MessageId}" +
                                $"\t\nMore information on {path}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saved user. Error message: {ex.Message}");
        }
    }

    private bool CheckingUserId(long? userId)
    {
        var idUser = _configuration.UserId!.IdUser!.ToArray();
        return idUser.Any(x => x == userId);
    }

    public bool CheckingMessageText(string receivedText, string necessaryText)
    {
        return string.Equals(receivedText, necessaryText, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Метод, который обрабатывает сообщения
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task HandleMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;

        Logger.Info(
            $"Пользователь || {message?.From?.FirstName} {message?.From?.LastName} || написал сообщение боту!" +
            $"\n\tТекст сообщения: {message?.Text}" +
            $"\n\tID Пользователя: {message?.From?.Id}" +
            $"\n\tUsername: @{message?.From?.Username}");

        if (message?.Text is null)
        {
            await botClient.SendTextMessageAsync(message!.Chat, "👍", cancellationToken: cancellationToken);
            return;
        }

        if (message.Text!.StartsWith(
                $"@{botClient.GetMeAsync(cancellationToken: cancellationToken).Result.Username}"))
            message.Text = message.Text.Split(' ')[1];

        SaveNewUser(message);
        GettingSpecialCommands.IncrementCountMessage();

        if (message.Text is not null)
        {
            if (GettingSpecialCommands.CountMessage % BotConstants.CountMessageForPresent == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, поздравляю! Тебе повезло! Ты выиграл набор крутых стикеров! 🎁" +
                    $"\nhttps://t.me/addstickers/BusyaEveryDay",
                    cancellationToken: cancellationToken);
                Logger.Info("!!!PRESENT!!! Best Stickers BusyaEveryDay!");
            }

            if (CheckingMessageText(message.Text, BotConstants.CommandStart)
                || CheckingMessageText(message.Text, BotConstants.CommandBack))
            {
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, смотри мои возможности!",
                    replyMarkup: _replyButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
                await botClient.SendTextMessageAsync(message.Chat,
                    $"Я могу показать расписание занятий таких групп: {BotConstants.GroupPmi} и {BotConstants.GroupPri}!\n\n" +
                    $"Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас идет!\n\n" +
                    $"Доступные команды:\n" +
                    $"{BotConstants.CommandStart} - обновление бота\n" +
                    $"{BotConstants.CommandTodayPmi} - расписание на сегодня группы ПМИ-120\n" +
                    $"{BotConstants.CommandTomorrowPmi} - расписание на завтра группы ПМИ-120\n" +
                    $"{BotConstants.CommandSessionPmi} - расписание сессии группы ПМИ-120\n" +
                    $"{BotConstants.CommandTodayPri} - расписание на сегодня группы ПРИ-121\n" +
                    $"{BotConstants.CommandTomorrowPri} - расписание на завтра группы ПРИ-121\n" +
                    $"{BotConstants.CommandSessionPri} - расписание сессии группы ПРИ-121",
                    replyMarkup: _inlineButtons.InlineButtonOnBot(), cancellationToken: cancellationToken);
                return;
            }

            if (CheckingMessageText(message.Text, BotConstants.GroupPmi)
                || CheckingMessageText(message.Text, BotConstants.GroupPri))
            {
                await _gettingSchedule.GetButtonForGroup(botClient, message, update, message.Text!);
                return;
            }

            if (GettingSchedule.DayOfWeekPmi.Contains(message.Text)
                || CheckingMessageText(message.Text, BotConstants.ScheduleForPmiToday)
                || CheckingMessageText(message.Text, BotConstants.CommandTodayPmi)
                || CheckingMessageText(message.Text, BotConstants.ScheduleForPmiTomorrow)
                || CheckingMessageText(message.Text, BotConstants.CommandTomorrowPmi))
            {
                await _gettingSchedule.GetScheduleForGroupPMI(botClient, message, message.Text);
                return;
            }

            if (GettingSchedule.DayOfWeekPri.Contains(message.Text)
                || CheckingMessageText(message.Text, BotConstants.ScheduleForPriToday)
                || CheckingMessageText(message.Text, BotConstants.CommandTodayPri)
                || CheckingMessageText(message.Text, BotConstants.ScheduleForPriTomorrow)
                || CheckingMessageText(message.Text, BotConstants.CommandTomorrowPri))
            {
                await _gettingSchedule.GetScheduleForGroupPRI(botClient, message, message.Text);
                return;
            }

            if (CheckingMessageText(message.Text, BotConstants.ScheduleSessionForPmi)
                || CheckingMessageText(message.Text, BotConstants.CommandSessionPmi))
            {
                await _gettingSession.GetSessionOnPMI(botClient, message, cancellationToken);
                return;
            }

            if (CheckingMessageText(message.Text, BotConstants.ScheduleSessionForPri)
                || CheckingMessageText(message.Text, BotConstants.CommandSessionPri))
            {
                await _gettingSession.GetSessionOnPRI(botClient, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith(BotConstants.SpecialCommandForViewAllSpecialCommand)
                && CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetButtonWithSpecialCommands(botClient, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith(BotConstants.SpecialCommandForGetLogFile)
                && CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetLogFile(botClient, update, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith(BotConstants.SpecialCommandForCheckYourProfile)
                && CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetInfoYourProfile(botClient, update, message, cancellationToken);
                return;
            }

            if (CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetAnswersFromChatGpt(botClient, update, message, cancellationToken);
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!" +
                $"\nВозможно ты используешь старую команду, попробуй обновить бота, нажав сюда: /start!",
                cancellationToken: cancellationToken);
        }
    }
}