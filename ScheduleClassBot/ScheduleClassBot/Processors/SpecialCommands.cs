using NLog;
using ScheduleClassBot.Internal;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.Processors;
internal class SpecialCommands
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    private static DateTime dateTime;
    internal static ulong countMessage { get; set; }
    internal static string? pathOnProject { get; set; }
    internal static string? path { get; set; }
    internal static string? logdate { get; set; }

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
            _logger.Error("!!!SPECIAL COMMAND!!! Error view users list. {method}: {error}", nameof(GetUsersList), ex);
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
            _logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    public static async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            dateTime = DateTime.Now;
            string month = dateTime.Month <= 9 ? $"0{dateTime.Month}" : $"{dateTime.Month}";
            string day = dateTime.Day <= 9 ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            if (!(message.Text! == "specialcommandforgetlogfile"))
            {
                int index = message.Text!.IndexOf(":");
                if (index != -1)
                    logdate = message.Text!.Substring(index + 1).Trim();

                pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(pathOnProject, $"log/{logdate}");

                if (System.IO.File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        InputFileStream inputFile = new InputFileStream(fileStream, logdate);
                        await botClient.SendDocumentAsync(message.Chat, inputFile, caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!");
                    }
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!\n" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:" +
                    $"\n```\nspecialcommandforgetlogfile:yyyy-mm-dd.log\n```" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```\n" +
                    $"Нажми, чтобы скопировать :)", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get log file success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error get log file. {method}: {error}", nameof(GetLogFile), ex);
        }
    }

    public static async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи список специальных функций бота!", replyMarkup: SpecialBotButton.SpecialCommandButton(), cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get button with all special commands success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error button with all special commands. {method}: {error}", nameof(GetButtonWithSpecialCommands), ex);
        }
    }

    public static async Task GetInfoYourProfile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи информацию о аккаунте!\n\n" +
                $"Идентификатор пользователя: {message?.From?.Id}\n" +
                $"Имя пользователя: @{message?.From?.Username}\n" +
                $"Имя: {message?.From?.FirstName}\n" +
                $"Фамилия: {message?.From?.LastName}\n" +
                $"Язык: {message?.From?.LanguageCode}\n" +
                $"Информация о местоположении: {message?.Location}\n" +
                $"Контактные данные: {message?.Contact}\n" +
                $"Наличие Telegram премиум: {message?.From?.IsPremium}\n" +
                $"Бот: {message?.From?.IsBot}", cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get your info profile success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error your info profile. {method}: {error}", nameof(GetInfoYourProfile), ex);
        }
    }
}