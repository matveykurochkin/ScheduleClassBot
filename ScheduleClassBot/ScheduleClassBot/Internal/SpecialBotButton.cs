using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.Internal;

internal static class SpecialBotButton
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
        })
        {
            ResizeKeyboard = true
        };
        return tgButton;
    }

    public static InlineKeyboardMarkup SpecialCommandInlineButton()
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

    public static InlineKeyboardMarkup SpecialBackInlineButton()
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