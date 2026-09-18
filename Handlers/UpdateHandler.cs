using Telegram.Bot;
using Telegram.Bot.Types;

namespace VideoDownloaderBot.Handlers;

public sealed class UpdateHandler
{
    private readonly MessageHandler _messageHandler;
    private readonly CallbackQueryHandler _callbackQueryHandler;

    public UpdateHandler(
        MessageHandler messageHandler,
        CallbackQueryHandler callbackQueryHandler)
    {
        _messageHandler = messageHandler;
        _callbackQueryHandler = callbackQueryHandler;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Message is not null)
        {
            await _messageHandler.HandleAsync(
                update.Message,
                cancellationToken);

            return;
        }

        if (update.CallbackQuery is not null)
        {
            await _callbackQueryHandler.HandleAsync(
                update.CallbackQuery,
                cancellationToken);

            return;
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(
            $"ERROR: {exception.Message}");

        return Task.CompletedTask;
    }
}