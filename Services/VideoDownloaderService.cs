using System.Diagnostics;

using VideoDownloaderBot.Models;

namespace VideoDownloaderBot.Services;

public sealed class VideoDownloaderService
{
    public async Task<string> DownloadAsync(
        string url,
        VideoFormat format)
    {
        var downloadFolder =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "downloads");

        Directory.CreateDirectory(
            downloadFolder);

        var fileName =
            $"{Guid.NewGuid()}.mp4";

        var outputFile =
            Path.Combine(
                downloadFolder,
                fileName);

        var formatSelector =
            format.HasAudio
                ? format.FormatId
                : $"{format.FormatId}+bestaudio";

        using var process =
            new Process();

        process.StartInfo.FileName =
            "yt-dlp";

        process.StartInfo.ArgumentList.Add(
            "--js-runtimes");

        process.StartInfo.ArgumentList.Add(
            "node");

        process.StartInfo.ArgumentList.Add(
            "-f");

        process.StartInfo.ArgumentList.Add(
            formatSelector);

        process.StartInfo.ArgumentList.Add(
            "--merge-output-format");

        process.StartInfo.ArgumentList.Add(
            "mp4");

        process.StartInfo.ArgumentList.Add(
            "-o");

        process.StartInfo.ArgumentList.Add(
            outputFile);

        process.StartInfo.ArgumentList.Add(
            url);

        process.StartInfo.UseShellExecute =
            false;

        process.StartInfo.CreateNoWindow =
            true;

        process.StartInfo.RedirectStandardError =
            true;

        process.StartInfo.RedirectStandardOutput =
            true;

        process.Start();

        var outputTask =
            process.StandardOutput
                .ReadToEndAsync();

        var errorTask =
            process.StandardError
                .ReadToEndAsync();

        await process.WaitForExitAsync();

        var output =
            await outputTask;

        var error =
            await errorTask;

        Console.WriteLine(output);

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"yt-dlp failed.\n{error}");
        }

        if (File.Exists(outputFile))
        {
            return outputFile;
        }

        var baseName =
            Path.GetFileNameWithoutExtension(
                outputFile);

        var files =
            Directory.GetFiles(
                downloadFolder,
                $"{baseName}.*");

        var downloadedFile =
            files.FirstOrDefault(
                file =>
                    file.EndsWith(
                        ".mp4",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    file.EndsWith(
                        ".mkv",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    file.EndsWith(
                        ".webm",
                        StringComparison.OrdinalIgnoreCase));

        if (downloadedFile is not null)
        {
            return downloadedFile;
        }

        throw new FileNotFoundException(
            "Downloaded video file was not found.",
            outputFile);
    }
}
