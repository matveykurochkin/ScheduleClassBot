using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.Internal;
internal class SpecialBotButton
{
    public static IReplyMarkup SpecialCommandButton()
    {
        var tgButton = new ReplyKeyboardMarkup(new[]
        {
    new[]
    {
        new KeyboardButton("specialcommandforviewlistusers")
    },
        new[]
    {
        new KeyboardButton("specialcommandforviewcountmessages")
    },
        new[]
    {
        new KeyboardButton("specialcommandforgetlogfile")
    },
        new[]
    {
        new KeyboardButton("specialcommandforcheckyourprofile")
    },
        new[]
    {
        new KeyboardButton("Назад ⬅")
    }
     });
        tgButton.ResizeKeyboard = true;
        return tgButton;
    }
}
