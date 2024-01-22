using System.Globalization;
using NLog;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSchedule : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private string? _addedToResponseText;

    /// <summary>
    /// массив, содержащий дни недели для группы ПМИ, пример: "Среда ПМИ-120"
    /// </summary>
    internal static readonly string[] DayOfWeekPmi =
        { BotConstants.MondayPmi, BotConstants.TuesdayPmi, BotConstants.WednesdayPmi, BotConstants.ThursdayPmi, BotConstants.FridayPmi };

    /// <summary>
    /// массив, содержащий дни недели для группы ПРИ, пример: "Среда ПРИ-121"
    /// </summary>
    internal static readonly string[] DayOfWeekPri =
        { BotConstants.MondayPri, BotConstants.TuesdayPri, BotConstants.WednesdayPri, BotConstants.ThursdayPri, BotConstants.FridayPri };

    /// <summary>
    /// массив, содержащий дни недели 
    /// </summary>
    private static readonly string[] DayOfWeek =
        { BotConstants.Monday, BotConstants.Tuesday, BotConstants.Wednesday, BotConstants.Thursday, BotConstants.Friday, BotConstants.Saturday, BotConstants.Sunday };

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
    /// Метод, который получает сегодняшний день недели для переданного массива DayOfWeekPmi или DayOfWeekPri
    /// </summary>
    /// <param name="dayArr">один из массивов DayOfWeekPmi или DayOfWeekPri</param>
    /// <param name="today">сегодняшний день</param>
    /// <returns>возвращает сегодняшний день недели для переданного массива DayOfWeekPmi или DayOfWeekPri</returns>
    private string GetTodaySchedule(IReadOnlyList<string> dayArr, DayOfWeek today)
    {
        var todayIndex = Array.IndexOf(DayOfWeek, today.ToString());

        if (todayIndex < dayArr.Count) return dayArr[todayIndex];
        _addedToResponseText += BotConstants.WeekendsToday;
        return dayArr[0];
    }

    /// <summary>
    /// Метод, который получает завтрашний день недели для переданного массива DayOfWeekPmi или DayOfWeekPri
    /// </summary>
    /// <param name="dayArr">один из массивов DayOfWeekPmi или DayOfWeekPri</param>
    /// <param name="today">завтрашний день</param>
    /// <returns>возвращает завтрашний день недели для переданного массива DayOfWeekPmi или DayOfWeekPri</returns>
    private string GetTomorrowSchedule(IReadOnlyList<string> dayArr, DayOfWeek today)
    {
        var todayIndex = Array.IndexOf(DayOfWeek, today.ToString());

        switch (todayIndex)
        {
            case 4:
            case 5:
                _addedToResponseText += BotConstants.WeekendsTomorrow;
                return dayArr[0];
            case 6:
                return dayArr[0];
            default:
                return dayArr[todayIndex + 1];
        }
    }

    /// <summary>
    /// Метод, который после выбора группы пользователем, вызывает метод AllGroup, который составляет сетку
    /// возможных вариантов запроса расписания для указанной группы
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="update"></param>
    /// <param name="nameGroup">название группы, для которой необходимо составить сетку</param>
    internal async Task GetButtonForGroup(ITelegramBotClient botClient, Message message, Update update, string nameGroup)
    {
        try
        {
            var botButtons = new BotButtons.ReplyButtons();
            await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, ты выбрал(а) группу {nameGroup}!",
                replyMarkup: botButtons.AllGroup(nameGroup));
        }
        catch (Exception ex)
        {
            Logger.Error("Error view button for all group. {method}: {error}", nameof(GetButtonForGroup), ex);
        }
    }

    /// <summary>
    /// Главный метод, который обрабатывет все возможные варианты из сетки запросов расписания, работает для группы ПМИ-120
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="textMessage">полученное сообщение от пользователя</param>
    // ReSharper disable once InconsistentNaming
    internal async Task GetScheduleForGroupPMI(ITelegramBotClient botClient, Message message, string textMessage)
    {
        try
        {
            var today = DateTime.Now.DayOfWeek;
            _addedToResponseText = ISOWeek.GetWeekOfYear(DateTime.Now) % 2 == 0
                ? $"❗Текущая неделя: {BotConstants.Denominator}❗\n\n"
                : $"❗Текущая неделя: {BotConstants.Numerator}❗\n\n";

            if (CheckingMessageText(textMessage, BotConstants.ScheduleForPmiToday)
                || CheckingMessageText(textMessage, BotConstants.CommandTodayPmi))
                textMessage = GetTodaySchedule(DayOfWeekPmi, today);

            if (CheckingMessageText(textMessage, BotConstants.ScheduleForPmiTomorrow)
                || CheckingMessageText(textMessage, BotConstants.CommandTomorrowPmi))
                textMessage = GetTomorrowSchedule(DayOfWeekPmi, today);

            if (DayOfWeekPmi.Contains(textMessage))
            {
                switch (textMessage)
                {
                    case BotConstants.MondayPmi:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Понедельник (Числитель)\n" +
                            $"4⃣ пара 14:00 - 15:30\nНечёткие множества в управлении и принятии решений лк 120-3\nЧернов В.Г.\n" +
                            $"5⃣ пара 15:50 - 17:20\nНечёткие множества в управлении и принятии решений лб 511г-3\nЧернов В.Г.\n\n" +
                            $"📌Расписание на Понедельник (Знаменатель)\n" +
                            $"4⃣ пара 14:00 - 15:30\nНечёткие множества в управлении и принятии решений лк 111-3\nЧернов В.Г.\n" +
                            $"5⃣ пара 15:50 - 17:20\nНечёткие множества в управлении и принятии решений лб 511г-3\nЧернов В.Г.\n");
                        break;
                    case BotConstants.TuesdayPmi:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Вторник (Числитель)\n" +
                            $"5⃣ пара 15:50 - 17:20\nСистемный анализ лк 420-3\nЛексин А.Ю.\n" +
                            $"6️⃣ пара 17:40 - 19:10\nСистемный анализ пр 420-3\nЛексин А.Ю.\n\n" +
                            $"📌Расписание на Вторник (Знаменатель)\n" +
                            $"5⃣ пара 15:50 - 17:20\nНечёткие множества в управлении и принятии решений пр 100-3\nЧернов В.Г.\n" +
                            $"6️⃣ пара 17:40 - 19:10\nСистемный анализ пр 420-3\nЛексин А.Ю.\n");
                        break;
                    case BotConstants.WednesdayPmi:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Среду (Числитель)\n" +
                            $"3⃣ пара 12:10 - 13:40\nСистемный анализ лк 422-3 425-3 428-3 \nЛексин А.Ю.\n" +
                            $"4⃣ пара 14:00 - 15:30\nНечёткие множества в управлении и принятии решений лк 119-3\nЧернов В.Г.\n\n" +
                            $"📌Расписание на Среду (Знаменатель)\n" +
                            $"4⃣ пара 14:00 - 15:30\nНечёткие множества в управлении и принятии решений лк 120-3\nЧернов В.Г.\n" +
                            $"5⃣ пара 15:50 - 17:20\nНечёткие множества в управлении и принятии решений лб 511б-3\nЧернов В.Г.");
                        break;
                    case BotConstants.ThursdayPmi:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Четверг (Числитель)\n" +
                            $"3⃣ пара 12:10 - 13:40\nОсновы научно - технического перевода пр 426-3\nИванова И.С.нем.\n" +
                            $"4⃣ пара 14:00 - 15:30\nПолитология лк А-3\nЕфимова С.А.\n" +
                            $"5⃣ пара 15:50 - 17:20\nОсновы научно - технического перевода пр 326а-1\nТагиева Э.Р.англ.\n\n" +
                            $"📌Расписание на Четверг (Знаменатель)\n" +
                            $"3⃣ пара 12:10 - 13:40\nОсновы научно - технического перевода пр 426-3\nИванова И.С.нем.\n" +
                            $"4⃣ пара 14:00 - 15:30\nПолитология лк А-3\nЕфимова С.А.\n" +
                            $"5⃣ пара 15:50 - 17:20\nОсновы научно - технического перевода пр 326а-1\nТагиева Э.Р.англ.");
                        break;
                    case BotConstants.FridayPmi:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Пятницу (Числитель)\n" +
                            $"3⃣ пара 12:10 - 13:40\nПолитология пр 201б-3\nЕфимова С.А.\n" +
                            $"4⃣ пара 14:00 - 15:30\nПолитология пр 201б-3\nЕфимова С.А.\n\n" +
                            $"📌Расписание на Пятницу (Знаменатель)\n" +
                            $"3⃣ пара 12:10 - 13:40\nПолитология лк Г-3\nЕфимова С.А.\n" +
                            $"4⃣ пара 14:00 - 15:30\nОсновы научно - технического перевода пр 426-3\nТагиева Э.Р.англ.\nОсновы научно - технического перевода пр 430-3\nИванова И.С.нем.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error view schedule for group PMI. {method}: {error}", nameof(GetScheduleForGroupPMI), ex);
        }
    }

    /// <summary>
    /// Главный метод, который обрабатывет все возможные варианты из сетки запросов расписания, работает для группы ПРИ-121
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="textMessage">полученное сообщение от пользователя</param>
    // ReSharper disable once InconsistentNaming
    internal async Task GetScheduleForGroupPRI(ITelegramBotClient botClient, Message message, string textMessage)
    {
        try
        {
            var today = DateTime.Now.DayOfWeek;
            _addedToResponseText = ISOWeek.GetWeekOfYear(DateTime.Now) % 2 == 0
                ? $"❗Текущая неделя: {BotConstants.Denominator}❗\n\n"
                : $"❗Текущая неделя: {BotConstants.Numerator}❗\n\n";

            if (CheckingMessageText(textMessage, BotConstants.ScheduleForPriToday)
                || CheckingMessageText(textMessage, BotConstants.CommandTodayPri))
                textMessage = GetTodaySchedule(DayOfWeekPri, today);

            if (CheckingMessageText(textMessage, BotConstants.ScheduleForPriTomorrow)
                || CheckingMessageText(textMessage, BotConstants.CommandTomorrowPri))
                textMessage = GetTomorrowSchedule(DayOfWeekPri, today);

            if (DayOfWeekPri.Contains(textMessage))
            {
                switch (textMessage)
                {
                    case BotConstants.MondayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Понедельник (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                            $"2⃣ пара 10:20 - 11:50\nCASE - технологии лк 410-2\nБородина Е.К.\n" +
                            $"3⃣ пара 12:10 - 13:40\nРаспределенные программные системы лб 404а-2 (подгруппа 1)\nПроскурина Г.В.\n" +
                            $"Интегрированные информационные системы лб 117-3 с 1 нед. по 8 нед. (подгруппа 2)\nГрадусов Д.А.\n" +
                            $"4⃣ пара 14:00 - 15:30\nCASE - технологии лб 418-2 (подгруппа 1)\nБородина Е.К.\n" +
                            $"Интегрированные информационные системы лб 117-3 с 1 нед. по 8 нед. (подгруппа 2)\nГрадусов Д.А.\n\n" +
                            $"📌Расписание на Понедельник (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nОсновы разработки веб - приложений лк 408-2\nСпирин И.В.\n" +
                            $"2⃣ пара 10:20 - 11:50\nCASE - технологии лк 410-2\nБородина Е.К.\n" +
                            $"3⃣ пара 12:10 - 13:40\nИнтегрированные информационные системы лб 117-3 с 1 нед. по 8 нед (подгруппа 1)\nГрадусов Д.А.\n" +
                            $"Распределенные программные системы лб 404а-2 (подгруппа 2)\nПроскурина Г.В.\n" +
                            $"4⃣ пара 14:00 - 15:30\nИнтегрированные информационные системы лб 117-3 с 1 нед. по 8 нед (подгруппа 1)\nГрадусов Д.А.\n" +
                            $"CASE - технологии лб 418-2 (подгруппа 2)\nБородина Е.К.\n");
                        break;
                    case BotConstants.TuesdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Вторник (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nРаспределенные программные системы лк 410-2\nТимофеев А.А.\n" +
                            $"2⃣ пара 10:20 - 11:50\nCASE - технологии пр 414-2\nБородина Е.К.\n\n" +
                            $"📌Расписание на Вторник (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nРаспределенные программные системы лк 410-2\nТимофеев А.А.\n" +
                            $"2⃣ пара 10:20 - 11:50\nРаспределенные программные системы пр 418 - 2\nПроскурина Г.В.");
                        break;
                    case BotConstants.WednesdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Среду (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nИнтегрированные информационные системы лк 120-3\nГрадусов Д.А.\n" +
                            $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                            $"4⃣ пара 14:00 - 15:30\nАдБ ПИС лб 404а-2\nКурочкин С.В. (подруппа 1)\n" +
                            $"5⃣ пара 15:50 - 17:20\nАдБ ПИС лб 404а-2\nКурочкин С.В. (подруппа 1)\n\n" +
                            $"📌Расписание на Среду (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nИнтегрированные информационные системы лк 120-3\nГрадусов Д.А.\n" +
                            $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                            $"4⃣ пара 14:00 - 15:30\nАдБ ПИС лб 404а-2\nКурочкин С.В. (подруппа 2)\n" +
                            $"5⃣ пара 15:50 - 17:20\nАдБ ПИС лб 404а-2\nКурочкин С.В. (подруппа 2)\n\n");
                        break;
                    case BotConstants.ThursdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Четверг (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nГеоинформационные технологии лб 314-3  (подгруппа 2)\nМонахова Г.Е.\n" +
                            $"2⃣ пара 10:20 - 11:50\nГеоинформационные технологии лб 314-3  (подгруппа 2)\nМонахова Г.Е.\n" +
                            $"3⃣ пара 12:10 - 13:40\nОсновы разработки веб - приложений лб 424-2 (подгруппа 1)\nСпирин И.В.\n" +
                            $"4⃣ пара 14:00 - 15:30\nОсновы разработки веб - приложений лб 424-2 (подгруппа 1)\nСпирин И.В.\n\n" +
                            $"📌Расписание на Четверг (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nГеоинформационные технологии лб 314-3  (подгруппа 1)\nМонахова Г.Е.\n" +
                            $"2⃣ пара 10:20 - 11:50\nГеоинформационные технологии лб 314-3  (подгруппа 1)\nМонахова Г.Е.\n" +
                            $"3⃣ пара 12:10 - 13:40\nОсновы разработки веб - приложений лб 424-2 (подгруппа 2)\nСпирин И.В.\n" +
                            $"4⃣ пара 14:00 - 15:30\nОсновы разработки веб - приложений лб 424-2 (подгруппа 2)\nСпирин И.В.");
                        break;
                    case BotConstants.FridayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Пятницу (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nГеоинформационные технологии лк 410-2 \nМонахова Г.Е\n" +
                            $"2⃣ пара 10:20 - 11:50\nАдБ ПИС лк 120-3\nКурочкин С.В.\n\n" +
                            $"📌Расписание на Пятницу (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nГеоинформационные технологии лк 410-2 \nМонахова Г.Е\n" +
                            $"2⃣ пара 10:20 - 11:50\nАдБ ПИС лк 213-3\nКурочкин С.В.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error view schedule for group PRI. {method}: {error}", nameof(GetScheduleForGroupPRI), ex);
        }
    }
}