using Microsoft.Extensions.Configuration;
using NLog;
using ScheduleClassBot.Internal;
using System.Net.Http.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.Processors;

internal static class SpecialCommands
{
    // ReSharper disable once InconsistentNaming
    private static readonly IConfiguration configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();

    // ReSharper disable once InconsistentNaming
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    // ReSharper disable once InconsistentNaming
    private static readonly string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    private static DateTime _dateTime;

    // ReSharper disable once InconsistentNaming
    private static readonly string? apiKey = configuration.GetSection("OpenAI:ChatGPTKey").Value;

    // ReSharper disable once InconsistentNaming
    private static readonly string endpoint = "https://api.openai.com/v1/chat/completions";

    // ReSharper disable once InconsistentNaming
    private static readonly List<GptResponse.Message> messages = new();

    // ReSharper disable once InconsistentNaming
    private static string? gptMessage { get; set; }

    // ReSharper disable once InconsistentNaming
    internal static ulong countMessage { get; set; }

    // ReSharper disable once InconsistentNaming
    private static string? pathOnProject { get; set; }

    // ReSharper disable once InconsistentNaming
    private static string? path { get; set; }

    // ReSharper disable once InconsistentNaming
    private static string? logdate { get; set; }

    public static async Task Back(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                "Держи список специальных функций бота!", replyMarkup: SpecialBotButton.SpecialCommandInlineButton(),
                cancellationToken: cancellationToken);

            _logger.Info("!!!SPECIAL COMMAND!!! Back success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error back. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    public static async Task GetUsersList(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery!.Message!.Chat.Id;
            string combinePath = Path.Combine(projectPath, "ListUsers.txt");

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
                    replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! View users list success!");
            }
            else
            {
                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Пользователей нет!",
                    replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! Error view users list success!");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error view users list. {method}: {error}", nameof(GetUsersList), ex);
        }
    }

    public static async Task GetCountMessage(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                $"Количество написанных сообщений боту: {countMessage}!" +
                $"\nКоличество отправленных подарков: {countMessage / 150}!",
                replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            _logger.Info($"!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage),
                ex);
        }
    }

    public static async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            _dateTime = DateTime.Now;
            string month = _dateTime.Month <= 9 ? $"0{_dateTime.Month}" : $"{_dateTime.Month}";
            string day = _dateTime.Day <= 9 ? $"0{_dateTime.Day}" : $"{_dateTime.Day}";

            if (!(message.Text! == "specialcommandforgetlogfile"))
            {
                int index = message.Text!.IndexOf(":", StringComparison.Ordinal);
                if (index != -1)
                    logdate = message.Text!.Substring(index + 1).Trim();

                pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(pathOnProject, $"log/{logdate}");

                if (System.IO.File.Exists(path))
                {
                    await using FileStream fileStream = new FileStream(path, FileMode.Open);
                    InputFileStream inputFile = new InputFileStream(fileStream, logdate);
                    await botClient.SendDocumentAsync(message.Chat, inputFile,
                        caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!", cancellationToken: cancellationToken);
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat,
                        $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!\n" +
                        $"\nПример команды на сегодня:\n" +
                        $"```\nspecialcommandforgetlogfile:{_dateTime.Year}-{month}-{day}.log\n```",
                        parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:" +
                    $"\n```\nspecialcommandforgetlogfile:yyyy-mm-dd.log\n```" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{_dateTime.Year}-{month}-{day}.log\n```\n" +
                    $"Нажми, чтобы скопировать :)", parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);

            _logger.Info($"!!!SPECIAL COMMAND!!! Get log file success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error get log file. {method}: {error}", nameof(GetLogFile), ex);
        }
    }

    public static async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat,
                "Держи список специальных функций бота!",
                replyMarkup: SpecialBotButton.SpecialCommandInlineButton(), cancellationToken: cancellationToken);
            _logger.Info("!!!SPECIAL COMMAND!!! Get button with all special commands success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error button with all special commands. {method}: {error}",
                nameof(GetButtonWithSpecialCommands), ex);
        }
    }

    public static async Task GetInfoYourProfile(ITelegramBotClient botClient, Update update, Message message,
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
            _logger.Info("!!!SPECIAL COMMAND!!! Get your info profile success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error your info profile. {method}: {error}",
                nameof(GetInfoYourProfile), ex);
        }
    }

    public static async Task GetQuestionsFromChatGpt(ITelegramBotClient botClient, Update update, Message message,
        CancellationToken cancellationToken)
    {
        int firstMessageId = message.MessageId;
        try
        {
            gptMessage = message.Text!;
            var firstMessage = await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, обрабатываю твой запрос...", parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
            firstMessageId = firstMessage.MessageId;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            GptResponse.Message mes = new GptResponse.Message()
            {
                Role = "user",
                Content = gptMessage
            };

            messages.Add(mes);

            GptResponse.Request requestData = new GptResponse.Request()
            {
                ModelId = "gpt-3.5-turbo",
                Messages = messages
            };
            using var response = await httpClient.PostAsJsonAsync(endpoint, requestData, cancellationToken: cancellationToken);
            GptResponse.ResponseData? responseData =
                await response.Content.ReadFromJsonAsync<GptResponse.ResponseData>(cancellationToken: cancellationToken);

            var choices = responseData?.Choices ?? new List<GptResponse.Choice>();
            var choice = choices[0];

            GptResponse.Message responseMessage = choice.Message;
            messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();
            _logger.Info($"ChatGPT: {responseText}");
            await botClient.EditMessageTextAsync(message.Chat, firstMessageId, $"Chat GPT: {responseText}",
                parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get response from Chat GPT success!");
        }
        catch (Exception ex)
        {
            await botClient.EditMessageTextAsync(message.Chat, firstMessageId,
                $"{update.Message?.From?.FirstName}, произошла ошибка, попробуй еще раз!",
                parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Error("!!!SPECIAL COMMAND!!! Error response from Chat GPT. {method}: {error}",
                nameof(GetQuestionsFromChatGpt), ex);
            messages.Clear();
        }
    }
}