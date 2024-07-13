using ScheduleClassBot.Constants;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleClassBot.BotButtons;

internal class SpecialInlineButtons
{
    /// <summary>
    /// Метод, показывающий кнопки специальных команд доступен только для пользователей у которых есть к ним доступ
    /// </summary>
    /// <returns></returns>
    public InlineKeyboardMarkup SpecialCommandInlineButton()
    {
        var inlineButton = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Все пользователи", callbackData: BotConstants.SpecialCommandForViewListUsers)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Последний пользователь", callbackData: BotConstants.LastUser)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Статистика сообщений", callbackData: BotConstants.SpecialCommandForViewCountMessages)
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "Jenkins", callbackData: BotConstants.JenkinsLink)
            },
            new[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(text: "Получение логов", query: BotConstants.SpecialCommandForGetLogFile)
            }
        });
        return inlineButton;
    }

    /// <summary>
    /// Метод, отвечающий за возвращение к методу SpecialCommandInlineButton при нажатии на такие кнопки:
    /// "Просмотр списка пользователей" и "Просмотр количества написанных сообщений"
    /// </summary>
    /// <returns></returns>
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