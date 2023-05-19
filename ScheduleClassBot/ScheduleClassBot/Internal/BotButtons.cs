using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot;

internal class BotButtons
{
    public static IReplyMarkup MainButtonOnBot()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
    new[]
    {
        new KeyboardButton("Узнать расписание 📜")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }
    public static IReplyMarkup ListGroup()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
    new[]
    {
        new KeyboardButton("ПМИ-120"),
        new KeyboardButton("ПРИ-121")
    },
    new[]
    {
        new KeyboardButton("Назад ⬅")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }

    public static IReplyMarkup AllGroup(string nameGroup)
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
        new KeyboardButton($"Список групп 📋")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }
}
