namespace ScheduleClassBot.Constants;

/// <summary>
/// Класс, представляющий необходимые для работы бота константы,
/// сделано с целью удобства изменения повторяющегося кода,
/// класс разделен на отдельные директивы #region и #endregion, каждая из которых имеет название группы, которую они представляют
/// </summary>
public static class BotConstants
{
    #region Day Of Week all group

    public const string MondayPmi = "Понедельник ПМИ-120";
    public const string TuesdayPmi = "Вторник ПМИ-120";
    public const string WednesdayPmi = "Среда ПМИ-120";
    public const string ThursdayPmi = "Четверг ПМИ-120";
    public const string FridayPmi = "Пятница ПМИ-120";
    
    public const string MondayPri = "Понедельник ПРИ-121";
    public const string TuesdayPri = "Вторник ПРИ-121";
    public const string WednesdayPri = "Среда ПРИ-121";
    public const string ThursdayPri = "Четверг ПРИ-121";
    public const string FridayPri = "Пятница ПРИ-121";
    
    public const string Monday = "Monday";
    public const string Tuesday = "Tuesday";
    public const string Wednesday = "Wednesday";
    public const string Thursday = "Thursday";
    public const string Friday = "Friday";
    public const string Saturday = "Saturday";
    public const string Sunday = "Sunday";

    #endregion

    #region Numerator & Denominator

    public const string Numerator = "ЧИСЛИТЕЛЬ";
    public const string Denominator = "ЗНАМЕНАТЕЛЬ";

    #endregion

    #region Bot Commands

    public const string CommandStart = "/start";
    public const string CommandTodayPmi = "/todaypmi";
    public const string CommandTomorrowPmi = "/tomorrowpmi";
    public const string CommandSessionPmi = "/sessionpmi";
    public const string CommandTodayPri = "/todaypri";
    public const string CommandTomorrowPri = "/tomorrowpri";
    public const string CommandSessionPri = "/sessionpri";
    public const string CommandBack = "Назад ⬅";

    #endregion

    #region Schedule for

    public const string ScheduleForPmiToday = "Расписание на сегодня ПМИ-120";
    public const string ScheduleForPmiTomorrow = "Расписание на завтра ПМИ-120";
    public const string ScheduleForPriToday = "Расписание на сегодня ПРИ-121";
    public const string ScheduleForPriTomorrow = "Расписание на завтра ПРИ-121";
    public const string ScheduleSessionForPmi = "Расписание сессии ПМИ-120";
    public const string ScheduleSessionForPri = "Расписание сессии ПРИ-121";
    public const string ScheduleToday = "Расписание на сегодня";
    public const string ScheduleTomorrow = "Расписание на завтра";
    public const string ScheduleSession = "Расписание сессии";

    #endregion

    #region Weekends Text

    public const string WeekendsToday = "❗ВЫХОДНЫЕ, показано расписание на понедельник!❗\n\n";
    public const string WeekendsTomorrow = "❗ЗАВТРА ВЫХОДНОЙ, показано расписание на понедельник!❗\n\n";

    #endregion

    #region Group

    public const string GroupPmi = "ПМИ-120";
    public const string GroupPri = "ПРИ-121";

    #endregion

    #region Special commands

    public const string SpecialCommandForViewAllSpecialCommand = "specialcommandforviewbuttonwithlistallspecialcommands";
    public const string SpecialCommandForGetLogFile = "specialcommandforgetlogfile";
    public const string SpecialCommandForCheckYourProfile = "specialcommandforcheckyourprofile";
    public const string SpecialCommandForViewListUsers = "specialcommandforviewlistusers";
    public const string SpecialCommandForViewCountMessages = "specialcommandforviewcountmessages";
    public const string SpecialCommandForFixKeyboardLayout = "c ";

    #endregion

    #region Like & Dislike

    public const string Like = "like";
    public const string DisLike = "dislike";

    #endregion

    #region Count Messages For Present

    public const int CountMessageForPresent = 50;

    #endregion
}