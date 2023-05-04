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

    public static IReplyMarkup GroupPMI()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
    new[]
    {
        new KeyboardButton("Понедельник ПМИ-120"),
        new KeyboardButton("Вторник ПМИ-120")
    },new[]
    {
        new KeyboardButton("Среда ПМИ-120"),
        new KeyboardButton("Четверг ПМИ-120")
    },new[]
    {
        new KeyboardButton("Пятница ПМИ-120"),
        new KeyboardButton("Список групп 📋")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }

    public static IReplyMarkup GroupPRI()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
    new[]
    {
        new KeyboardButton("Понедельник ПРИ-121"),
        new KeyboardButton("Вторник ПРИ-121")
    },new[]
    {
        new KeyboardButton("Среда ПРИ-121"),
        new KeyboardButton("Четверг ПРИ-121")
    },new[]
    {
        new KeyboardButton("Пятница ПРИ-121"),
        new KeyboardButton("Список групп 📋")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }
}
