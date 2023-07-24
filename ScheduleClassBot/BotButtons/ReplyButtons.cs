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
                new KeyboardButton($"{BotConstants.Monday} {nameGroup}"),
                new KeyboardButton($"{BotConstants.Tuesday} {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"{BotConstants.Wednesday} {nameGroup}"),
                new KeyboardButton($"{BotConstants.Thursday} {nameGroup}")
            },
            new[]
            {
                new KeyboardButton($"{BotConstants.Friday} {nameGroup}"),
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