using CsvHelper;
using DataUtils;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramTools
{
    public class TelegramManager
    {
        private readonly TelegramBotClient _client;
        private const string Token = "7193907178:AAEymT6ucM5VJsT95Uxfr1Vl-KcQlPtXCrE";

        public TelegramManager()
        {
            _client = new TelegramBotClient(Token);
        }

        public void StartBot()
        {
            using CancellationTokenSource cts = new();
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            //_logger.LogInformation("Начало работы бота.");
            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Console.ReadLine();
            cts.Cancel();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            var usersInfo = UsersInfo.GetInstance();
            if (update.Message is { } message)
            {
                var chatId = message.Chat.Id;
                var state = usersInfo.GetState(chatId);

                //_logger.LogInformation($"Запись в файл стейта пользователя с id {chatId}");

                if (message.Text is { } messageText)
                {
                    //_logger.LogInformation($"Получено сообщение от пользователя с id {chatId}. Текст: {messageText}");
                    await HandleUserMessage(client, chatId, cancellationToken, messageText, state);
                }
                else if (message.Document is { } messageDocument)
                {
                    //_logger.LogInformation($"Получено документ от пользователя с id {chatId}. " +
                    //                       $"Файл: {messageDocument.FileName}");
                    await HandleUserDocument(client, chatId, cancellationToken, messageDocument, state);
                }
                else
                {
                    await client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Получено сообщение, содержащее неизвестный объект, следуйте интсрукциям, которые были выше, или перезапустите бота, с помощью /start.",
                        cancellationToken: cancellationToken);
                }
            }

            else if (update.CallbackQuery is { } callbackQuery)
            {
                var chatId = callbackQuery.Message!.Chat.Id;
                var fileId = UsersInfo.GetInstance().GetFileId(chatId);
                var state = UsersInfo.GetInstance().GetState(chatId);
                //_logger.LogInformation($"Получен callback от пользователя с id {chatId}. Callback data: {callbackQuery.Data}");

                var fileInfo = fileId is null ? null : await client.GetFileAsync(fileId, cancellationToken);
                var filePath = fileInfo?.FilePath ?? "";
                var fileExtension = Path.GetExtension(filePath);
                if (fileInfo is null)
                {
                    state.CurrentState = State.GetFile;
                    await client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Время хранения файла истекло(\n Пожалуйста отправьте новый файл.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }
                else
                {
                    var stations = new List<MetroStation>();
                    using (var stream = new MemoryStream())
                    {
                        await client.DownloadFileAsync(filePath, stream);
                        stream.Position = 0;
                        try
                        {
                            if (fileExtension == ".csv")
                            {
                                stations = await CSVProcessing.Read(stream);
                            }
                            else
                            {
                                stations = await JSONProcessing.Read(stream);
                            }
                        }
                        catch (Exception ex) when (ex is CsvHelperException || ex is JsonException)
                        {
                            state.CurrentState = State.GetFile;

                            await client.SendTextMessageAsync(
                            chatId: chatId,
                            text: "При обработке файла произошла ошибка, пожалуйста отправьте новый файл.",
                            replyMarkup: new ReplyKeyboardRemove(),
                            cancellationToken: cancellationToken);
                        }
                    }

                    // смотрим что нажал пользователь.
                    switch (callbackQuery.Data)
                    {
                        case "filter_line":
                            Console.WriteLine("something");
                            break;
                        default:
                            break;

                    }
                }
            }
            else
            {
                // logger получен неизвестный объект
            }
        }

        private async Task HandleUserMessage(ITelegramBotClient client, long chatId, CancellationToken cancellationToken, string messageText, StateGroup state)
        {
            switch (messageText)
            {
                case "/start":
                    //_logger.LogInformation($"Получена команда /start от пользователя с id: {chatId}");
                    await HandleCommandStart(client, chatId, cancellationToken, state);
                    break;
                //  switch-case для удобства добавления последующих команд.
                default:
                    switch (state.CurrentState)
                    {
                        case State.GetFile:
                            await AskFile(client, chatId, cancellationToken);
                            break;
                        case State.FileActionChoose:
                            await HandleGetFileAction(client, chatId, messageText, cancellationToken, state);
                            break;
                        default:
                            await HandleCommandStart(client, chatId, cancellationToken, state);
                            break;
                    }
                    break;
            }
        }
        private async Task HandleUserDocument(ITelegramBotClient client, long chatId, CancellationToken cancellationToken, Telegram.Bot.Types.Document document, StateGroup state)
        {
            //_logger.LogInformation($"Начата обработка документа для пользователя с id: {chatId}");
            var fileInfo = await client.GetFileAsync(document.FileId, cancellationToken);
            var filePath = fileInfo.FilePath;
            var fileExtension = Path.GetExtension(filePath);

            if (state.CurrentState == State.GetFile)
            {
                if (filePath is null)
                {
                    await client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "У программы нет доступа к этому файлу, попробуйте еще раз.",
                        cancellationToken: cancellationToken
                        );
                }
                else if (fileExtension == ".csv" || fileExtension == ".json")
                {
                    using (var stream = new MemoryStream())
                    {
                        await client.DownloadFileAsync(filePath, stream);
                        stream.Position = 0;
                        try
                        {
                            if (fileExtension == ".csv")
                            {
                                await CSVProcessing.Read(stream);
                            }
                            else
                            {
                                await JSONProcessing.Read(stream);
                            }

                            state.CurrentState = State.FileActionChoose;

                            var markup = new ReplyKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    new KeyboardButton("Отфильтровать"),
                                    new KeyboardButton("Отсортировать")
                                },
                            })
                            {
                                ResizeKeyboard = true
                            };

                            await client.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Файл успешно прочитан, теперь выберите действие с файлом:",
                                replyMarkup: markup,
                                cancellationToken: cancellationToken);

                            UsersInfo.GetInstance().SetFileId(chatId, fileInfo.FileId);
                        }
                        catch (CsvHelperException ex)
                        {
                            await client.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"В файле содержатся некорректные данные: номер строки *{ex.Context.Parser.Row}*, попробуйте еще раз.",
                                parseMode: ParseMode.Markdown,
                                cancellationToken: cancellationToken);
                        }
                        catch (JsonException ex)
                        {
                            await client.SendTextMessageAsync(
                                chatId: chatId,
                                text: $"В файле содержатся некорректные данные, попробуйте еще раз.\n\n {ex.Message}",
                                cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await client.SendTextMessageAsync(
                                chatId: chatId,
                                text: "Во время чтения файла возникла непредвиденная ошибка, попробуйте еще раз.",
                                cancellationToken: cancellationToken);
                        }
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Неправильное расшинерие файла, пожалуйста отправьте *csv* или *json* файл.",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                await client.SendTextMessageAsync(
                    chatId: chatId,
                    text: "На этом этапе не нужно отправлять файл, попробуйте что-нибудь другое.",
                    cancellationToken: cancellationToken);
            }
        }

        private async Task HandleCommandStart(ITelegramBotClient client, long chatId, CancellationToken cancellationToken, StateGroup state)
        {
            //_logger.LogInformation($"Обработка команды /start пользователя с id: {chatId}");
            state.CurrentState = State.GetFile;
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: "Привет!\n\nЯ помогу Вам удобно просматривать информацию о станциях метро, используя сортировку и фильтрацию данных.\n\n" +
                      "Просто отправьте *csv* или *json* файл.",
                parseMode: ParseMode.Markdown,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
        private async Task AskFile(ITelegramBotClient client, long chatId, CancellationToken cancellationToken)
        {
            await client.SendTextMessageAsync(
                chatId: chatId,
                text: "Пожалуйста отправьте *csv* или *json* файл.",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);
        }

        private async Task HandleGetFileAction(ITelegramBotClient client, long chatId, string messageText, CancellationToken cancellationToken, StateGroup state)
        {
            switch (messageText)
            {
                case "Отфильтровать":
                    {
                        var markup = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "Station", callbackData: "filter_station"),
                                InlineKeyboardButton.WithCallbackData(text: "RailwayLine", callbackData: "filter_line")
                            },
                            new[] { InlineKeyboardButton.WithCallbackData(text: "Station и RailwayLine", callbackData: "filter_both") }
                        });

                        await client.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Выберите поле для фильтрации:",
                            replyMarkup: markup,
                            cancellationToken: cancellationToken);
                        break;
                    }
                case "Отсортировать":
                    {
                        var markup = new InlineKeyboardMarkup(new[]
                        {
                            new[] { InlineKeyboardButton.WithCallbackData(text: "ID по возрастанию", callbackData: "sort_ID")},
                            new[] { InlineKeyboardButton.WithCallbackData(text: "По возрастанию времени открытия", callbackData: "sort_time") }
                        });

                        await client.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Выберите опцию для сортировки:",
                            replyMarkup: markup,
                            cancellationToken: cancellationToken);
                        break;
                    }
                default:
                    {
                        var markup = new ReplyKeyboardMarkup(new[]
                                {
                                new[]
                                {
                                    new KeyboardButton("Отфильтровать"),
                                    new KeyboardButton("Отсортировать")
                                },
                            })
                        {
                            ResizeKeyboard = true
                        };

                        await client.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Неизвестное действие, пожалуйста выберите из предложенных ниже:",
                            replyMarkup: markup,
                            cancellationToken: cancellationToken);
                        break;
                    }
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            //_logger.LogCritical(errorMessage);

            return Task.CompletedTask;
        }
    }
}