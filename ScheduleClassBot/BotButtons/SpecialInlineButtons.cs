﻿using ScheduleClassBot.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class SpecialInlineButtons
{
    public InlineKeyboardMarkup SpecialCommandInlineButton()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Просмотр списка пользователей", callbackData: BotConstants.SpecialCommandForViewListUsers)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Просмотр количества написанных сообщений", callbackData: BotConstants.SpecialCommandForViewCountMessages)
            },
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text: "Получение логов", query: BotConstants.SpecialCommandForGetLogFile)
            },
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text: "Просмотр информации о пользователе", query: BotConstants.SpecialCommandForCheckYourProfile)
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
                InlineKeyboardButton.WithCallbackData(text: BotConstants.CommandBack, callbackData: BotConstants.CommandBack)
            }
        });
        return inlineButton;
    }
}