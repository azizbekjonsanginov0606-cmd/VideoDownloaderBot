namespace VideoDownloaderBot.Models;

public sealed class PendingDownload
{
    public long ChatId { get; }

    public string Url { get; }

    public IReadOnlyList<VideoFormat> Formats { get; }

    public PendingDownload(
        long chatId,
        string url,
        IReadOnlyList<VideoFormat> formats)
    {
        ChatId = chatId;
        Url = url;
        Formats = formats;
    }
}