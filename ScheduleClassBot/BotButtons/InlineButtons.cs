using ScheduleClassBot.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class InlineButtons
{
    /// <summary>
    /// Метод, отвечающий за кнопки like и dislike
    /// </summary>
    /// <returns></returns>
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