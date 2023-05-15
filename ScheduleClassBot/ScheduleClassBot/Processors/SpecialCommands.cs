using NLog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.Processors;
internal class SpecialCommands
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    internal static long countMessage = 0;
    private static string pathOnProject = "", path = "", logdate = "";
    public static async Task GetUsersList(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            string fileContent = "", path = Path.Combine(projectPath, "ListUsers.txt");

            if (System.IO.File.Exists(path))
            {
                fileContent = System.IO.File.ReadAllText(path);
                StringBuilder responseBuilder = new StringBuilder();

                foreach (string line in fileContent.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        responseBuilder.AppendLine($"User Info: {line.Trim()}");
                }

                await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи список пользователей:\n{fileContent}", cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! View users list success!");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, пользователей нет!", cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! Error view users list success!");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"!!!SPECIAL COMMAND!!! Error view users list. Error message: {ex.Message}");
        }
    }

    public static async Task GetCountMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, количество написанных сообщений боту: {countMessage}!" +
                $"\nКоличество отправленных подарков: {countMessage / 150}!", cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            _logger.Error($"!!!SPECIAL COMMAND!!! Error view count message. Error message: {ex.Message}");
        }
    }

    public static async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            if (!(message.Text! == "specialcommandforgetlogfile"))
            {
                int index = message.Text!.IndexOf(":");
                if (index != -1)
                    logdate = message.Text!.Substring(index + 1).Trim();

                pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(pathOnProject, $"log/{logdate}");

                if (System.IO.File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        InputFileStream inputFile = new InputFileStream(fileStream, logdate);
                        await botClient.SendDocumentAsync(message.Chat, inputFile, caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!");
                    }
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!", cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:\n```\nspecialcommandforgetlogfile:yyyy-mm-dd.log\n```", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error($"!!!SPECIAL COMMAND!!! Error get log file. Error message: {ex.Message}");
        }
    }
}