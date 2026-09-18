using Telegram.Bot;
using Telegram.Bot.Types;

using VideoDownloaderBot.Services;

namespace VideoDownloaderBot.Handlers;

public sealed class CallbackQueryHandler
{
    private readonly ITelegramBotClient _bot;

    private readonly VideoDownloaderService
        _videoDownloaderService;

    private readonly PendingDownloadService
        _pendingDownloadService;

    public CallbackQueryHandler(
        ITelegramBotClient bot,
        VideoDownloaderService videoDownloaderService,
        PendingDownloadService pendingDownloadService)
    {
        _bot = bot;

        _videoDownloaderService =
            videoDownloaderService;

        _pendingDownloadService =
            pendingDownloadService;
    }

    public async Task HandleAsync(
        CallbackQuery callbackQuery,
        CancellationToken cancellationToken)
    {
        var data =
            callbackQuery.Data;

        if (string.IsNullOrWhiteSpace(data))
        {
            return;
        }

        if (!data.StartsWith(
                "format:",
                StringComparison.Ordinal))
        {
            return;
        }

        var formatId =
            data["format:".Length..];

        if (string.IsNullOrWhiteSpace(formatId))
        {
            return;
        }

        // IMPORTANT:
        // Answer Telegram immediately.
        await _bot.AnswerCallbackQuery(
            callbackQuery.Id,
            cancellationToken: cancellationToken);

        var chatId =
            callbackQuery.Message?.Chat.Id;

        if (chatId is null)
        {
            return;
        }

        if (!_pendingDownloadService.TryGet(
                chatId.Value,
                out var pending))
        {
            await _bot.SendMessage(
                chatId.Value,
                "❌ Ин download дигар фаъол нест. Link-ро аз нав фиристед.",
                cancellationToken: cancellationToken);

            return;
        }

        var selectedFormat =
            pending!.Formats.FirstOrDefault(
                format =>
                    format.FormatId == formatId);

        if (selectedFormat is null)
        {
            await _bot.SendMessage(
                chatId.Value,
                "❌ Ин quality дигар дастрас нест.",
                cancellationToken: cancellationToken);

            return;
        }

        await _bot.SendMessage(
            chatId.Value,
            $"⏬ Download оғоз шуд: {selectedFormat.Height}p",
            cancellationToken: cancellationToken);

        string filePath;

        try
        {
            filePath =
                await _videoDownloaderService
                    .DownloadAsync(
                        pending.Url,
                        selectedFormat);
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                exception);

            await _bot.SendMessage(
                chatId.Value,
                "❌ Download иҷро нашуд.",
                cancellationToken: cancellationToken);

            return;
        }

        try
        {
            await using var stream =
                File.OpenRead(filePath);

            await _bot.SendVideo(
                chatId.Value,
                stream,
                cancellationToken: cancellationToken);
        }
        catch (Exception exception)
        {
            Console.WriteLine(
                exception);

            await _bot.SendMessage(
                chatId.Value,
                "❌ Видео ба Telegram фиристода нашуд.",
                cancellationToken: cancellationToken);
        }
        finally
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Cleanup error: {exception}");
            }
        }
    }
}