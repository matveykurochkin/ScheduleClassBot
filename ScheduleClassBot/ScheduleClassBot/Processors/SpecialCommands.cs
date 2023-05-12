using NLog;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Processors;
internal class SpecialCommands
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    internal static long countMessage = 0;
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
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, количество написанных сообщений боту: {countMessage}!", cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            _logger.Error($"!!!SPECIAL COMMAND!!! Error view count message. Error message: {ex.Message}");
        }
    }
}
