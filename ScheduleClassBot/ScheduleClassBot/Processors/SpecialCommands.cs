using Microsoft.Extensions.Configuration;
using NLog;
using ScheduleClassBot.Internal;
using System.Net.Http.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.Processors;
internal class SpecialCommands
{
    private static IConfiguration configuration = new ConfigurationBuilder()
       .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
       .AddJsonFile("appsettings.json")
       .Build();

    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private static string projectPath = AppDomain.CurrentDomain.BaseDirectory;
    private static DateTime dateTime;

    private static string apiKey = "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";
    private static string endpoint = "https://api.openai.com/v1/chat/completions";
    private static List<GPTResponse.Message> messages = new List<GPTResponse.Message>();
    private static string? gptMessage { get; set; }

    private static long[]? idUser = configuration.GetSection("UserID:IdUser").Get<long[]>();

    internal static ulong countMessage { get; set; }
    internal static string? pathOnProject { get; set; }
    internal static string? path { get; set; }
    internal static string? logdate { get; set; }

    public static async Task Back(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Держи список специальных функций бота!", replyMarkup: SpecialBotButton.SpecialCommandInlineButton(), cancellationToken: cancellationToken);

            _logger.Info($"!!!SPECIAL COMMAND!!! Back success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error back. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    public static async Task GetUsersList(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var callbackQuery = update.CallbackQuery;
            var chatId = callbackQuery!.Message!.Chat.Id;
            string fileContent = "", path = Path.Combine(projectPath, "ListUsers.txt");

            if (System.IO.File.Exists(path))
            {
                fileContent = System.IO.File.ReadAllText(path);
                StringBuilder responseBuilder = new StringBuilder();

                foreach (string line in fileContent.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        responseBuilder.AppendLine($"User Info: {line.Trim()}");
                }

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Держи список пользователей:\n{fileContent}", replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! View users list success!");
            }
            else
            {
                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Пользователей нет!", replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);
                _logger.Info($"!!!SPECIAL COMMAND!!! Error view users list success!");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error view users list. {method}: {error}", nameof(GetUsersList), ex);
        }
    }

    public static async Task GetCountMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Количество написанных сообщений боту: {countMessage}!" +
                                $"\nКоличество отправленных подарков: {countMessage / 150}!", replyMarkup: SpecialBotButton.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            _logger.Info($"!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage), ex);
        }
    }

    public static async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            dateTime = DateTime.Now;
            string month = dateTime.Month <= 9 ? $"0{dateTime.Month}" : $"{dateTime.Month}";
            string day = dateTime.Day <= 9 ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            if (!(message.Text! == "specialcommandforgetlogfile"))
            {
                int index = message.Text!.IndexOf(":");
                if (index != -1)
                    logdate = message.Text!.Substring(index + 1).Trim();

                pathOnProject = AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(pathOnProject, $"log/{logdate}");

                if (System.IO.File.Exists(path))
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        InputFileStream inputFile = new InputFileStream(fileStream, logdate);
                        await botClient.SendDocumentAsync(message.Chat, inputFile, caption: $"{update.Message?.From?.FirstName}, держи логи за выбранный день!");
                    }
                }
                else
                    await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, данного файла не обнаружено! Проверь корректность введенной даты!\n" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            else
                await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, команда выглядит следующим образом:" +
                    $"\n```\nspecialcommandforgetlogfile:yyyy-mm-dd.log\n```" +
                    $"\nПример команды на сегодня:\n" +
                    $"```\nspecialcommandforgetlogfile:{dateTime.Year}-{month}-{day}.log\n```\n" +
                    $"Нажми, чтобы скопировать :)", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get log file success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error get log file. {method}: {error}", nameof(GetLogFile), ex);
        }
    }

    public static async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи список специальных функций бота!", replyMarkup: SpecialBotButton.SpecialCommandInlineButton(), cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get button with all special commands success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error button with all special commands. {method}: {error}", nameof(GetButtonWithSpecialCommands), ex);
        }
    }

    public static async Task GetInfoYourProfile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, держи информацию о аккаунте!\n\n" +
                $"Идентификатор пользователя: {message?.From?.Id}\n" +
                $"Имя пользователя: @{message?.From?.Username}\n" +
                $"Имя: {message?.From?.FirstName}\n" +
                $"Фамилия: {message?.From?.LastName}\n" +
                $"Язык: {message?.From?.LanguageCode}\n" +
                $"Информация о местоположении: {message?.Location}\n" +
                $"Контактные данные: {message?.Contact}\n" +
                $"Наличие Telegram премиум: {message?.From?.IsPremium}\n" +
                $"Бот: {message?.From?.IsBot}", cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get your info profile success!");
        }
        catch (Exception ex)
        {
            _logger.Error("!!!SPECIAL COMMAND!!! Error your info profile. {method}: {error}", nameof(GetInfoYourProfile), ex);
        }
    }

    public static async Task GetQuestionsFromChatGPT(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        int firstMessageId = message.MessageId;
        try
        {
            if (!(message.Text! == "Q") && idUser!.Any(x => x == message?.From?.Id))
            {
                int index = message!.Text!.IndexOf(":");
                if (index != -1)
                    gptMessage = message.Text!.Substring(index + 1).Trim();
                var firstMessage = await botClient.SendTextMessageAsync(message.Chat, $"{update.Message?.From?.FirstName}, обрабатываю твой запрос...", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                firstMessageId = firstMessage.MessageId;
            }
            else
            {
                var firstMessage = await botClient.SendTextMessageAsync(message!.Chat, $"{update.Message?.From?.FirstName}, извини, я не знаю как ответить на это!\nВозможно ты используешь старую команду, попробуй обновить бота, нажав сюда: /start!", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
                firstMessageId = firstMessage.MessageId;
                return;
            }

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            GPTResponse.Message mes = new GPTResponse.Message()
            {
                Role = "user",
                Content = gptMessage!
            };

            messages.Add(mes);

            GPTResponse.Request requestData = new GPTResponse.Request()
            {
                ModelId = "gpt-3.5-turbo",
                Messages = messages
            };
            using var response = await httpClient.PostAsJsonAsync(endpoint, requestData);

            GPTResponse.ResponseData? responseData = await response.Content.ReadFromJsonAsync<GPTResponse.ResponseData>();

            var choices = responseData?.Choices ?? new List<GPTResponse.Choice>();
            var choice = choices[0];

            GPTResponse.Message responseMessage = choice.Message;
            messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();
            _logger.Info($"ChatGPT: {responseText}");
            await botClient.EditMessageTextAsync(message.Chat,firstMessageId, $"Chat GPT: {responseText}", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Info($"!!!SPECIAL COMMAND!!! Get response from Chat GPT success!");
        }
        catch (Exception ex)
        {
            await botClient.EditMessageTextAsync(message.Chat, firstMessageId, $"{update.Message?.From?.FirstName}, произошла ошибка, попробуй еще раз!", parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            _logger.Error("!!!SPECIAL COMMAND!!! Error response from Chat GPT. {method}: {error}", nameof(GetQuestionsFromChatGPT), ex);
            messages.Clear();
        }
    }
}