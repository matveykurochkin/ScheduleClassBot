using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSessionSchedule
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task GetSessionOnPMI(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"📌Расписание сессии группы ПМИ-120📌\n\n" +
                                                               $"1⃣  Экзамен Уравнения математической физики Мастерков Ю.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Вторник 13.06.2023 16:00 405 - 3\n" +
                                                               $"ЭКЗАМЕН: Среда 14.06.2023 08:30 405 - 3\n\n" +
                                                               $"2⃣  ЭКЗАМЕН Веб - программирование и основы веб - дизайна Лексин А.Ю.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 16.06.2023 14:00 511г - 3\n" +
                                                               $"ЭКЗАМЕН: Понедельник 19.06.2023 08:30 511г - 3\n\n" +
                                                               $"3⃣  ЭКЗАМЕН Теория случайных процессов Буланкина Л.А.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 23.06.2023 16:00 405 - 3\n" +
                                                               $"ЭКЗАМЕН: Понедельник 26.06.2023 08:30 230 - 3\n\n" +
                                                               $"4⃣  ЭКЗАМЕН Методы оптимизации и исследование операций Абрахин С.И.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Четверг 29.06.2023 16:00 100 - 3\n" +
                                                               $"ЭКЗАМЕН: Пятница 30.06.2023 08:30 100 - 3\n",
                cancellationToken: cancellationToken);

            Logger.Info("View session schedule for group PMI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PMI. Error message: {ex.Message}");
        }
    }

    public async Task GetSessionOnPRI(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"📌Расписание сессии группы ПРИ-121📌\n\n" +
                                                               $"1⃣  Экзамен Мультимедиа технологии Озерова М.И.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 09.06.2023 14:00 213 - 3\n" +
                                                               $"ЭКЗАМЕН: Вторник 13.06.2023 08:30 213 - 3\n\n" +
                                                               $"2⃣  ЭКЗАМЕН Управление данными Вершинин В.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 16.06.2023 14:00 205в - 3\n" +
                                                               $"ЭКЗАМЕН: Понедельник 19.06.2023 08:30 205в - 3\n\n" +
                                                               $"3⃣  ЭКЗАМЕН Платформонезависимое программирование Проскурина Г.В.\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Пятница 23.06.2023 14:00 418 - 2\n" +
                                                               $"ЭКЗАМЕН: Понедельник 26.06.2023 08:30 418 - 2\n\n" +
                                                               $"4⃣  ЭКЗАМЕН Иностранный язык Койкова Т.И.(англ.) Ермолаева Л.Д.(англ.)\n" +
                                                               $"КОНСУЛЬТАЦИЯ: Четверг 29.06.2023 14:00 127б - 1 Ермолаева Л.Д.\n" +
                                                               $"Четверг 29.06.2023 14:00 135 - 1 Койкова Т.И.\n" +
                                                               $"ЭКЗАМЕН: Пятница 30.06.2023 08:30 127б - 1 Ермолаева Л.Д.\n" +
                                                               $"Пятница 30.06.2023 08:30 135 - 1 Койкова Т.И.\n",
                cancellationToken: cancellationToken);

            Logger.Info("View session schedule for group PMI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PRI. Error message: {ex.Message}");
        }
    }
}