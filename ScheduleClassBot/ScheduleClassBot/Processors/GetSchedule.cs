using NLog;
using System.Globalization;
using ScheduleClassBot.Internal;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleClassBot.Processors;
internal class GetSchedule
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string _numerator = "ЧИСЛИТЕЛЬ", _denominator = "ЗНАМЕНАТЕЛЬ", _text = "";
    private static int _weekNumber, _numeratorAndDenominator;
    private static DayOfWeek today;

    internal static readonly string[] dayOfWeekPMI = { "Понедельник ПМИ-120", "Вторник ПМИ-120", "Среда ПМИ-120", "Четверг ПМИ-120", "Пятница ПМИ-120" };
    internal static readonly string[] dayOfWeekPRI = { "Понедельник ПРИ-121", "Вторник ПРИ-121", "Среда ПРИ-121", "Четверг ПРИ-121", "Пятница ПРИ-121" };
    private static string[] dayOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

    public static string GetTodaySchedule(string[] dayArr, DayOfWeek today)
    {
        int todayIndex = Array.IndexOf(dayOfWeek, today.ToString());

        if (todayIndex >= dayArr.Length)
        {
            _text += $"❗ВЫХОДНЫЕ, показано расписание на понедельник!❗\n\n";
            return dayArr[0];
        }
        else
            return dayArr[todayIndex];
    }

    public static string GetTomorrowSchedule(string[] dayArr, DayOfWeek today)
    {
        int todayIndex = Array.IndexOf(dayOfWeek, today.ToString());

        if (todayIndex == 4 || todayIndex == 5)
        {
            _text += $"❗ЗАВТРА ВЫХОДНОЙ, показано расписание на понедельник!❗\n\n";
            return dayArr[0];
        }
        else if (todayIndex == 6)
        {
            return dayArr[0];
        }
        else
            return dayArr[todayIndex + 1];
    }

    internal static async Task GetButtonForGroup(ITelegramBotClient botClient, Message message, Update update, string nameGroup)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, ты выбрал(а) группу {nameGroup}!", replyMarkup: BotButtons.AllGroup(nameGroup));
        }
        catch (Exception ex)
        {
            _logger.Error($"Error view button for all group. Error message: {ex.Message}");
        }
    }

    internal static async Task GetScheduleForGroupPMI(ITelegramBotClient botClient, Message message, string textMessage)
    {
        try
        {
            _weekNumber = ISOWeek.GetWeekOfYear(DateTime.Now);
            _numeratorAndDenominator = _weekNumber % 2;
            today = DateTime.Now.DayOfWeek;
            _text = _numeratorAndDenominator == 0 ? $"❗Текущая неделя: {_denominator}❗\n\n" : $"❗Текущая неделя: {_numerator}❗\n\n";

            if (textMessage == "Расписание на сегодня ПМИ-120" || textMessage == "/todaypmi")
                textMessage = GetTodaySchedule(dayOfWeekPMI, today);

            if (textMessage == "Расписание на завтра ПМИ-120" || textMessage == "/tomorrowpmi")
                textMessage = GetTomorrowSchedule(dayOfWeekPMI, today);

            if (dayOfWeekPMI.Contains(textMessage))
            {
                switch (textMessage)
                {
                    case "Понедельник ПМИ-120":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Понедельник (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nВеб - программирование и основы веб - дизайна лк 318 - 3\nЛексин А.Ю.\n" +
                            $"3⃣ пара 12:10 - 13:40\nОсновы естествознания пр 420 - 3\nХмельницкая Е.В.\n" +
                            $"4⃣ пара 14:00 - 15:30\nВеб - программирование и основы веб - дизайна лб 511б - 3\nБухаров Д.Н.\n\n" +
                            $"📌Расписание на Понедельник (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nВеб - программирование и основы веб - дизайна лк 318 - 3\nЛексин А.Ю.\n" +
                            $"3⃣ пара 12:10 - 13:40\nМетоды оптимизации и исследование операций лб 423 - 2\nАбрахин С.И.");
                        break;
                    case "Вторник ПМИ-120":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Вторник (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nУравнения математической физики лк 528-3\nМастерков Ю.В.\n" +
                            $"2⃣ пара 10:20 - 11:50\nОсновы естествознания лк Г-3\nХмельницкая Е.В.\n" +
                            $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nЖив О.Г.\n\n" +
                            $"📌Расписание на Вторник (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nУравнения математической физики лк 528-3\nМастерков Ю.В.\n" +
                            $"2⃣ пара 10:20 - 11:50\nПравоведение лк В-3\nАбрамова О.К.\n" +
                            $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nЖив О.Г.");
                        break;
                    case "Среда ПМИ-120":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Среду (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nМетоды оптимизации и исследование операций лк 420-3\nАбрахин С.И.\n" +
                            $"2⃣ пара 10:20 - 11:50\nУравнения математической физики пр 405-3\nМастерков Ю.В.\n" +
                            $"3⃣ пара 12:10 - 13:40\nТехнология разработки программного обеспечения пр 511б-3\nПроскурина Г.В.\n\n" +
                            $"📌Расписание на Среду (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nУравнения математической физики пр 405-3\nМастерков Ю.В.\n" +
                            $"3⃣ пара 12:10 - 13:40\nПравоведение пр 431-3\nАбрамова О.К.");
                        break;
                    case "Четверг ПМИ-120":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Четверг (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nТеория случайных процессов лк 430-3\nБуланкина Л.А.\n" +
                            $"3⃣ пара 12:10 - 13:40\nТеория случайных процессов пр 430-3\nБуланкина Л.А.\n\n" +
                            $"📌Расписание на Четверг (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nТеория случайных процессов лб 430-3\nБуланкина Л.А.\n" +
                            $"2⃣ пара 10:20 - 11:50\nТеория случайных процессов лк 430-3\nБуланкина Л.А.\n" +
                            $"3⃣ пара 12:10 - 13:40\nТехнология разработки программного обеспечения лб 423-2\nПроскурина Г.В.");
                        break;
                    case "Пятница ПМИ-120":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Пятницу (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nЭлективные дисциплины по физической культуре и спорту пр\nЖив О.Г.\n" +
                            $"2⃣ пара 10:20 - 11:50\nТехнология разработки программного обеспечения лк 318-3\nКрушатина М.Э.\n\n" +
                            $"📌Расписание на Пятницу (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nВеб-программирование и основы веб-дизайна пр 511б-3\nЛексин А.Ю.\n" +
                            $"2⃣ пара 10:20 - 11:50\nТехнология разработки программного обеспечения лк 318-3\nКрушатина М.Э.");
                        break;
                }
            }
            _logger.Info($"Сообщение \"{message?.Text}\" успешно обработано!");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error view schedule for group PMI. Error message: {ex.Message}");
        }
    }

    internal static async Task GetScheduleForGroupPRI(ITelegramBotClient botClient, Message message, string textMessage)
    {
        try
        {
            _weekNumber = ISOWeek.GetWeekOfYear(DateTime.Now);
            _numeratorAndDenominator = _weekNumber % 2;
            today = DateTime.Now.DayOfWeek;
            _text = _numeratorAndDenominator == 0 ? $"❗Текущая неделя: {_denominator}❗\n\n" : $"❗Текущая неделя: {_numerator}❗\n\n";

            if (textMessage == "Расписание на сегодня ПРИ-121" || textMessage == "/todaypri")
                textMessage = GetTodaySchedule(dayOfWeekPRI, today);

            if (textMessage == "Расписание на завтра ПРИ-121" || textMessage == "/tomorrowpri")
                textMessage = GetTomorrowSchedule(dayOfWeekPRI, today);

            if (dayOfWeekPRI.Contains(textMessage))
            {
                switch (textMessage)
                {
                    case "Понедельник ПРИ-121":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Понедельник (Числитель)\n" +
                             $"2⃣ пара 10:20 - 11:50\nМультимедиа технологии лк 213-3\nОзерова М.И.\n" +
                             $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                             $"4⃣ пара 14:00 - 15:30\nУправление данными лб 418-2\nДанилов В.В.\n" +
                             $"5⃣ пара 15:50 - 17:20\nУправление данными лб 418-2\nДанилов В.В.\n\n" +
                             $"📌Расписание на Понедельник (Знаменатель)\n" +
                             $"2⃣ пара 10:20 - 11:50\nМультимедиа технологии лк 213-3\nОзерова М.И.\n" +
                             $"3⃣ пара 12:10 - 13:40\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                             $"4⃣ пара 14:00 - 15:30\nУправление данными лб 418-2\nДанилов В.В.\n" +
                             $"5⃣ пара 15:50 - 17:20\nУправление данными лб 418-2\nДанилов В.В.");
                        break;
                    case "Вторник ПРИ-121":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Вторник (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nБазовые информационные технологии лб 414-2\nКириллова С.Ю.\n" +
                            $"3⃣ пара 12:10 - 13:40\nПлатформонезависимое программирование лб 404а-2\nШамышев А.А.\n" +
                            $"4⃣ пара 14:00 - 15:30\nБазовые информационные технологии лк 410-2\nКириллова С.Ю.\n" +
                            $"5⃣ пара 15:50 - 17:20\nТехнологии программирования лб 404а-2\nДанилов В.В.\n" +
                            $"6️⃣ пара 17:40 - 19:10\nТехнологии программирования лб 404а-2\nДанилов В.В.\n\n" +
                            $"📌Расписание на Вторник (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nБазовые информационные технологии лб 414-2\nКириллова С.Ю.\n" +
                            $"3⃣ пара 12:10 - 13:40\nПлатформонезависимое программирование лб 404а-2\nШамышев А.А.\n" +
                            $"4⃣ пара 14:00 - 15:30\nБазовые информационные технологии лк 410-2\nКириллова С.Ю.\n" +
                            $"5⃣ пара 15:50 - 17:20\nТехнологии программирования лб 404а-2\nДанилов В.В.\n" +
                            $"6️⃣ пара 17:40 - 19:10\nТехнологии программирования лб 404а-2\nДанилов В.В.");
                        break;
                    case "Среда ПРИ-121":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Среду (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nМультимедиа технологии лб 314-3\nЛанская М.С.\n" +
                            $"3⃣ пара 12:10 - 13:40\nИностранный язык пр 303б-1 Иностранный язык пр 331-1\nКойкова Т.И.англ. Ермолаева Л.Д.англ.\n" +
                            $"4⃣ пара 14:00 - 15:30\nПлатформонезависимое программирование лк 410-2\nПроскурина Г.В.\n\n" +
                            $"📌Расписание на Среду (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nМультимедиа технологии лб 314-3\nЛанская М.С.\n" +
                            $"3⃣ пара 12:10 - 13:40\nИностранный язык пр 125-1 Иностранный язык пр 331-1\nКойкова Т.И.англ. Ермолаева Л.Д.англ.\n" +
                            $"4⃣ пара 14:00 - 15:30\nПлатформонезависимое программирование лк 410-2\nПроскурина Г.В.");
                        break;
                    case "Четверг ПРИ-121":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Четверг (Числитель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                            $"2⃣ пара 10:20 - 11:50\nПравоведение лк Г-3\nМамедов С.Н.\n\n" +
                            $"📌Расписание на Четверг (Знаменатель)\n" +
                            $"1⃣ пара 08:30 - 10:00\nЭлективные дисциплины по физической культуре и спорту пр\nТарасевич О.Д.\n" +
                            $"2⃣ пара 10:20 - 11:50\nПравоведение пр 303б-1\nАсадов Р.Б.\n" +
                            $"3⃣ пара 12:10 - 13:40\nУправление данными пр 410-2\nВершинин В.В.");
                        break;
                    case "Пятница ПРИ-121":
                        await botClient.SendTextMessageAsync(message.Chat, $"{_text}📌Расписание на Пятницу (Числитель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nУправление данными лк 410-2\nВершинин В.В.\n" +
                            $"3⃣ пара 12:10 - 13:40\nТехнологии программирования лк 410-2\nВершинин В.В.\n\n" +
                            $"📌Расписание на Пятницу (Знаменатель)\n" +
                            $"2⃣ пара 10:20 - 11:50\nУправление данными лк 410-2\nВершинин В.В.\n" +
                            $"3⃣ пара 12:10 - 13:40\nТехнологии программирования лк 410-2\nВершинин В.В.");
                        break;
                }
            }
            _logger.Info($"Сообщение \"{message?.Text}\" успешно обработано!");
        }
        catch (Exception ex)
        {
            _logger.Error($"Error view schedule for group PRI. Error message: {ex.Message}");
        }
    }
}