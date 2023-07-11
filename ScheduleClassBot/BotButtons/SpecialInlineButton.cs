using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class SpecialInlineButton
{
    public InlineKeyboardMarkup SpecialCommandInlineButton()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Просмотр списка пользователей",
                    callbackData: "specialcommandforviewlistusers")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Просмотр количества написанных сообщений",
                    callbackData: "specialcommandforviewcountmessages")
            },
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text: "Получение логов",
                    query: "specialcommandforgetlogfile")
            },
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text: "Просмотр информации о пользователе",
                    query: "specialcommandforcheckyourprofile")
            }
        });
        return inlineButton;
    }

    public InlineKeyboardMarkup SpecialBackInlineButton()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Назад ⬅", callbackData: "back")
            }
        });
        return inlineButton;
    }
}