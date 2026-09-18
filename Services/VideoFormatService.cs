using System.Diagnostics;
using System.Text.Json;

using VideoDownloaderBot.Models;

namespace VideoDownloaderBot.Services;

public sealed class VideoFormatService
{
    private static readonly int[] SupportedQualities =
    {
        2160,
        1440,
        1080,
        720,
        480,
        360
    };

    public async Task<List<VideoFormat>> GetAvailableFormatsAsync(
        string url)
    {
        using var process = new Process();

        process.StartInfo.FileName = "yt-dlp";

        process.StartInfo.ArgumentList.Add("-J");

        process.StartInfo.ArgumentList.Add("--no-playlist");

        process.StartInfo.ArgumentList.Add("--js-runtimes");

        process.StartInfo.ArgumentList.Add("node");

        process.StartInfo.ArgumentList.Add(url);

        process.StartInfo.UseShellExecute = false;

        process.StartInfo.RedirectStandardOutput = true;

        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.CreateNoWindow = true;

        process.Start();

        var outputTask =
            process.StandardOutput.ReadToEndAsync();

        var errorTask =
            process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        var output =
            await outputTask;

        var error =
            await errorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"yt-dlp format check failed:\n{error}");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new Exception(
                "yt-dlp returned empty result.");
        }

        using var document =
            JsonDocument.Parse(output);

        var root =
            document.RootElement;

        if (!root.TryGetProperty(
                "formats",
                out var formats))
        {
            throw new Exception(
                "Formats were not found.");
        }

        var availableFormats =
            new List<VideoFormat>();

        foreach (var format in formats.EnumerateArray())
        {
            if (!format.TryGetProperty(
                    "height",
                    out var heightElement))
            {
                continue;
            }

            if (heightElement.ValueKind !=
                JsonValueKind.Number)
            {
                continue;
            }

            var height =
                heightElement.GetInt32();

            if (!SupportedQualities.Contains(height))
            {
                continue;
            }

            if (!format.TryGetProperty(
                    "format_id",
                    out var formatIdElement))
            {
                continue;
            }

            var formatId =
                formatIdElement.GetString();

            if (string.IsNullOrWhiteSpace(formatId))
            {
                continue;
            }

            var extension = "mp4";

            if (format.TryGetProperty(
                    "ext",
                    out var extensionElement))
            {
                extension =
                    extensionElement.GetString()
                    ?? "mp4";
            }

            var hasVideo = true;

            if (format.TryGetProperty(
                    "vcodec",
                    out var videoCodecElement))
            {
                var videoCodec =
                    videoCodecElement.GetString();

                hasVideo =
                    !string.Equals(
                        videoCodec,
                        "none",
                        StringComparison.OrdinalIgnoreCase);
            }

            if (!hasVideo)
            {
                continue;
            }

            var hasAudio = false;

            if (format.TryGetProperty(
                    "acodec",
                    out var audioCodecElement))
            {
                var audioCodec =
                    audioCodecElement.GetString();

                hasAudio =
                    !string.Equals(
                        audioCodec,
                        "none",
                        StringComparison.OrdinalIgnoreCase);
            }

            availableFormats.Add(
                new VideoFormat(
                    formatId,
                    height,
                    extension,
                    hasAudio));
        }

        return availableFormats
            .GroupBy(format => format.Height)
            .Select(group =>
                group
                    .OrderByDescending(
                        format => format.HasAudio)
                    .First())
            .OrderByDescending(
                format => format.Height)
            .ToList();
    }
}
