namespace ScheduleClassBot.Interfaces;

internal interface ICheckMessage
{
    /// <summary>
    /// Используется для сравнения двух строк на равенство.
    /// Метод возвращает значение true, если строки совпадают, и
    /// false, во всех остальных случаях.
    /// </summary>
    /// <param name="receivedText">полученный на вход текст</param>
    /// <param name="necessaryText">текст с которым надо сравнить</param>
    /// <returns></returns>
    public bool CheckingMessageText(string receivedText, string necessaryText);
}