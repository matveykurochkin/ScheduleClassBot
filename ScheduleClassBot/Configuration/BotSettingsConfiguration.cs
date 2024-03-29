﻿namespace ScheduleClassBot.Configuration;

/// <summary>
/// Класс, объединяющий воедино все нижеперечисленные классы, инициализируется при запуске приложения
/// после инициализации в классе хранятся:
/// токен подключения к телеграм боту,
/// список людей, кому доступны специальные команды,
/// токен для подключения к Chat GPT
/// (обязательным параметром является только токен подключения к боту)
/// (получает данные с файла appsettings.json и присваивает значение полей одноименным полям в текущем классе)
/// </summary>
public class BotSettingsConfiguration
{
    public BotTokenSection? BotToken { get; set; }
    public UserIdSection? UserId { get; set; }
    public OpenAiSection? OpenAi { get; set; }
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
/// Класс, который содержит свойство ChatGptKey, которое необходимо для работы Chat GPT
/// </summary>
public class OpenAiSection
{
    public string? ChatGptKey { get; set; }
}