using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using VideoDownloaderBot.Services;

namespace VideoDownloaderBot.Handlers;

public sealed class MessageHandler
{
    private readonly ITelegramBotClient _bot;

    private readonly PlatformService
        _platformService;

    private readonly PendingDownloadService
        _pendingDownloadService;

    private readonly VideoFormatService
        _videoFormatService;

    private readonly QualityKeyboardService
        _qualityKeyboardService;

    public MessageHandler(
        ITelegramBotClient bot,
        PlatformService platformService,
        PendingDownloadService pendingDownloadService,
        VideoFormatService videoFormatService,
        QualityKeyboardService qualityKeyboardService)
    {
        _bot = bot;

        _platformService =
            platformService;

        _pendingDownloadService =
            pendingDownloadService;

        _videoFormatService =
            videoFormatService;

        _qualityKeyboardService =
            qualityKeyboardService;
    }

    public async Task HandleAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message.Text is null)
        {
            return;
        }

        var text =
            message.Text.Trim();

        Console.WriteLine(
            $"Text: {text}");

        Console.WriteLine(
            $"Chat ID: {message.Chat.Id}");

        Console.WriteLine(
            $"User: {message.From?.Username}");

        if (text == "/start")
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "👋 Салом! Ман Video Downloader Bot ҳастам.",
                cancellationToken: cancellationToken);

            return;
        }

        if (text == "/help")
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "ℹ️ Link-и видеоро фиристед.",
                cancellationToken: cancellationToken);

            return;
        }

        if (!Uri.TryCreate(
                text,
                UriKind.Absolute,
                out var uri))
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "❌ Лутфан link-и дурусти видео фиристед.",
                cancellationToken: cancellationToken);

            return;
        }

        var platform =
            _platformService
                .DetectPlatform(text);

        if (platform == "Unknown")
        {
            await _bot.SendMessage(
                message.Chat.Id,
                "❌ Ин platform ҳоло дастгирӣ намешавад.",
                cancellationToken: cancellationToken);

            return;
        }

        Console.WriteLine(
            $"Platform: {platform}");

        await _bot.SendMessage(
            message.Chat.Id,
            "🔎 Ҷустуҷӯи quality-ҳои дастрас...",
            cancellationToken: cancellationToken);

        try
        {
            var formats =
                await _videoFormatService
                    .GetAvailableFormatsAsync(text);

            if (formats.Count == 0)
            {
                await _bot.SendMessage(
                    message.Chat.Id,
                    "❌ Барои ин видео quality-и дастрас ёфт нашуд.",
                    cancellationToken: cancellationToken);

                return;
            }

            _pendingDownloadService.Save(
                message.Chat.Id,
                text,
                formats);

            var keyboard =
                _qualityKeyboardService
                    .Create(formats);

            await _bot.SendMessage(
                message.Chat.Id,
                "🎬 Quality-ро интихоб кунед:",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                exception);

            await _bot.SendMessage(
                message.Chat.Id,
                "❌ Ҳангоми гирифтани information-и видео хато шуд.",
                cancellationToken: cancellationToken);
        }
    }
}