using System.Text;
using NLog;
using Npgsql;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSpecialCommands(BotSettingsConfiguration configuration) : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    internal static long CountMessage;

    /// <summary>
    /// Метод, который безопасно в многопоточной среде считает количество сообщений отправленных боту
    /// </summary>
    public static void IncrementCountMessage()
    {
        Interlocked.Increment(ref CountMessage);
    }

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

    private readonly SpecialInlineButtons _specialInlineButtons = new();

    /// <summary>
    /// Метод который возвращается назад к списку доступных команд с помощью метода SpecialCommandInlineButton
    /// (доступен огранниченному количеству пользователей)
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task BackInSpecialCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                "Держи список специальных функций бота!",
                replyMarkup: _specialInlineButtons.SpecialCommandInlineButton(),
                cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! Back success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error back. {method}: {error}", nameof(BackInSpecialCommands), ex);
        }
    }

    /// <summary>
    /// Метод, который получает список всех, кто пользовался ботом из базы данных 
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="chatId"></param>
    /// <param name="cancellationToken"></param>
    private async Task GetUsersListFromDb(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string selectFromBotUsers = "SELECT * FROM botusers";

            await using var command = new NpgsqlCommand(selectFromBotUsers, connection);
            await using var reader = command.ExecuteReader();

            var listOfBotUsers = string.Empty;
            while (await reader.ReadAsync(cancellationToken))
                listOfBotUsers += $"{reader["name"]} {reader["surname"]} ({reader["username"]}) ID: {reader["id"]}" + Environment.NewLine;

            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message!.MessageId,
                $"Держи список пользователей:\n{listOfBotUsers}",
                replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);
            Logger.Info("!!!SPECIAL COMMAND!!! View users list success from DB!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view users list from DB. {method}: {error}", nameof(GetUsersListFromDb), ex);
        }
    }

    /// <summary>
    /// Метод, позволяющий получить список пользователей, пользовавщихся ботом
    /// является методом по умолчанию, если не подключена база данных
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetUsersList(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery!.Message!.Chat.Id;

            if (configuration.IsWorkWithDb(configuration.DataBase!.ConnectionString))
            {
                await GetUsersListFromDb(botClient, callbackQuery, chatId, cancellationToken);
                return;
            }

            var combinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ListUsers.txt");

            if (System.IO.File.Exists(combinePath))
            {
                var fileContent = await System.IO.File.ReadAllTextAsync(combinePath, cancellationToken);
                var responseBuilder = new StringBuilder();

                foreach (var line in fileContent.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        responseBuilder.AppendLine($"User Info: {line.Trim()}");
                }

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                    $"Держи список пользователей:\n{fileContent}",
                    replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                Logger.Info("!!!SPECIAL COMMAND!!! View users list success!");
            }
            else
            {
                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Пользователей нет!",
                    replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                Logger.Info("!!!SPECIAL COMMAND!!! Error view users list success!");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view users list. {method}: {error}", nameof(GetUsersList), ex);
        }
    }

    /// <summary>
    /// Метод, который получает количество, которые были написаны боту
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="callbackQuery"></param>
    /// <param name="chatId"></param>
    /// <param name="cancellationToken"></param>
    private async Task GetCountMessageFromDb(ITelegramBotClient botClient, CallbackQuery callbackQuery, long chatId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Общее количество сообщений
            const string countMessagesFromDb = "SELECT COUNT(*) FROM messages;";
            await using var commandCountMessages = new NpgsqlCommand(countMessagesFromDb, connection);
            var countMessage = (long)(await commandCountMessages.ExecuteScalarAsync(cancellationToken))!;

            // Общее количество подарков
            const string countPresentsFromDb = "SELECT COUNT(*) FROM presents;";
            await using var commandCountPresents = new NpgsqlCommand(countPresentsFromDb, connection);
            var countPresents = (long)(await commandCountPresents.ExecuteScalarAsync(cancellationToken))!;

            // Количество сообщений за текущий месяц
            const string countMessagesCurrentMonth = @"
                SELECT COUNT(*) 
                FROM messages 
                WHERE DATE_TRUNC('month', messagedate) = DATE_TRUNC('month', CURRENT_DATE);";
            await using var commandCountMessagesCurrentMonth = new NpgsqlCommand(countMessagesCurrentMonth, connection);
            var countMessagesCurrMonth = (long)(await commandCountMessagesCurrentMonth.ExecuteScalarAsync(cancellationToken))!;

            // Количество сообщений за предыдущий месяц
            const string countMessagesPreviousMonth = @"
                SELECT COUNT(*) 
                FROM messages 
                WHERE DATE_TRUNC('month', messagedate) = DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 month';";
            await using var commandCountMessagesPreviousMonth = new NpgsqlCommand(countMessagesPreviousMonth, connection);
            var countMessagesPrevMonth = (long)(await commandCountMessagesPreviousMonth.ExecuteScalarAsync(cancellationToken))!;

            // Количество подарков за текущий месяц
            const string countPresentsCurrentMonth = @"
                SELECT COUNT(*) 
                FROM presents p
                JOIN messages m ON p.id = m.id
                WHERE DATE_TRUNC('month', m.messagedate) = DATE_TRUNC('month', CURRENT_DATE);";
            await using var commandCountPresentsCurrentMonth = new NpgsqlCommand(countPresentsCurrentMonth, connection);
            var countPresentsCurrMonth = (long)(await commandCountPresentsCurrentMonth.ExecuteScalarAsync(cancellationToken))!;

            // Количество подарков за предыдущий месяц
            const string countPresentsPreviousMonth = @"
                SELECT COUNT(*) 
                FROM presents p
                JOIN messages m ON p.id = m.id
                WHERE DATE_TRUNC('month', m.messagedate) = DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 month';";
            await using var commandCountPresentsPreviousMonth = new NpgsqlCommand(countPresentsPreviousMonth, connection);
            var countPresentsPrevMonth = (long)(await commandCountPresentsPreviousMonth.ExecuteScalarAsync(cancellationToken))!;

            // Формирование сообщения
            var responseMessage = 
                $"Количество написанных сообщений боту: {countMessage}!" +
                $"\nКоличество отправленных подарков: {countPresents}!" +
                $"\n\nЗа текущий месяц:" +
                $"\n- Сообщений: {countMessagesCurrMonth}" +
                $"\n- Подарков: {countPresentsCurrMonth}" +
                $"\n\nЗа предыдущий месяц:" +
                $"\n- Сообщений: {countMessagesPrevMonth}" +
                $"\n- Подарков: {countPresentsPrevMonth}";

            // Отправка сообщения
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message!.MessageId, responseMessage, replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! View count message from DB success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view count message from DB. {method}: {error}", nameof(GetCountMessageFromDb), ex);
        }
    }

    public async Task GetLastUser(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery!.Message!.Chat.Id;
            var username = callbackQuery.From.FirstName;

            await using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string selectLastUserFromDb = "SELECT userid FROM messages ORDER BY id DESC LIMIT 1;";

            await using var commandSelectLastUser = new NpgsqlCommand(selectLastUserFromDb, connection);
            await using var reader = await commandSelectLastUser.ExecuteReaderAsync(cancellationToken);

            if (!reader.HasRows || !await reader.ReadAsync(cancellationToken))
            {
                await botClient.SendTextMessageAsync(chatId, "Нет данных о последнем пользователе.", cancellationToken: cancellationToken);
                return;
            }

            var userId = reader.GetInt64(reader.GetOrdinal("userid"));

            await using var connectionUserById = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            await connectionUserById.OpenAsync(cancellationToken);

            var selectUserById = $"SELECT * FROM botusers WHERE id = @userId;";
            await using var commandSelectUserById = new NpgsqlCommand(selectUserById, connectionUserById);
            commandSelectUserById.Parameters.AddWithValue("userId", userId);

            await using var readerSelectUserById = await commandSelectUserById.ExecuteReaderAsync(cancellationToken);

            if (!readerSelectUserById.HasRows || !await readerSelectUserById.ReadAsync(cancellationToken))
            {
                await botClient.SendTextMessageAsync(chatId, "Нет данных о пользователе с данным ID.", cancellationToken: cancellationToken);
                return;
            }

            var userInfo = $"{username}, последний пользователь, который написал боту!\n\n" +
                           $"Id: {readerSelectUserById["id"]}\n" +
                           $"Name: {readerSelectUserById["name"]}\n" +
                           $"Surname: {readerSelectUserById["surname"]}\n" +
                           $"Username: {readerSelectUserById["username"]}\n";

            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message!.MessageId, userInfo,
                replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! Get last user success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error getting last user. {method}: {error}", nameof(GetLastUser), ex);
        }
    }

  
    /// <summary>
    /// Метод, позволяющий получить количество отправленных ботом подарков,
    /// количество написанных сообщений, и юзернейм последнего написавшего человека боту
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetCountMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            if (configuration.IsWorkWithDb(configuration.DataBase!.ConnectionString))
            {
                await GetCountMessageFromDb(botClient, callbackQuery, chatId, cancellationToken);
                return;
            }

            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                $"Количество написанных сообщений боту: {CountMessage}!" +
                $"\nКоличество отправленных подарков: {CountMessage / BotConstants.PresentPercent}!",
                replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    /// <summary>
    /// Метод, позволяющий получить файлы ведения журнала ботом
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var dateTime = DateTime.Now;
            string? logDate = default;
            var month = dateTime.Month <= 9 ? $"0{dateTime.Month}" : $"{dateTime.Month}";
            var day = dateTime.Day <= 9 ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            if (!CheckingMessageText(message.Text!, "specialcommandforgetlogfile"))
            {
                var index = message.Text!.IndexOf(":", StringComparison.Ordinal);
                if (index != -1)
                    logDate = message.Text![(index + 1)..].Trim();

                var pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(pathOnProject,
                    $"log{Path.DirectorySeparatorChar}{logDate}");

                if (System.IO.File.Exists(path))
                {
                    await using var fileStream = new FileStream(path, FileMode.Open);
                    var inputFile = new InputFileStream(fileStream, logDate);
                    await botClient.SendDocumentAsync(message.Chat, inputFile,
                        caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!",
                        cancellationToken: cancellationToken);
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!\n" +
                        $"\nПример команды на сегодня:\n" +
                        $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```",
                        parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:" +
                    $"\n```\nspecialcommandforgetlogfile:yyyy-MM-dd.log\n```" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```\n", parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! Get log file success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error get log file. {method}: {error}", nameof(GetLogFile), ex);
        }
    }

    /// <summary>
    /// Метод, позволяющий получить меню со списком специальных команд бота
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat,
                "Держи список специальных функций бота!",
                replyMarkup: _specialInlineButtons.SpecialCommandInlineButton(), cancellationToken: cancellationToken);
            Logger.Info("!!!SPECIAL COMMAND!!! Get button with all special commands success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error button with all special commands. {method}: {error}",
                nameof(GetButtonWithSpecialCommands), ex);
        }
    }
}