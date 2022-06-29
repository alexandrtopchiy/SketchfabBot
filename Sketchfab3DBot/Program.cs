using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
//using Telegram.Bot.Examples.Polling;
using Telegram.Bot.Types.Enums;
using _3DAPI.Models;
using SketchfabAPI.Controllers;
using SketchfabAPI.Models;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using Newtonsoft.Json;


var botClient = new TelegramBotClient("5502147579:AAFpckRkhpU7r6IjQK1BTuT2lENlpFowHVg");
using var cts = new CancellationTokenSource();
using var httpClient = new HttpClient();
Dictionary<string, string> newProfileParameters = new Dictionary<string, string>();
Dictionary<string, string> newModelParameters = new Dictionary<string, string>();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = {
    }
};

botClient.StartReceiving(
    HandleUpdatesAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Начал прослушку @{me.Username}");
Console.ReadLine();

cts.Cancel();


async Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    {
        await HandleMessage(botClient, update.Message);
        return;
    }

    if (update.Type == UpdateType.CallbackQuery)
    {
        await HandleCallbackQuery(botClient, update.CallbackQuery);
        return;
    }
}


async Task HandleMessage(ITelegramBotClient botClient, Message message)
{
     
    if (message.Text == "/start")
    {
        ReplyKeyboardMarkup keyboard = new(new[]
        {
            new KeyboardButton[] { "Getmymodels", "Getmyprofile"},
            new KeyboardButton[] {"Search", "Authorize"}
        })
        {
            ResizeKeyboard = true
        };
    await botClient.SendTextMessageAsync(message.Chat.Id, "Hello, it's  a Sketchfab3DBot!\n First of all, you need to authorize your account " +
                    "so click \"Authorize\" and follow the instructions", replyMarkup: keyboard);
    return;
    }
    if (message.Text == "Authorize")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "You need to go there, make registration and insert token using \"token\" command.\n" +
            "For example: token/yourtoken ");
        return;
    }
    if (message.Text.ToLower().Contains("token/"))
    {
        message.Text = message.Text.Replace("token/", "");
        SetToken(message.Text);
        return;
    }

    if (message.Text == "Getmymodels")
    {
        // GetMeModels res = new ModelController().GetMyModels(false, false);
        var modelResult = await httpClient.GetAsync($"https://localhost:44364/Model/GetMyModels/false/false");
        var content = modelResult.Content.ReadAsStringAsync().Result;
        var res = JsonConvert.DeserializeObject<GetMeModels>(content);
        for (int i = 0; i < res.results.Count; i++)
        {
            InlineKeyboardMarkup inlineKeyboard = new(new[] {
                new[] {
                        InlineKeyboardButton.WithCallbackData("More information", callbackData: $"Infor{res.results[i].uid}"),
                        InlineKeyboardButton.WithCallbackData("Delete", callbackData: $"Delete{res.results[i].uid}"),
                      },
                new[]{
                    InlineKeyboardButton.WithCallbackData("Update information", callbackData: $"UPD{res.results[i].uid}"),
                     InlineKeyboardButton.WithUrl("Watch", res.results[i].viewerUrl ),
                      },
                      });
             await botClient.SendPhotoAsync(message.Chat.Id, res.results[i].thumbnails.images[0].url, $"{res.results[i].name}", replyMarkup: inlineKeyboard);
        }
        return;

    }
    if (message.Text == "Getmyprofile")
    { 
        var profileResult = await httpClient.GetAsync("https://localhost:44364/Profile/GetMyProfile");
        var content = profileResult.Content.ReadAsStringAsync().Result;
        var result = JsonConvert.DeserializeObject<ProfileModel>(content);
        InlineKeyboardMarkup inlineKeyboard = new(
                        InlineKeyboardButton.WithCallbackData("Update profile information", callbackData: "profileUpdate")
                     );
        await botClient.SendPhotoAsync(message.Chat.Id, $"{result?.avatar.iMages[0].url}", ProfileInformation(result), replyMarkup: inlineKeyboard);
        return;
    }
    if (message.Text == "Search")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "To find models you need to use command \"/find\"\n " +
            "Please, type your request right behind \"/\". For example:\n " +
            "/find/box");
        return;
    }
    if (message.Text.ToLower().Contains("/find"))
    {
        string  req = message.Text.Replace("/find/", "");
        await botClient.SendTextMessageAsync(message.Chat.Id, $"Your request is: {req}\n");
          ShowSearchResult(message, req);      
        return;
    }
   
    if(message.Text.ToLower().Contains("displayname/"))
    {
        message.Text = message.Text.Replace("displayname/", ""); // write some exceptions
        newProfileParameters.Add("_displayName", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("password/"))
    {
        message.Text = message.Text.Replace("password/", ""); // write some exceptions
        newProfileParameters.Add("_password", message.Text);
        await botClient.SendTextMessageAsync(message.Chat.Id, $"So, your new password is \"{message.Text}\".\n " +
            $"Now you need to confirm your password by using \"confirmpassword/\" command");
        return;
    }
    if (message.Text.ToLower().Contains("confirmpassword/"))
    {
        message.Text = message.Text.Replace("confirmpassword/", ""); // write some exceptions
        newProfileParameters.Add("_passwordConfirmation", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("facebookusername/"))
    {
        message.Text = message.Text.Replace("facebookusername/", ""); // write some exceptions
        newProfileParameters.Add("_facebookUsername", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("biography/"))
    {
        message.Text = message.Text.Replace("biography/", ""); // write some exceptions
        newProfileParameters.Add("_biography", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("tagline/"))
    {
        message.Text = message.Text.Replace("tagline/", ""); // write some exceptions
        newProfileParameters.Add("_tagline", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("website/"))
    {
        message.Text = message.Text.Replace("website/", ""); // write some exceptions
        newProfileParameters.Add("_website", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("country/"))
    {
        message.Text = message.Text.Replace("country/", ""); // write some exceptions
        newProfileParameters.Add("_country", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("city/"))
    {
        message.Text = message.Text.Replace("city/", ""); // write some exceptions
        newProfileParameters.Add("_city", message.Text);
        return;
    }
    if (message.Text==("newprofilesend"))
    {
        SendNewProfile(message);
        return;
    }
    //
    if (message.Text.ToLower().Contains("name/"))
    {
        message.Text = message.Text.Replace("name/", ""); // write some exceptions
        newModelParameters.Add("_name", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("description/"))
    {
        message.Text = message.Text.Replace("description/", ""); // write some exceptions
        newModelParameters.Add("_description", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("tags/"))
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Enter some tags using \",\"");
        message.Text = message.Text.Replace("tags/", ""); // write some exceptions
        newModelParameters.Add("_tags", message.Text);
        return;
    }
    if (message.Text.ToLower().Contains("category/"))
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Enter category");
        message.Text = message.Text.Replace("category/", ""); // write some exceptions
        newModelParameters.Add("_tags", message.Text);
        return;
    }
    if (message.Text==("newmodelsend"))
    {
        UpdateModelInformation(message);
        return;
    }
}

void SetToken(string text)
{
    throw new NotImplementedException();
}

async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
{

    if (callbackQuery.Data.StartsWith("Infor"))
    {
        callbackQuery.Data = callbackQuery.Data.Replace("Infor", "");
        await botClient.SendTextMessageAsync(
        callbackQuery.Message.Chat.Id,
        MoreInformation(callbackQuery.Data)
        );
        return;
    }
    
    if (callbackQuery.Data.StartsWith("Like"))
    {
        callbackQuery.Data = callbackQuery.Data.Replace("Like", "");        
        LikeModel(callbackQuery.Data, callbackQuery);
        return;
    }
    if (callbackQuery.Data.StartsWith("Delete"))
    {
        callbackQuery.Data = callbackQuery.Data.Replace("Delete", "");
        DeleteModel(callbackQuery.Data);
        return;
    }
    if (callbackQuery.Data.StartsWith("profileUpdate"))
    {
       await botClient.SendTextMessageAsync(
      callbackQuery.Message.Chat.Id,
      "There are few parametres: password, displayname, facebookusername, biography, tagline, website, country and city\n" +
      "If you want to make changes you need to enter parameter name, then slash and enter new value.\n" +
      "For example: displayname/newname\n"+
      "When you changed all the parametres you want, use \" newprofilesend\" command "
      );
        return;
    }
    if (callbackQuery.Data.StartsWith("UPD"))
    {
        callbackQuery.Data = callbackQuery.Data.Replace("UPD", "");
        await botClient.SendTextMessageAsync(
      callbackQuery.Message.Chat.Id,
      "There are few parametres: name, category, tags, description\n" +
      "If you want to make changes you need to enter parameter name, then slash and enter new value.\n" +
      "For example: displayname/newname\n" +
      "When you changed all the parametres you want, use \"newmodelsend\" command "
      );
        newModelParameters.Add("_uid", callbackQuery.Data);
        return;
    }


}

async void DeleteModel(string data)
{
    await httpClient.DeleteAsync($"https://localhost:44364/Model/DeleteModel/{data}");

}

async void UpdateModelInformation(Message message)
{
    string _uid;
    bool some= newModelParameters.TryGetValue("_uid", out _uid);
    newModelParameters.Remove("_uid");
    string reqUri = $"https://localhost:44364/Model/UpdateModelInfoByAsync/{_uid}?";

    //await botClient.SendTextMessageAsync(message.Chat.Id, _uid);

    List<string> Keys = new List<string>(newModelParameters.Keys);
    List<string> Values = new List<string>(newModelParameters.Values);

    for (int i = 0; i < newModelParameters.Count; i++)
    {
        if (Keys[i] != "_tags" && Keys[i] !="_categories") {
            reqUri = reqUri.Insert(reqUri.Length, Keys[i]);
            reqUri = reqUri.Insert(reqUri.Length, "=");
            reqUri = reqUri.Insert(reqUri.Length, Values[i]);
            reqUri = reqUri.Insert(reqUri.Length, "&");
        }
        if (Keys[i] == "_tags")
        {
            var tags = Values[i].Split(",");
            foreach (var item in tags)
            {
                reqUri += "_tags=" + item + "&";
            }
        }
        if (Keys[i] == "_categories")
        {
            var categories = Values[i].Split(",");
            foreach (var item in categories)
            {
                reqUri += "_categories=" + item + "&";
            }
           
        }
    }
    newModelParameters.Clear();
    Keys.Clear();
    Values.Clear();
    reqUri = reqUri.Substring(0, reqUri.Length - 1);
    UpdateModelInfo up = new UpdateModelInfo();


    var json = JsonConvert.SerializeObject(up);
    var param = new StringContent(json, Encoding.UTF8, "application/json");
    var responce = httpClient.PatchAsync(reqUri, param);
    await botClient.SendTextMessageAsync(message.Chat.Id, "Updated succesfully");

}

async void SendNewProfile(Message message)
{
    string reqUri = "https://localhost:44364/Profile/UpdateProfileByAsync?";
    List<string> Keys = new List<string>(newProfileParameters.Keys);
    List<string> Values = new List<string>(newProfileParameters.Values);

    for (int i = 0; i < newProfileParameters.Count; i++)
    {
        reqUri = reqUri.Insert(reqUri.Length, Keys[i]);
        reqUri = reqUri.Insert(reqUri.Length, "=");
        reqUri = reqUri.Insert(reqUri.Length, Values[i]);
        if (i != newProfileParameters.Count - 1) reqUri = reqUri.Insert(reqUri.Length, "&");

    }
    Keys.Clear();
    Values.Clear();
    newProfileParameters.Clear();
    UpdateProfileInfo upd = new UpdateProfileInfo() { };
    //{
    //    password = newProfileParameters.TryGetValue("_password", out value),
    //    passwordConfirmation = _passwordConfirmation,
    //    displayName = _displayName,
    //    facebookUsername = _facebookUsername,
    //    biography = _biography,
    //    tagline = _tagline,
    //    website = _website,
    //    city = _city,
    //    country = _country
    //};
    var json = JsonConvert.SerializeObject(upd);
    var param = new StringContent(json, Encoding.UTF8, "application/json");

  //  await botClient.SendTextMessageAsync(message.Chat.Id, reqUri);

    var responce = httpClient.PatchAsync(reqUri, param);
    await botClient.SendTextMessageAsync(message.Chat.Id, "Updated successfully");
    

}

string ProfileInformation(ProfileModel p)
{
    string result = $"Display name: {p.displayName}\n" +
        $"Email: {p.email}\n" +
        $"Account: {p.account}\n" +
        $"Biography: {p.biography}\n" +
        $"Models count: {p.modelCount}\n" +
        $"Subscriptions: {p.subscriptionCount}\n" +
        $"Followers count: {p.followerCount}\n" +
        $"Tagline: {p.tagline}\n" +
        $"Website: {p.website}\n" +
        $"Facebook: {p.facebookUsername}\n" +
        $"Country: {p.country}\n" +
        $"City: {p.city}\n";
    return result;
}

async void ShowSearchResult(Message message, string request)
{
    var profileResult = await httpClient.GetAsync($"https://localhost:44364/Model/SearchModels/{request}");
    var content = profileResult.Content.ReadAsStringAsync().Result;
    var result = JsonConvert.DeserializeObject<SearchModels>(content);
    for (int k = 0; k < result.Results.Models.Count; k++)
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[] {
                        InlineKeyboardButton.WithCallbackData("More information", callbackData: $"Infor{result.Results.Models[k].Uid}"),
                        InlineKeyboardButton.WithCallbackData("Like", callbackData: $"Like{result.Results.Models[k].Uid}"),
                        InlineKeyboardButton.WithUrl("Watch",  $"{result.Results.Models[k].ViewerUrl}")
        }
                       );
        await botClient.SendPhotoAsync(message.Chat.Id, result?.Results.Models[k].Thumbnails.Images[0].Url, result?.Results.Models[k].Name, replyMarkup: inlineKeyboard);
    }
}

async void  LikeModel(string data, CallbackQuery callb)
{
    LikeModel lk = new LikeModel();
    lk.model = data;
    string res = "You liked this model";
    var json = Newtonsoft.Json.JsonConvert.SerializeObject(lk);
    var param = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");
    var responce = await httpClient.PostAsync($"https://localhost:44364/Profile/LikeTheModel?uid={data}", param);

    await botClient.SendTextMessageAsync(callb.Message.Chat.Id, res);
}

Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

 string MoreInformation(string uid)
{
    //var modInf = new ModelController().ModelInf(uid);
    var  modelResult = GetMoreInformationAsync(uid).Result;
    var modInf = JsonConvert.DeserializeObject<Models3D>(modelResult);

    string result = $"Model name: {modInf.name}\n" +
        $"Created at: {modInf.createdAt}\n" +
        $"Published at: {modInf.publishedAt}\n" +
        $"Views: {modInf.viewCount}\n" +
        $"Likes: {modInf.likeCount}\n" +
        $"Vertex count: {modInf.vertexCount}\n" +
        $"Face count: {modInf.faceCount}\n" +
        $"Comments: {modInf.commentCount}\n" +
        $"Animations: {modInf.animationCount}\n" +
        $"Url: {modInf.viewerUrl}\n" 
        //$"Category: {modInf.categories}\n" +
        //$"Tags: {modInf.tags.ToArray()}\n"
        ;

    return result;
}

async Task<string> GetMoreInformationAsync(string uid)
{
    var modelResult = await httpClient.GetAsync($"https://localhost:44364/Model/ModelInfo/{uid}");
    var content = modelResult.Content.ReadAsStringAsync().Result;
    return content;
}


//string  GetStringFromList(List<object> ob)
//{
//    string result = "";
//    for (int i = 0; i <ob.Count ; i++)
//    {
//        //string categories = Convert.ToString(ob[i].);
//        Console.WriteLine(categories);
//    }
//    return result;
//}


//class Program
//{
//    static ITelegramBotClient bot = new TelegramBotClient("5502147579:AAHfdWNiRsBRIH8cGYsT1zMiiLsT0Taqtsg");
//    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//    {
//        // Некоторые действия
//        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
//        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
//        {
//            var message = update.Message;
//            if (message.Text.ToLower() == "/start")
//            {
//                await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
//                return;
//            }
//            await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
//        }
//    }

//    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
//    {
//        // Некоторые действия
//        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
//    }


//    static void Main(string[] args)
//    {
//        Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

//        var cts = new CancellationTokenSource();
//        var cancellationToken = cts.Token;
//        var receiverOptions = new ReceiverOptions
//        {
//            AllowedUpdates = { }, // receive all update types
//        };
//        bot.StartReceiving(
//            HandleUpdateAsync,
//            HandleErrorAsync,
//            receiverOptions,
//            cancellationToken
//        );
//        Console.ReadLine();
//    }
//}

//  code that works


//namespace TelegramBot
//{


//    class Program
//    {
//        private static string token { get; set; } = "5502147579:AAHfdWNiRsBRIH8cGYsT1zMiiLsT0Taqtsg";
//        private static TelegramBotClient Bot;

//        static void Main(string[] args)
//        {
//            Bot = new TelegramBotClient(token);
//            using var cts = new CancellationTokenSource();

//            ReceiverOptions receiverOptions = new()
//            {
//                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
//                {
//               Telegram.Bot.Types.Enums.UpdateType.Message,
//               Telegram.Bot.Types.Enums.UpdateType.EditedMessage,
//                }
//            };
//            Bot.StartReceiving(Handlers.HandleUpdateAsync, Handlers.HandleErrorAsync, receiverOptions, cts.Token);

//            Console.WriteLine("Bot is ready...");
//            Console.ReadLine();

//            cts.Cancel();
//        }

//    }
//}


//if (message.Text.ToLower().Contains("/authorize"))
//{
//    string token = message.Text.Replace("/authorize", "");
//    token = message.Text.Replace(" ", "");


//    ProfileModel me = new ProfileController().GetMyProfile();
//    await botClient.SendTextMessageAsync(message.Chat.Id, $"User {me.displayName} authorized succesfully");
//    GetMeModels res = new ModelController().GetMyModels(false, false);
//    for (int i = 0; i < res.results.Count; i++)
//    {
//        await botClient.SendPhotoAsync(message.Chat.Id, res.results[i].thumbnails.images[0].url, $"{res.results[i].name}");

//    }
//}