using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class ReplyButtons
{
    public IReplyMarkup MainButtonOnBot()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
            new[]
            {
                new KeyboardButton("ПМИ-120"),
                new KeyboardButton("ПРИ-121")
            }
        })
        {
            ResizeKeyboard = true
        };
        return tgButton;
    }

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
                new KeyboardButton($"Расписание сессии {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"Расписание на сегодня {nameGroup}"),
                new KeyboardButton($"Расписание на завтра {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"Назад ⬅")
            }
        })
        {
            ResizeKeyboard = true
        };
        return tgButton;
    }
}