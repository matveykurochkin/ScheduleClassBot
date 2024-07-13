using NLog;
using Npgsql;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using ScheduleClassBot.ProcessingMethods;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.Handlers;

internal class MessageHandler(BotSettingsConfiguration configuration, GettingSpecialCommands gettingSpecialCommands) : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly GettingSessionSchedule _gettingSession = new();
    private readonly GettingSchedule _gettingSchedule = new();
    private readonly ReplyButtons _replyButtons = new();

    /// <summary>
    /// Метод, сохраняющий id пользователя, его сообщение, id сообщения и текущую дату в базу данных
    /// </summary>
    /// <param name="message"></param>
    private void SaveMessageForDb(Message message)
    {
        try
        {
            const string insertMessage = "INSERT INTO messages (USERID, TEXT, ID, MESSAGEDATE)" +
                                         "VALUES (@userid, @text, @id, @messagedate);";

            using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            using var command = new NpgsqlCommand(insertMessage, connection);

            command.Parameters.AddWithValue("@userid", message.From?.Id!);
            command.Parameters.AddWithValue("@text", message.Text!);
            command.Parameters.AddWithValue("@id", message.MessageId);
            command.Parameters.AddWithValue("@messagedate", DateTime.Now); // Добавляем текущую дату и время

            connection.Open();
            var rowsAffected = command.ExecuteNonQuery();
            Logger.Info(rowsAffected > 0
                ? "Information about the message has been successfully saved to the database"
                : "Information about the message is not saved to the database");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error Save Message For Db. Error message: {ex.Message}");
        }
    }

    /// <summary>
    /// Метод, сохраняющий данные пользователя, в базу данных, если они там уже есть, то они обновляются
    /// </summary>
    /// <param name="message"></param>
    private void SaveUserForDb(Message message)
    {
        try
        {
            const string insertNewPeople = "INSERT INTO botusers (id, name, surname, username) " +
                                           "VALUES (@id, @name, @surname, @username) " +
                                           "ON CONFLICT (id) DO UPDATE " +
                                           "SET name = EXCLUDED.name, surname = EXCLUDED.surname, username = EXCLUDED.username;";

            using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            using var command = new NpgsqlCommand(insertNewPeople, connection);

            command.Parameters.AddWithValue("@id", message.From?.Id!);
            command.Parameters.AddWithValue("@name", message.From?.FirstName! == null ? "" : message.From?.FirstName!);
            command.Parameters.AddWithValue("@surname", message.From?.LastName! == null ? "" : message.From?.LastName!);
            command.Parameters.AddWithValue("@username", $"@{message.From?.Username!}");

            connection.Open();
            var rowsAffected = command.ExecuteNonQuery();
            Logger.Info(rowsAffected > 0
                ? "A person has been successfully added to the database or his data has been updated"
                : "An error occurred when adding a person to the database");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saved user for db. Error message: {ex.Message}");
        }
    }

    /// <summary>
    /// Метод, сохраняющий информацию о полученных подарках в базе данных.
    /// Для сохранения используются данные из сообщения.
    /// </summary>
    /// <param name="message">Сообщение пользователя</param>
    private async Task RecordPresentInDb(Message message)
    {
        try
        {
            const string insertPresent = "INSERT INTO presents (userid, id, text) " +
                                         "VALUES (@userid, @id, @text);";

            await using var connection = new NpgsqlConnection(configuration.DataBase!.ConnectionString);
            await using var command = new NpgsqlCommand(insertPresent, connection);

            command.Parameters.AddWithValue("@userid", message.From?.Id!);
            command.Parameters.AddWithValue("@id", message.MessageId);
            command.Parameters.AddWithValue("@text", message.Text!);

            await connection.OpenAsync();
            var rowsAffected = await command.ExecuteNonQueryAsync();
            Logger.Info(rowsAffected > 0
                ? "Информация о полученном подарке успешно добавлена в базу данных"
                : "Произошла ошибка при добавлении информации о подарке в базу данных");
        }
        catch (Exception ex)
        {
            Logger.Error($"Ошибка при сохранении информации о подарке в базу данных. Сообщение об ошибке: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Метод, сохраняющий новых пользователей, на вход получает сообщение, так как из сообщения можно получить
    /// необходимую информацию о том, кто его отправил, если пользователь новый происходит сохранение, если старый - ничего не происходит
    /// </summary>
    /// <param name="message">сообщение текущего пользователя</param>
    private void SaveNewUser(Message message)
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
                if (fileContent.Contains(userInfo)) return;
                System.IO.File.AppendAllText(path, userInfo);
                Logger.Info("Saved new user!" +
                            $"\t\nMessage Id: {message.MessageId}" +
                            $"\t\nMore information on {path}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error saved user. Error message: {ex.Message}");
        }
    }

    /// <summary>
    /// Метод, проверяющий есть ли в файле конфигурации id пользователя который отправл сообщение,
    /// если есть то бот открывает доступ пользователю к специальным командам, если нет отвечает, что не знает как ответить на сообщение,
    /// если пользователь воодит специальную команду
    /// </summary>
    /// <param name="userId">id пользователя</param>
    /// <returns>true, если id текущего пользователя есть в файле конфигурации, false - во всех остальных случаях</returns>
    private bool CheckingUserId(long? userId)
    {
        var idUser = configuration.UserId!.IdUser!.ToArray();
        return idUser.Any(x => x == userId);
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

    /// <summary>
    /// Метод, который отправляет ссылку на стикеры, каждые n раз
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    private async Task PresentStickers(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                InlineKeyboardButton.WithUrl("Нажми сюда, чтобы забрать!", "https://t.me/addstickers/BusyaEveryDay")
            }
        });

        await botClient.SendTextMessageAsync(
            update.Message!.Chat,
            $"{update.Message?.From?.FirstName}, поздравляю! Тебе повезло! Ты выиграл набор стикеров! 🎁",
            cancellationToken: cancellationToken,
            replyMarkup: inlineKeyboard
        );

        Logger.Info("!!!PRESENT!!! Best Stickers BusyaEveryDay!");
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
            $"Пользователь || {message!.From?.FirstName} {message.From?.LastName} || написал сообщение боту!" +
            $"\n\tТекст сообщения: {message.Text}" +
            $"\n\tID Пользователя: {message.From?.Id}" +
            $"\n\tUsername: @{message.From?.Username}");

        if (configuration.IsWorkWithDb(configuration.DataBase!.ConnectionString))
        {
             SaveUserForDb(message);
             SaveMessageForDb(message);
            
            // Шанс получения подарка
            var chance = BotConstants.PresentPercent / 100;
            
            // Генерация случайного числа от 0 до 1
            var random = new Random().NextDouble();
            
            // NextDouble() генерирует случайное число типа double в диапазоне от 0.0 (включительно) до 1.0 (исключительно) 
            // с использованием равномерного распределения. Это означает, что каждое возможное значение 
            // в этом диапазоне имеет одинаковую вероятность быть выбранным.
            // Внутри NextDouble() используется базовый генератор псевдослучайных чисел (Random Number Generator), 
            // который обычно называется System.Random. Этот генератор использует некоторый начальный "зерно" (seed), 
            // обычно основанное на текущем времени, чтобы генерировать последовательность псевдослучайных чисел. 
            // Каждый раз, когда вызывается NextDouble(), он использует этот генератор, чтобы создать следующее случайное число.
            // Генераторы псевдослучайных чисел создают "псевдослучайные" числа, потому что их выход не является истинно случайным, 
            // а зависит от начального "зерна". Однако, при правильном использовании и достаточно большом количестве генерируемых чисел,
            // эти генераторы обычно обеспечивают достаточно хороший уровень случайности для большинства приложений.

            // Если случайное число меньше или равно шансу, выдаем подарок
            if (random <= chance)
            {
                await PresentStickers(botClient, update, cancellationToken);
                await RecordPresentInDb(message);
            }
        }

        if (message.Text is null)
        {
            await botClient.SendTextMessageAsync(message.Chat, "👍", cancellationToken: cancellationToken);
            return;
        }

        if (message.Text!.StartsWith(
                $"@{botClient.GetMeAsync(cancellationToken: cancellationToken).Result.Username}"))
            message.Text = message.Text.Split(' ')[1];

        var botUsername = $"@{botClient.GetMeAsync(cancellationToken: cancellationToken).Result.Username}";
        if (message.Text!.Contains(botUsername))
            message.Text = message.Text.Replace(botUsername, "").Trim();
        
        if (!configuration.IsWorkWithDb(configuration.DataBase!.ConnectionString))
        {
            SaveNewUser(message);
            GettingSpecialCommands.IncrementCountMessage();
            if (GettingSpecialCommands.CountMessage > 0 && GettingSpecialCommands.CountMessage % BotConstants.PresentPercentWithoutDb == 0)
                await PresentStickers(botClient, update, cancellationToken);
        }

        if (message.Text is not null)
        {
            if (CheckingMessageText(message.Text, BotConstants.CommandStart)
                || CheckingMessageText(message.Text, BotConstants.CommandBack))
            {
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, смотри мои возможности!\n\nЯ могу показать расписание занятий группы {BotConstants.GroupPri}!\n\n" +
                    $"Для просмотра расписания необходимо выбрать группу и день недели, также я расскажу числитель или знаменатель сейчас идет!\n\n" +
                    $"Доступные команды:\n" +
                    $"{BotConstants.CommandStart} - обновление бота\n" +
                    $"{BotConstants.CommandTodayPri} - расписание на сегодня группы ПРИ-121\n" +
                    $"{BotConstants.CommandTomorrowPri} - расписание на завтра группы ПРИ-121\n" +
                    $"{BotConstants.CommandSessionPri} - расписание сессии группы ПРИ-121",
                    replyMarkup: _replyButtons.MainButtonOnBot(), cancellationToken: cancellationToken);
                return;
            }

            if (CheckingMessageText(message.Text, BotConstants.GroupPri))
            {
                await _gettingSchedule.GetButtonForGroup(botClient, message, update, message.Text!);
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

            if (CheckingMessageText(message.Text, BotConstants.ScheduleSessionForPri)
                || CheckingMessageText(message.Text, BotConstants.CommandSessionPri))
            {
                await _gettingSession.GetSessionOnPRI(botClient, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith(BotConstants.SpecialCommandForViewAllSpecialCommand)
                && CheckingUserId(message.From?.Id))
            {
                await gettingSpecialCommands.GetButtonWithSpecialCommands(botClient, message, cancellationToken);
                return;
            }

            if (message.Text.StartsWith(BotConstants.SpecialCommandForGetLogFile)
                && CheckingUserId(message.From?.Id))
            {
                await gettingSpecialCommands.GetLogFile(botClient, update, message, cancellationToken);
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!" +
                $"\nВозможно ты используешь старую команду, попробуй обновить бота, нажав сюда: /start!",
                cancellationToken: cancellationToken);
        }
    }
}