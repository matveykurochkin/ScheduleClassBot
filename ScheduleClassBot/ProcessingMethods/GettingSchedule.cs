using System.Globalization;
using System.Text;
using NLog;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using File = System.IO.File;

namespace ScheduleClassBot.ProcessingMethods;

public class ScheduleItem
{
    public string? pair { get; set; }
    public string? time { get; set; }
    public string? lesson { get; set; }
    public string? teacher { get; set; }
}

public class Schedule
{
    public List<ScheduleItem>? Numerator { get; set; }
    public List<ScheduleItem>? Denominator { get; set; }
}

public class Timetable
{
    public string? day { get; set; }
    public Schedule? schedule { get; set; }
}

internal class GettingSchedule : ICheckMessage
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private string? _addedToResponseText;

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

    private string GetScheduleString(Timetable timetable)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\ud83d\udcccЧИСЛИТЕЛЬ:");
        AppendSchedule(sb, timetable.schedule!.Numerator!);
        sb.AppendLine();
        sb.AppendLine("\ud83d\udcccЗНАМЕНАТЕЛЬ:");
        AppendSchedule(sb, timetable.schedule!.Denominator!);
        return sb.ToString();
    }

    private void AppendSchedule(StringBuilder sb, List<ScheduleItem> schedule)
    {
        foreach (var item in schedule)
        {
            sb.AppendLine($"{item.pair} {item.time}");
            sb.AppendLine($"{item.lesson}");
            sb.AppendLine($"{item.teacher}");
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
            // Получаем путь к текущей директории, из которой выполняется приложение
            var currentDirectory = Directory.GetCurrentDirectory();
            // Относительный путь к файлу JSON
            var jsonFilePath = Path.Combine(currentDirectory, "Schedule", "PRI121.json");
            var jsonString = await File.ReadAllTextAsync(jsonFilePath);
            // Десериализация в список объектов типа Timetable
            var timetableList = JsonConvert.DeserializeObject<List<Timetable>>(jsonString);
            
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
            
            // Поиск соответствующего дня в расписании
            var selectedTimetable = timetableList!.Find(t => t.day == textMessage);

            // Формирование строки с расписанием
            var scheduleString = GetScheduleString(selectedTimetable!);

            if (DayOfWeekPri.Contains(textMessage))
            {
                switch (textMessage)
                {
                    case BotConstants.MondayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Понедельник\n\n" +
                            $"{scheduleString}");
                        break;
                    case BotConstants.TuesdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Вторник\n\n" +
                            $"{scheduleString}");
                        break;
                    case BotConstants.WednesdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Среду\n\n" +
                            $"{scheduleString}");
                        break;
                    case BotConstants.ThursdayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Четверг\n\n" +
                            $"{scheduleString}");
                        break;
                    case BotConstants.FridayPri:
                        await botClient.SendTextMessageAsync(message.Chat,
                            $"{_addedToResponseText}📌Расписание на Пятницу\n\n" +
                            $"{scheduleString}");
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