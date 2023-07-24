using ScheduleClassBot.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class InlineButtons
{
    public InlineKeyboardMarkup InlineButtonOnBot()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "👍🏻", BotConstants.Like),
                InlineKeyboardButton.WithCallbackData(text: "👎🏻", BotConstants.DisLike)
            }
        });
        return inlineButton;
    }
}