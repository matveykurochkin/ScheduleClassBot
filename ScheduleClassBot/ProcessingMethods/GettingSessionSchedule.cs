using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSessionSchedule
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Метод, показывающий расписание сессии группы ПМИ-120
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetSessionOnPMI(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"📌Расписание сессии группы ПМИ-120📌\n\n" +
                                                               $"Расписание экзаменационной сессии будет доступно позднее!",
                cancellationToken: cancellationToken);

            Logger.Info("View session schedule for group PMI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PMI. Error message: {ex.Message}");
        }
    }

    /// <summary>
    /// Метод, показывающий расписание сессии группы ПРИ-121
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetSessionOnPRI(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"📌Расписание сессии группы ПРИ-121📌\n\n" +
                                                               $"Расписание экзаменационной сессии будет доступно позднее!",
                cancellationToken: cancellationToken);

            Logger.Info("View session schedule for group PMI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PRI. Error message: {ex.Message}");
        }
    }
}