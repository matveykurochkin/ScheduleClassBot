using ScheduleClassBot.Constants;
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
                new KeyboardButton(BotConstants.GroupPmi),
                new KeyboardButton(BotConstants.GroupPri)
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