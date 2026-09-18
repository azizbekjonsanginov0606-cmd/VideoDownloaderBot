namespace VideoDownloaderBot.Models;

public sealed class VideoFormat
{
    public string FormatId { get; }

    public string AudioFormatId { get; }

    public int Height { get; }

    public string Extension { get; }

    public long? VideoSize { get; }

    public long? AudioSize { get; }

    public long? TotalSize { get; }

    public bool IsApproximateSize { get; }

    public VideoFormat(
        string formatId,
        string audioFormatId,
        int height,
        string extension,
        long? videoSize,
        long? audioSize,
        long? totalSize,
        bool isApproximateSize)
    {
        FormatId = formatId;
        AudioFormatId = audioFormatId;
        Height = height;
        Extension = extension;
        VideoSize = videoSize;
        AudioSize = audioSize;
        TotalSize = totalSize;
        IsApproximateSize = isApproximateSize;
    }
}
