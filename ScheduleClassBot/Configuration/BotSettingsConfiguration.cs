namespace ScheduleClassBot.Configuration;

public class BotSettingsConfiguration
{
    public BotTokenSection? BotToken { get; set; }
    public UserIdSection? UserId { get; set; }
    public OpenAiSection? OpenAi { get; set; }
}

public class BotTokenSection
{
    public string? TelegramBotToken { get; set; }
}

public class UserIdSection
{
    public List<long>? IdUser { get; set; }
}

public class OpenAiSection
{
    public string? ChatGptKey { get; set; }
}