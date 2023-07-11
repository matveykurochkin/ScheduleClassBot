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
                InlineKeyboardButton.WithCallbackData(text: "👍🏻", callbackData: "like"),
                InlineKeyboardButton.WithCallbackData(text: "👎🏻", callbackData: "dislike")
            }
        });
        return inlineButton;
    }
}