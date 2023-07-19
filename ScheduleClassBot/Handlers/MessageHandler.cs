using Microsoft.Extensions.Configuration;
using NLog;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Handlers;

internal class MessageHandler
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly string ProjectPath = AppDomain.CurrentDomain.BaseDirectory;
    private readonly long[]? _idUser = Configuration.GetSection("UserID:IdUser").Get<long[]>();

    private readonly GettingSessionSchedule _gettingSession = new();
    private readonly GettingSchedule _gettingSchedule = new();
    private readonly InlineButtons _inlineButtons = new();
    private readonly ReplyButtons _replyButtons = new();
    private readonly GettingSpecialCommands _gettingSpecialCommands = new();

    private void UserList(string name, string surname, string username, long? id)
    {
        try
        {
            var path = Path.Combine(ProjectPath, "ListUsers.txt");
            var userInfo = $"User Info: {name} {surname} (@{username}) ID: {id}\n";

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, userInfo);
                Logger.Info("Saved First user!");
            }
            else
            {
                var fileContent = System.IO.File.ReadAllText(path);
                if (!fileContent.Contains(userInfo))
                {
                    System.IO.File.AppendAllText(path, userInfo);
                    Logger.Info("Saved new user!");
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
        return _idUser != null && _idUser.Any(x => x == userId);
    }

    public async Task HandlerMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        UserList(message.From?.FirstName!, message.From?.LastName!, message.From?.Username!, message.From?.Id);
        GettingSpecialCommands.CountMessage++;

        if (message.Text is not null)
        {
            if (GettingSpecialCommands.CountMessage % 50 == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, поздравляю! Тебе повезло! Ты выиграл набор крутых стикеров! 🎁" +
                    $"\nhttps://t.me/addstickers/BusyaEveryDay",
                    cancellationToken: cancellationToken);
                Logger.Info("!!!PRESENT!!! Best Stickers BusyaEveryDay!");
            }

            if (message.Text == "/start"
                || message.Text == "Назад ⬅")
            {
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, смотри мои возможности!",
                    replyMarkup: _replyButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
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
                    replyMarkup: _inlineButtons.InlineButtonOnBot(), cancellationToken: cancellationToken);
                return;
            }

            if (message.Text == "ПМИ-120"
                || message.Text == "ПРИ-121")
            {
                await _gettingSchedule.GetButtonForGroup(botClient, message, update, message.Text!);
                return;
            }

            if (GettingSchedule.DayOfWeekPmi.Contains(message.Text)
                || message.Text == "Расписание на сегодня ПМИ-120"
                || message.Text == "/todaypmi"
                || message.Text == "Расписание на завтра ПМИ-120"
                || message.Text == "/tomorrowpmi")
            {
                await _gettingSchedule.GetScheduleForGroupPMI(botClient, message, message.Text);
                return;
            }

            if (GettingSchedule.DayOfWeekPri.Contains(message.Text)
                || message.Text == "Расписание на сегодня ПРИ-121"
                || message.Text == "/todaypri"
                || message.Text == "Расписание на завтра ПРИ-121"
                || message.Text == "/tomorrowpri")
            {
                await _gettingSchedule.GetScheduleForGroupPRI(botClient, message, message.Text);
                return;
            }

            if (message.Text == "Расписание сессии ПМИ-120"
                || message.Text == "/sessionpmi")
            {
                await _gettingSession.GetSessionOnPMI(botClient, update, message, cancellationToken);
                return;
            }

            if (message.Text == "Расписание сессии ПРИ-121"
                || message.Text == "/sessionpri")
            {
                await _gettingSession.GetSessionOnPRI(botClient, update, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith("specialcommandforviewbuttonwithlistallspecialcommands")
                && CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetButtonWithSpecialCommands(botClient, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith("specialcommandforgetlogfile")
                && CheckingUserId(message.From?.Id))
            {
                await _gettingSpecialCommands.GetLogFile(botClient, update, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith("specialcommandforcheckyourprofile")
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