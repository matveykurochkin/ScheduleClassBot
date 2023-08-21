using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using NLog;
using ScheduleClassBot.BotButtons;
using ScheduleClassBot.Configuration;
using ScheduleClassBot.Constants;
using ScheduleClassBot.Interfaces;
using ScheduleClassBot.Responses;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ScheduleClassBot.ProcessingMethods;

internal class GettingSpecialCommands : ICheckMessage
{
    private readonly BotSettingsConfiguration _configuration;

    public GettingSpecialCommands(BotSettingsConfiguration configuration)
    {
        _configuration = configuration;
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    internal static ulong CountMessage;
    
    /// <summary>
    /// Метод, который безопасно в многопоточной среде считает количество сообщений отправленных боту
    /// </summary>
    public static void IncrementCountMessage()
    {
        Interlocked.Increment(ref CountMessage);
    }

    /// <summary>
    /// Метод, сравнивающий полученный текст с необходимым, без учета регистра
    /// </summary>
    /// <param name="receivedText">полученный на вход текст</param>
    /// <param name="necessaryText">необходимый текст</param>
    /// <returns>true, если полученный на вход текст = необходимому тексу, false - во всех остальных случаях</returns>
    public bool CheckingMessageText(string receivedText, string necessaryText)
    {
        return string.Equals(receivedText, necessaryText, StringComparison.OrdinalIgnoreCase);
    }

    private readonly SpecialInlineButtons _specialInlineButtons = new();

    /// <summary>
    /// Метод который возвращается назад к списку доступных команд с помощью метода SpecialCommandInlineButton
    /// (доступен огранниченному количеству пользователей)
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task BackInSpecialCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

    /// <summary>
    /// Метод, позволяющий получить список пользователей, пользовавщихся ботом
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
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

    internal static string? LastUser { get; set; }

    /// <summary>
    /// Метод, позволяющий получить количество отправленных ботом подарков,
    /// количество написанных сообщений, и юзернейм последнего написавшего человека боту
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetCountMessage(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var callbackQuery = update.CallbackQuery;
        var chatId = callbackQuery!.Message!.Chat.Id;
        try
        {
            await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId,
                $"Количество написанных сообщений боту: {CountMessage}!" +
                $"\nКоличество отправленных подарков: {CountMessage / 150}!" +
                $"\nПоследний написавший человек боту: {LastUser}!",
                replyMarkup: _specialInlineButtons.SpecialBackInlineButton(), cancellationToken: cancellationToken);

            Logger.Info("!!!SPECIAL COMMAND!!! View count message success!");
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error view count message. {method}: {error}", nameof(GetCountMessage),
                ex);
        }
    }

    /// <summary>
    /// Метод, позволяющий получить файлы ведения журнала ботом
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetLogFile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            var dateTime = DateTime.Now;
            string? logDate = default;
            string month = dateTime.Month <= 9 ? $"0{dateTime.Month}" : $"{dateTime.Month}";
            string day = dateTime.Day <= 9 ? $"0{dateTime.Day}" : $"{dateTime.Day}";

            if (!CheckingMessageText(message.Text!, "specialcommandforgetlogfile"))
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

    /// <summary>
    /// Метод, позволяющий получить меню со списком специальных команд бота
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetButtonWithSpecialCommands(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
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

    /// <summary>
    /// Метод, позволяющий получить информацию о текущем пользователе
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetInfoYourProfile(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
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

    private const string EndPoint = "https://api.openai.com/v1/chat/completions";
    private static readonly List<GptResponse.Message> Messages = new();
    private string? GptMessage { get; set; }

    /// <summary>
    /// Метод, обрабатывающий запрос адресованный Chat GPT и отправляет пользователю ответ
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetAnswersFromChatGpt(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
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
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.OpenAi!.ChatGptKey}");

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
            Logger.Error("!!!SPECIAL COMMAND!!! Error response from Chat GPT. {method}: {error}", nameof(GetAnswersFromChatGpt), ex);
            Messages.Clear();
        }
    }

    /// <summary>
    /// Метод, позволяющий конвертировать ошибочное написание текста на английском/русском языке
    /// пример: ghbdtn -> привет, рш -> hi
    /// </summary>
    /// <param name="botClient"></param>
    /// <param name="update"></param>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    public async Task GetFixKeyboardLayout(ITelegramBotClient botClient, Update update, Message message, CancellationToken cancellationToken)
    {
        try
        {
            //Обращение к API Layout Keyboard Converting Service
            var apiUrl = $"http://79.137.198.66:9060/FixMessage?message={message.Text![BotConstants.SpecialCommandForFixKeyboardLayout.Length..]}";

            using var client = new HttpClient();
            var response = await client.GetAsync(apiUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                Logger.Info("!!!SPECIAL COMMAND!!! Method {method} complite success!", nameof(GetFixKeyboardLayout));
                
                await botClient.SendTextMessageAsync(message.Chat,
                    $"{update.Message?.From?.FirstName}, держи конвертированный текст:\n" +
                    $"\n```\n{JsonSerializer.Deserialize<string>(responseBody)}\n```\n" +
                    $"Нажми чтобы скопировать!", parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.SendTextMessageAsync(message.Chat,
                $"{update.Message?.From?.FirstName}, произошла ошибка конвертирования:" +
                $"\nstatus code: {response.StatusCode}," +
                $"\nerror message: {response.Content}",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error("!!!SPECIAL COMMAND!!! Error Fix Keyboard Layout. {method}: {error}", nameof(GetFixKeyboardLayout), ex);
        }
    }
}