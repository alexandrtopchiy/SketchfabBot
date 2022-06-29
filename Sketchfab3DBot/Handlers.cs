using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.InlineQueryResults;
//using Telegram.Bot.Types.InputFiles;
//using Telegram.Bot.Types.ReplyMarkups;
//using _3DAPI.Models;
//using SketchfabAPI.Controllers;
//using SketchfabAPI.Models;
//using SketchfabAPI.Services;
//using SketchfabAPI.Constans;
//using Telegram.Bot.Extensions.Polling;

public class PostLikeRequest
{
    public string uid { get; set; }
   
    public PostLikeRequest(string data)
    {
        uid = data;
    }
};



























//namespace Telegram.Bot.Examples.Polling
//{

//    public class Handlers
//    {
//        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
//        {
//            var ErrorMessage = exception switch
//            {
//                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
//                _ => exception.ToString()
//            };

//            Console.WriteLine(ErrorMessage);
//            return Task.CompletedTask;
//        }

//        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//        {
//            var handler = update.Type switch
//            {
//                // UpdateType.Unknown:
//                // UpdateType.ChannelPost:
//                // UpdateType.EditedChannelPost:
//                // UpdateType.ShippingQuery:
//                // UpdateType.PreCheckoutQuery:
//                // UpdateType.Poll:
//                UpdateType.Message => BotOnMessageReceived(botClient, update, update.Message!),
//              //  UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
//             //   UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
//               // UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
//             //   UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
//                _ => UnknownUpdateHandlerAsync(botClient, update)
//            };

//            try
//            {
//                await handler;
//            }
//            catch (Exception exception)
//            {
//                await HandleErrorAsync(botClient, exception, cancellationToken);
//            }
//        }

//        private static async Task BotOnMessageReceived(ITelegramBotClient botClient,Update update, Message message)
//        {          
//            if (message.Text == "/start")
//            {
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Hello, it's  a Sketchfab3DBot!\n At first, you need to authorize your account " +
//                    "so use \"/authorize\" command and follow the instructions");
//            }
//            if (message.Text == "/authorize")
//            {
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter token here");
//                string token;

//                if (update.Type == UpdateType.Message)
//                {
//                    if (update.Message.Type == MessageType.Text)
//                    {

//                        token = update.Message.Text;
//                        Console.WriteLine(token);
//                        //await botClient.SendTextMessageAsync(message.Chat.Id, $"Your token: {token}")}
//                    }
//                }
//               // Thread.Sleep(3000);

//                //await botClient.SendTextMessageAsync(message.Chat.Id, $"Your token: {token}");

//            }

//            if (message.Text == "/getmymodels")
//            {
//                GetMeModels res = new ModelController().GetMyModels(false,false);
//                for (int i = 0; i < res.results.Count; i++)
//                {
//                    await botClient.SendPhotoAsync(message.Chat.Id, res.results[i].thumbnails.images[0].url, $"{res.results[i].name}");

//                }

//                return;
//            }

//            //Console.WriteLine($"Receive message type: {message.Type}");
//            if (message.Type != MessageType.Text)
//                return;



// #region Îñòàëüíîé ôóíêöèîíàë áîòà ïîêà çàêîììåíòèðîâàë
/*
var action = message.Text!.Split('@')[0] switch
{
    "/inline" => SendInlineKeyboard(botClient, message),
    "/keyboard" => SendReplyKeyboard(botClient, message),
    "/remove" => RemoveKeyboard(botClient, message),
    "/photo" => SendFile(botClient, message),
    "/request" => RequestContactAndLocation(botClient, message),
    _ => Usage(botClient, message)
};
Message sentMessage = await action;
Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

// Send inline keyboard
// You can process responses in BotOnCallbackQueryReceived handler
static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
{
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

    // Simulate longer running task
    await Task.Delay(500);

    InlineKeyboardMarkup inlineKeyboard = new(
        new[]
        {
        // first row
        new []
        {
            InlineKeyboardButton.WithCallbackData("1.1", "11"),
            InlineKeyboardButton.WithCallbackData("1.2", "12"),
        },
        // second row
        new []
        {
            InlineKeyboardButton.WithCallbackData("2.1", "21"),
            InlineKeyboardButton.WithCallbackData("2.2", "22"),
        },
        });

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: "Choose",
                                                replyMarkup: inlineKeyboard);
}

static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
{
    ReplyKeyboardMarkup replyKeyboardMarkup = new(
        new[]
        {
            new KeyboardButton[] { "1.1", "1.2" },
            new KeyboardButton[] { "2.1", "2.2" },
        })
    {
        ResizeKeyboard = true
    };

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: "Choose",
                                                replyMarkup: replyKeyboardMarkup);
}

static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
{
    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: "Removing keyboard",
                                                replyMarkup: new ReplyKeyboardRemove());
}

static async Task<Message> SendFile(ITelegramBotClient botClient, Message message)
{
    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

    const string filePath = @"Files/tux.png";
    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

    return await botClient.SendPhotoAsync(chatId: message.Chat.Id,
                                          photo: new InputOnlineFile(fileStream, fileName),
                                          caption: "Nice Picture");
}

static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
{
    ReplyKeyboardMarkup RequestReplyKeyboard = new(
        new[]
        {
        KeyboardButton.WithRequestLocation("Location"),
        KeyboardButton.WithRequestContact("Contact"),
        });

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: "Who or Where are you?",
                                                replyMarkup: RequestReplyKeyboard);
}

static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
{
    const string usage = "Usage:\n" +
                         "/inline   - send inline keyboard\n" +
                         "/keyboard - send custom keyboard\n" +
                         "/remove   - remove custom keyboard\n" +
                         "/photo    - send a photo\n" +
                         "/request  - request location or contact";

    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                text: usage,
                                                replyMarkup: new ReplyKeyboardRemove());
}*/
//    #endregion
//}

// Process Inline Keyboard callback data
//        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
//        {
//            await botClient.AnswerCallbackQueryAsync(
//                callbackQueryId: callbackQuery.Id,
//                text: $"Received {callbackQuery.Data}");

//            await botClient.SendTextMessageAsync(
//                chatId: callbackQuery.Message.Chat.Id,
//                text: $"Received {callbackQuery.Data}");
//        }

//        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
//        {
//            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

//            InlineQueryResult[] results = {
//            // displayed result
//            new InlineQueryResultArticle(
//                id: "3",
//                title: "TgBots",
//                inputMessageContent: new InputTextMessageContent(
//                    "hello"
//                )
//            )
//        };

//            await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
//                                                   results: results,
//                                                   isPersonal: true,
//                                                   cacheTime: 0);
//        }

//        private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
//        {
//            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
//            return Task.CompletedTask;
//        }

//        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
//        {
//            Console.WriteLine($"Unknown update type: {update.Type}");
//            return Task.CompletedTask;
//        }
//    }
//}