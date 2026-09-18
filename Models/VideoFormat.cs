namespace VideoDownloaderBot.Models;

public sealed class VideoFormat
{
    public string FormatId { get; }

    public int Height { get; }

    public string Extension { get; }

    public bool HasAudio { get; }

    public VideoFormat(
        string formatId,
        int height,
        string extension,
        bool hasAudio)
    {
        FormatId = formatId;
        Height = height;
        Extension = extension;
        HasAudio = hasAudio;
    }
}
