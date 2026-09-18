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

        var videoFormats =
            new List<FormatInfo>();

        var audioFormats =
            new List<FormatInfo>();

        foreach (var format in formats.EnumerateArray())
        {
            var formatInfo =
                ParseFormat(format);

            if (formatInfo is null)
            {
                continue;
            }

            if (formatInfo.HasVideo &&
                formatInfo.Height is not null &&
                SupportedQualities.Contains(
                    formatInfo.Height.Value))
            {
                videoFormats.Add(formatInfo);
            }

            if (formatInfo.HasAudio &&
                !formatInfo.HasVideo)
            {
                audioFormats.Add(formatInfo);
            }
        }

        if (videoFormats.Count == 0)
        {
            return new List<VideoFormat>();
        }

        var bestAudio =
            audioFormats
                .OrderByDescending(
                    format => format.AudioBitrate ?? 0)
                .ThenByDescending(
                    format => format.Size ?? 0)
                .FirstOrDefault();

        var result =
            new List<VideoFormat>();

        foreach (var quality in SupportedQualities)
        {
            var video =
                videoFormats
                    .Where(
                        format =>
                            format.Height == quality)
                    .OrderByDescending(
                        format => GetVideoScore(format))
                    .FirstOrDefault();

            if (video is null)
            {
                continue;
            }

            long? totalSize = null;

            var approximate = false;

            if (video.Size is not null &&
                bestAudio?.Size is not null)
            {
                totalSize =
                    video.Size.Value +
                    bestAudio.Size.Value;

                approximate =
                    video.IsApproximateSize ||
                    bestAudio.IsApproximateSize;
            }
            else if (video.Size is not null)
            {
                totalSize =
                    video.Size.Value;

                approximate =
                    video.IsApproximateSize;
            }

            result.Add(
                new VideoFormat(
                    video.FormatId,
                    bestAudio?.FormatId ?? "",
                    quality,
                    video.Extension,
                    video.Size,
                    bestAudio?.Size,
                    totalSize,
                    approximate));
        }

        return result;
    }

    private static FormatInfo? ParseFormat(
        JsonElement format)
    {
        if (!format.TryGetProperty(
                "format_id",
                out var formatIdElement))
        {
            return null;
        }

        var formatId =
            formatIdElement.GetString();

        if (string.IsNullOrWhiteSpace(formatId))
        {
            return null;
        }

        var hasVideo =
            false;

        var hasAudio =
            false;

        if (format.TryGetProperty(
                "vcodec",
                out var videoCodecElement))
        {
            var codec =
                videoCodecElement.GetString();

            hasVideo =
                !string.IsNullOrWhiteSpace(codec) &&
                codec != "none";
        }

        if (format.TryGetProperty(
                "acodec",
                out var audioCodecElement))
        {
            var codec =
                audioCodecElement.GetString();

            hasAudio =
                !string.IsNullOrWhiteSpace(codec) &&
                codec != "none";
        }

        int? height = null;

        if (format.TryGetProperty(
                "height",
                out var heightElement) &&
            heightElement.ValueKind ==
                JsonValueKind.Number)
        {
            height =
                heightElement.GetInt32();
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

        long? size = null;

        var approximate = false;

        if (format.TryGetProperty(
                "filesize",
                out var fileSizeElement) &&
            fileSizeElement.ValueKind ==
                JsonValueKind.Number)
        {
            size =
                fileSizeElement.GetInt64();
        }

        if (size is null &&
            format.TryGetProperty(
                "filesize_approx",
                out var approximateElement) &&
            approximateElement.ValueKind ==
                JsonValueKind.Number)
        {
            size =
                approximateElement.GetInt64();

            approximate = true;
        }

        double? audioBitrate = null;

        if (format.TryGetProperty(
                "abr",
                out var abrElement) &&
            abrElement.ValueKind ==
                JsonValueKind.Number)
        {
            audioBitrate =
                abrElement.GetDouble();
        }

        double? videoBitrate = null;

        if (format.TryGetProperty(
                "vbr",
                out var vbrElement) &&
            vbrElement.ValueKind ==
                JsonValueKind.Number)
        {
            videoBitrate =
                vbrElement.GetDouble();
        }

        double? totalBitrate = null;

        if (format.TryGetProperty(
                "tbr",
                out var tbrElement) &&
            tbrElement.ValueKind ==
                JsonValueKind.Number)
        {
            totalBitrate =
                tbrElement.GetDouble();
        }

        return new FormatInfo(
            formatId,
            hasVideo,
            hasAudio,
            height,
            extension,
            size,
            approximate,
            audioBitrate,
            videoBitrate,
            totalBitrate);
    }

    private static double GetVideoScore(
        FormatInfo format)
    {
        var score = 0.0;

        if (format.Extension == "mp4")
        {
            score += 1000;
        }

        score +=
            (format.VideoBitrate ?? 0);

        score +=
            (format.Size ?? 0) / 1_000_000.0;

        return score;
    }

    private sealed class FormatInfo
    {
        public string FormatId { get; }

        public bool HasVideo { get; }

        public bool HasAudio { get; }

        public int? Height { get; }

        public string Extension { get; }

        public long? Size { get; }

        public bool IsApproximateSize { get; }

        public double? AudioBitrate { get; }

        public double? VideoBitrate { get; }

        public double? TotalBitrate { get; }

        public FormatInfo(
            string formatId,
            bool hasVideo,
            bool hasAudio,
            int? height,
            string extension,
            long? size,
            bool isApproximateSize,
            double? audioBitrate,
            double? videoBitrate,
            double? totalBitrate)
        {
            FormatId = formatId;
            HasVideo = hasVideo;
            HasAudio = hasAudio;
            Height = height;
            Extension = extension;
            Size = size;
            IsApproximateSize =
                isApproximateSize;

            AudioBitrate =
                audioBitrate;

            VideoBitrate =
                videoBitrate;

            TotalBitrate =
                totalBitrate;
        }
    }
}
