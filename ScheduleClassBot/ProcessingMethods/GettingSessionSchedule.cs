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
                                                               $"1⃣  Экзамен Математическое моделирование Прохоров А.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Вторник 09.01.2024 17:30 420 - 3\n" +
                                                               $"ЭКЗАМЕН: Среда 10.01.2024 12:00 420 - 3\n\n" +
                                                               $"2⃣  Экзамен Параллельное программирование и основы суперкомпьютерных технологий Голубев А.С.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 12.01.2024 14:00 511б - 3\n" +
                                                               $"ЭКЗАМЕН: Понедельник 15.01.2024 12:00 511г - 3\n\n" +
                                                               $"3⃣  Экзамен Защита информации  Бухаров Д.Н.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 17.01.2024 17:30 511б - 3\n" +
                                                               $"ЭКЗАМЕН: Четверг 18.01.2024 12:00 511б - 3\n\n",
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
                                                               $"1⃣  Экзамен Информационные сети Курочкин С.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Среда 10.01.2024 14:00 314 - 3\n" +
                                                               $"ЭКЗАМЕН: Четверг 11.01.2024 08:30 414 - 2\n\n" +
                                                               $"2⃣  Экзамен  Технологии программирования Вершинин В.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Понедельник 15.01.2024 14:00 420 - 2\n" +
                                                               $"ЭКЗАМЕН: Вторник 16.01.2024 08:30 404а - 2\n\n" +
                                                               $"3⃣  Экзамен Интерактивные графические системы Монахова Г.Е.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 19.01.2024 14:00 314 - 3\n" +
                                                               $"ЭКЗАМЕН: Четверг 20.01.2024 08:30 314 - 3\n\n",
                cancellationToken: cancellationToken);
            
            Logger.Info("View session schedule for group PRI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PRI. Error message: {ex.Message}");
        }
    }
}