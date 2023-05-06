using NLog;
using ScheduleClassBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Internal;
internal class ProcessingMessage
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;

    public static void UserList(string name, string surname, long? id)
    {
        try
        {
            string path = Path.Combine(projectPath, "ListUsers.txt");
            string fileContent, userInfo = $"User Info: {name} {surname} ID: {id}\n";

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

            _logger.Info($"Пользователь {message?.From?.FirstName} {message?.From?.LastName} написал боту данное сообщение: {message?.Text}\n id Пользователя: {message?.From?.Id}");

            UserList(message?.From?.FirstName!, message?.From?.LastName!, message?.From?.Id);

            if (message?.Text is not null)
            {
                if (message?.Text == "/start" || message?.Text == "Назад ⬅")
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, смотри мои возможности!\n\n" +
                        $"Я могу показать расписание занятий таких групп: ПМИ-120 и ПРИ-121!\n\n" +
                        $"Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас идет!\n\n" +
                        $"Доступные команды:\n" +
                        $"/start - команда для обновления бота\n" +
                        $"/listgroup - команда для просмотра списка групп", replyMarkup: BotButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
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
                if (GetSchedule.dayOfWeekPMI.Contains(message!.Text))
                {
                    await GetSchedule.GetScheduleForGroupPMI(botClient, message, message!.Text);
                    return;
                }
                if (GetSchedule.dayOfWeekPRI.Contains(message!.Text))
                {
                    await GetSchedule.GetScheduleForGroupPRI(botClient, message, message!.Text);
                    return;
                }

                await botClient.SendTextMessageAsync(message!.Chat, $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!", cancellationToken: cancellationToken);
            }
        }
    }

    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.Error(exception, $"Error received in telegram bot, name of bot: {botClient.GetMeAsync(cancellationToken: cancellationToken).Result.FirstName}");
        return Task.CompletedTask;
    }
}
