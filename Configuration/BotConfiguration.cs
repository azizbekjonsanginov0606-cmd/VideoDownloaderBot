namespace VideoDownloaderBot.Configuration;

public sealed class BotConfiguration
{
    public string Token { get; }

    public BotConfiguration()
    {
        Token = Environment.GetEnvironmentVariable(
            "TELEGRAM_BOT_TOKEN")
            ?? throw new InvalidOperationException(
                "TELEGRAM_BOT_TOKEN not found.");
    }
}