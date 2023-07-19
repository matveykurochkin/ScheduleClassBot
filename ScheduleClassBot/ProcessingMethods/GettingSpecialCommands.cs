using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using NLog;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.Responses;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSpecialCommands
{
    private static readonly IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    internal static ulong CountMessage { get; set; }

    private readonly SpecialInlineButtons _specialInlineButtons = new();

    public async Task BackInSpecialCommands(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                "Держи список специальных функций бота!",
                replyMarkup: _specialInlineButtons.SpecialCommandInlineButton(),
                cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! Back success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error back. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    public async Task GetUsersList(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery!.Message!.Chat.Id;
            string combinePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ListUsers.txt");

            if (System.IO.File.Exists(combinePath))
            {
                var fileContent = await System.IO.File.ReadAllTextAsync(combinePath, cancellationToken);
                StringBuilder responseBuilder = new StringBuilder();

                foreach (string line in fileContent.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        responseBuilder.AppendLine($"User Info: {line.Trim()}");
                }

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                    $"Держи список пользователей:\n{fileContent}",
                    replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                Logger.Info("!!!SPECIAL COMMAND!!! View users list success!");
            }
            else
            {
                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Пользователей нет!",
                    replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                Logger.Info("!!!SPECIAL COMMAND!!! Error view users list success!");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view users list. {method}: {error}", nameof(GetUsersList), ex);
        }
    }

    public async Task GetCountMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                $"Количество написанных сообщений боту: {CountMessage}!" +
                $"\nКоличество отправленных подарков: {CountMessage / 150}!",
                replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage),
                ex);
        }
    }

    public async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            var dateTime = DateTime.Now;
            string? logDate = default;
            string month = dateTime.Month <= 9 ? $"0{dateTime.Month}" : $"{dateTime.Month}";
            string day = dateTime.Day <= 9 ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            if (!(message.Text! == "specialcommandforgetlogfile"))
            {
                int index = message.Text!.IndexOf(":", StringComparison.Ordinal);
                if (index != -1)
                    logDate = message.Text!.Substring(index + 1).Trim();

                var pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(pathOnProject,
                    $"log{Path.DirectorySeparatorChar}{logDate}");

                if (System.IO.File.Exists(path))
                {
                    await using FileStream fileStream = new FileStream(path, FileMode.Open);
                    InputFileStream inputFile = new InputFileStream(fileStream, logDate);
                    await botClient.SendDocumentAsync(message.Chat, inputFile,
                        caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!",
                        cancellationToken: cancellationToken);
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!\n" +
                        $"\nПример команды на сегодня:\n" +
                        $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```",
                        parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:" +
                    $"\n```\nspecialcommandforgetlogfile:yyyy-MM-dd.log\n```" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```\n" +
                    $"Нажми, чтобы скопировать :)", parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! Get log file success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error get log file. {method}: {error}", nameof(GetLogFile), ex);
        }
    }

    public async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat,
                "Держи список специальных функций бота!",
                replyMarkup: _specialInlineButtons.SpecialCommandInlineButton(), cancellationToken: cancellationToken);
            Logger.Info("!!!SPECIAL COMMAND!!! Get button with all special commands success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error button with all special commands. {method}: {error}",
                nameof(GetButtonWithSpecialCommands), ex);
        }
    }

    public async Task GetInfoYourProfile(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, держи информацию о аккаунте!\n\n" +
                $"Идентификатор пользователя: {message.From?.Id}\n" +
                $"Имя пользователя: @{message.From?.Username}\n" +
                $"Имя: {message.From?.FirstName}\n" +
                $"Фамилия: {message.From?.LastName}\n" +
                $"Язык: {message.From?.LanguageCode}\n" +
                $"Информация о местоположении: {message.Location}\n" +
                $"Контактные данные: {message.Contact}\n" +
                $"Наличие Telegram премиум: {message.From?.IsPremium}\n" +
                $"Бот: {message.From?.IsBot}", cancellationToken: cancellationToken);
            Logger.Info("!!!SPECIAL COMMAND!!! Get your info profile success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error your info profile. {method}: {error}",
                nameof(GetInfoYourProfile), ex);
        }
    }

    private static readonly string? ApiKey = Configuration.GetSection("OpenAI:ChatGPTKey").Value;
    private const string EndPoint = "https://api.openai.com/v1/chat/completions";
    private static readonly List<GptResponse.Message> Messages = new();
    private string? GptMessage { get; set; }

    public async Task GetAnswersFromChatGpt(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        var currentMessageId = message.MessageId;
        try
        {
            GptMessage = message.Text!;
            var firstMessage = await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, обрабатываю твой запрос...", parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            currentMessageId = firstMessage.MessageId;

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            GptResponse.Message mes = new GptResponse.Message
            {
                Role = "user",
                Content = GptMessage
            };

            Messages.Add(mes);

            GptResponse.Request requestData = new GptResponse.Request
            {
                ModelId = "gpt-3.5-turbo",
                Messages = Messages
            };
            using var response =
                await httpClient.PostAsJsonAsync(EndPoint, requestData, cancellationToken: cancellationToken);
            GptResponse.ResponseData? responseData =
                await response.Content.ReadFromJsonAsync<GptResponse.ResponseData>(
                    cancellationToken: cancellationToken);

            var choices = responseData?.Choices ?? new List<GptResponse.Choice>();

            GptResponse.Message responseMessage = choices[0].Message;
            Messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();
            Logger.Info($"ChatGPT: {responseText}");
            await botClient.EditMessageTextAsync(message.Chat, currentMessageId, $"Chat GPT: {responseText}",
                parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            Logger.Info("!!!SPECIAL COMMAND!!! Get response from Chat GPT success!");
        }
        catch (Exception ex)
        {
            await botClient.EditMessageTextAsync(message.Chat, currentMessageId,
                $"{update.Message?.From?.FirstName}, произошла ошибка, попробуй еще раз!",
                cancellationToken: cancellationToken);
            Logger.Error("!!!SPECIAL COMMAND!!! Error response from Chat GPT. {method}: {error}",
                nameof(GetAnswersFromChatGpt), ex);
            Messages.Clear();
        }
    }
}