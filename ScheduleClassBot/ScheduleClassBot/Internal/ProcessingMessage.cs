using NLog;
using ScheduleClassBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Internal;
internal class ProcessingMessage
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    public static void UserList(string name, string surname, string username, long? id)
    {
        try
        {
            string path = Path.Combine(projectPath, "ListUsers.txt");
            string fileContent, userInfo = $"User Info: {name} {surname} (@{username}) ID: {id}\n";

            if (!System.IO.File.Exists(path))
            {
                System.IO.File.WriteAllText(path, userInfo);
                _logger.Info("Saved First user!");
            }
            else
            {
                fileContent = System.IO.File.ReadAllText(path);
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

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;

            _logger.Info($"Пользователь {message?.From?.FirstName} {message?.From?.LastName} написал боту данное сообщение: {message?.Text}\n id Пользователя: {message?.From?.Id} Username: @{message?.From?.Username}");

            UserList(message?.From?.FirstName!, message?.From?.LastName!, message?.From?.Username!, message?.From?.Id);
            SpecialCommands.countMessage++;

            if (message?.Text is not null)
            {
                if (SpecialCommands.countMessage % 150 == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, поздравляю! Тебе повезло! Ты выиграл набор крутых стикеров! 🎁\nhttps://t.me/addstickers/BusyaEveryDay", cancellationToken: cancellationToken);
                    _logger.Info($"!!!PRESENT!!! Best Stickers BusyaEveryDay!");
                }
                if (message?.Text == "/start" || message?.Text == "Назад ⬅")
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, смотри мои возможности!\n\n" +
                        $"Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!\n\n" +
                        $"Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас идет!\n\n" +
                        $"Доступные команды:\n" +
                        $"/start - команда для обновления бота\n" +
                        $"/listgroup - команда для просмотра списка групп\n" +
                        $"/todaypmi - команда для просмотра расписания на сегодня группы ПМИ-120\n" +
                        $"/todaypri - команда для просмотра расписания на сегодня группы ПРИ-121", replyMarkup: BotButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
                    return;
                }
                if (message?.Text == "Узнать расписание 📜" || message?.Text == "Список групп 📋" || message?.Text == "/listgroup")
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи список групп!", replyMarkup: BotButtons.ListGroup(), cancellationToken: cancellationToken);
                    return;
                }
                if (message?.Text == "ПМИ-120" || message?.Text == "ПРИ-121")
                {
                    await GetSchedule.GetButtonForGroup(botClient, message, update, message?.Text!);
                    return;
                }
                if (GetSchedule.dayOfWeekPMI.Contains(message!.Text) || message?.Text == "Расписание на сегодня ПМИ-120" || message?.Text == "/todaypmi")
                {
                    await GetSchedule.GetScheduleForGroupPMI(botClient, message, message!.Text);
                    return;
                }
                if (GetSchedule.dayOfWeekPRI.Contains(message!.Text) || message?.Text == "Расписание на сегодня ПРИ-121" || message?.Text == "/todaypri")
                {
                    await GetSchedule.GetScheduleForGroupPRI(botClient, message, message!.Text);
                    return;
                }
                if (message?.Text == "specialcommandforviewlistusers")
                {
                    await SpecialCommands.GetUsersList(botClient, update, message, cancellationToken);
                    return;
                }
                if (message?.Text == "specialcommandforviewcountmessages")
                {
                    await SpecialCommands.GetCountMessage(botClient, update, message, cancellationToken);
                    return;
                }
                if (message!.Text.Contains("specialcommandforgetlogfile"))
                {
                    await SpecialCommands.GetLogFile(botClient, update, message, cancellationToken);
                    return;
                }
                await botClient.SendTextMessageAsync(message!.Chat, $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!", cancellationToken: cancellationToken);
                return;
            }
            await botClient.SendTextMessageAsync(message!.Chat, $"👍", cancellationToken: cancellationToken);
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.Error(exception, $"Error received in telegram bot, name of bot: {botClient.GetMeAsync(cancellationToken: cancellationToken).Result.FirstName}");
        return Task.CompletedTask;
    }
}
