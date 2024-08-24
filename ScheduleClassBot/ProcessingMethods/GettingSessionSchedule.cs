using System.Text;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using File = System.IO.File;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSessionSchedule
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private class Consultation
    {
        public string? date { get; set; }
        public string? time { get; set; }
        public string? room { get; set; }
    }

    private class Exam
    {
        public string? date { get; set; }
        public string? time { get; set; }
        public string? room { get; set; }
    }

    private class ExamSchedule
    {
        public string? number { get; set; }
        public string? subject { get; set; }
        public string? teacher { get; set; }
        public Consultation? consultation { get; set; }
        public Exam? exam { get; set; }
    }

    private string FormatExamSchedules(List<ExamSchedule> examSchedules)
    {
        var result = new StringBuilder();
        foreach (var exam in examSchedules)
        {
            result.AppendLine($"{exam.number} {exam.subject} {exam.teacher}");
            result.AppendLine($"КОНСУЛЬТАЦИЯ: {exam.consultation!.date} {exam.consultation.time} {exam.consultation.room}");
            result.AppendLine($"ЭКЗАМЕН: {exam.exam!.date} {exam.exam.time} {exam.exam.room}\n");
        }
        return result.ToString();
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
            // Получаем путь к текущей директории, из которой выполняется приложение
            var currentDirectory = Directory.GetCurrentDirectory();
            // Относительный путь к файлу JSON
            var jsonFilePath = Path.Combine(currentDirectory, "Schedule", "PRI121Session.json");
            // Читаем содержимое файла JSON
            var jsonString = await File.ReadAllTextAsync(jsonFilePath, cancellationToken);
            // Десериализация в объект типа SessionSchedule
            var sessionData = JsonConvert.DeserializeObject<List<ExamSchedule>>(jsonString);
            // Получаем строку расписания сессии
            var sessionScheduleString = FormatExamSchedules(sessionData!);

            if (sessionScheduleString == "")
                sessionScheduleString = "Будет доступно позднее!";
            
            await botClient.SendTextMessageAsync(message.Chat, $"📌Расписание сессии группы ПРИ-121📌\n\n" +
                                                               $"{sessionScheduleString}",
                cancellationToken: cancellationToken);
            
            Logger.Info("View session schedule for group PRI success!");
        }
        catch (Exception ex)
        {
            Logger.Error($"Error view session schedule for group PRI. Error message: {ex.Message}");
        }
    }
}