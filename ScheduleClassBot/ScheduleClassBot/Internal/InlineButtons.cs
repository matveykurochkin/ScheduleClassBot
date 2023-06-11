using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.Internal;

internal static class InlineButtons
{
    public static InlineKeyboardMarkup InlineButtonOnBot()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
        new []
        {
            InlineKeyboardButton.WithCallbackData(text: "👍🏻", callbackData: "like"),
            InlineKeyboardButton.WithCallbackData(text: "👎🏻", callbackData: "dislike")
        }
        });
        return inlineButton;
    }
}
