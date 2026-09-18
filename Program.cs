using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

using VideoDownloaderBot.Configuration;
using VideoDownloaderBot.Handlers;
using VideoDownloaderBot.Services;

var configuration =
    new BotConfiguration();

var bot =
    new TelegramBotClient(
        configuration.Token);

var platformService =
    new PlatformService();

var pendingDownloadService =
    new PendingDownloadService();

var videoFormatService =
    new VideoFormatService();

var qualityKeyboardService =
    new QualityKeyboardService();

var videoDownloaderService =
    new VideoDownloaderService();

var messageHandler =
    new MessageHandler(
        bot,
        platformService,
        pendingDownloadService,
        videoFormatService,
        qualityKeyboardService);

var callbackQueryHandler =
    new CallbackQueryHandler(
        bot,
        videoDownloaderService,
        pendingDownloadService);

var updateHandler =
    new UpdateHandler(
        messageHandler,
        callbackQueryHandler);

var receiverOptions =
    new ReceiverOptions
    {
        AllowedUpdates =
            Array.Empty<UpdateType>()
    };

bot.StartReceiving(
    updateHandler.HandleUpdateAsync,
    updateHandler.HandleErrorAsync,
    receiverOptions);

var me =
    await bot.GetMe();

Console.WriteLine(
    $"Bot @{me.Username} started!");

Console.WriteLine(
    "Waiting for messages...");

await Task.Delay(
    Timeout.Infinite);