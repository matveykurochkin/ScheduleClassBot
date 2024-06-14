using ScheduleClassBot.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class ReplyButtons
{
    /// <summary>
    /// Метод, отвечающий за кнопки доступных для просмотра расписания групп
    /// </summary>
    /// <returns></returns>
    public IReplyMarkup MainButtonOnBot()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton(BotConstants.GroupPri)
            }
        })
        {
            ResizeKeyboard = true
        };
        return tgButton;
    }

    /// <summary>
    /// Метод, составляющий сетку расписания для запращиваемой группы
    /// </summary>
    /// <param name="nameGroup">название группы для которой требуется создать сетку возможных вариантов просмотра расписания</param>
    /// <returns></returns>
    public IReplyMarkup AllGroup(string nameGroup)
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton($"Понедельник {nameGroup}"),
                new KeyboardButton($"Вторник {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"Среда {nameGroup}"),
                new KeyboardButton($"Четверг {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"Пятница {nameGroup}"),
                new KeyboardButton($"{BotConstants.ScheduleSession} {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"{BotConstants.ScheduleToday} {nameGroup}"),
                new KeyboardButton($"{BotConstants.ScheduleTomorrow} {nameGroup}")
            },
            new[]
            {
                new KeyboardButton(BotConstants.CommandBack)
            }
        })
        {
            ResizeKeyboard = true
        };
        return tgButton;
    }
}