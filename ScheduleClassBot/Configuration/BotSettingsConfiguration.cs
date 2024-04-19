using System.Data;
using NLog;
using Npgsql;

namespace ScheduleClassBot.Configuration;

/// <summary>
/// Класс, объединяющий воедино все нижеперечисленные классы, инициализируется при запуске приложения
/// после инициализации в классе хранятся:
/// токен подключения к телеграм боту,
/// список людей, кому доступны специальные команды,
/// строка для подклчения к бд, если пропущена или введена не кореектно, бот работает по умолчанию с файлами
/// (обязательным параметром является только токен подключения к боту)
/// (получает данные с файла appsettings.json и присваивает значение полей одноименным полям в текущем классе)
/// </summary>
public class BotSettingsConfiguration
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public BotTokenSection? BotToken { get; set; }
    public UserIdSection? UserId { get; set; }
    public DataBaseConfiguration? DataBase { get; init; }

    /// <summary>
    /// Метод, для проверки строки подключения не пустая ли они и правильного ли формата
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public bool IsWorkWithDb(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return false; // Если строка подключения пуста, не выводим ошибку, а сразу возвращаем false

        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return connection.State == ConnectionState.Open;
        }
        catch (Exception ex)
        {
            Logger.Error($"The error is in the method that checks the connection to the database, " +
                         $"it occurs if you tried to connect the bot to the database, but this did not happen, " +
                         $"the bot works in the default mode (working with files). Error message: {ex.Message}");
        }

        return false;
    }
}

/// <summary>
/// Класс, который содержит свойство TelegramBotToken, которое содержит значение токена бота
/// </summary>
public class BotTokenSection
{
    public string? TelegramBotToken { get; set; }
}

/// <summary>
/// Класс, который содержит свойство IdUser, которое содержит список объектов типа long, в нем хранятся
/// id пользователей, которые имеют доступ к специальным командам
/// </summary>
public class UserIdSection
{
    public List<long>? IdUser { get; set; }
}

/// <summary>
/// Класс, который содержит свойство ConnectionString, которое необходимо для подключения и работы с базой данных
/// </summary>
public class DataBaseConfiguration
{
    public string? ConnectionString { get; init; }
}