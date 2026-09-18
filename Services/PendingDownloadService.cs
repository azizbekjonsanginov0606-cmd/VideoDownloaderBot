using System.Collections.Concurrent;

using VideoDownloaderBot.Models;

namespace VideoDownloaderBot.Services;

public sealed class PendingDownloadService
{
    private readonly ConcurrentDictionary<
        long,
        PendingDownload> _downloads = new();

    public void Save(
        long chatId,
        string url,
        IReadOnlyList<VideoFormat> formats)
    {
        var download =
            new PendingDownload(
                chatId,
                url,
                formats);

        _downloads[chatId] =
            download;
    }

    public bool TryGet(
        long chatId,
        out PendingDownload? download)
    {
        return _downloads.TryRemove(
            chatId,
            out download);
    }
}