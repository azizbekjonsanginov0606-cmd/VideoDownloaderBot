using Telegram.Bot.Types.ReplyMarkups;

using VideoDownloaderBot.Models;

namespace VideoDownloaderBot.Services;

public sealed class QualityKeyboardService
{
    public InlineKeyboardMarkup Create(
        IReadOnlyCollection<VideoFormat> formats)
    {
        var buttons =
            new List<InlineKeyboardButton>();

        foreach (var format in formats)
        {
            var quality =
                format.Height switch
                {
                    2160 => "4K",
                    1440 => "2K",
                    1080 => "1080p",
                    720 => "720p",
                    480 => "480p",
                    360 => "360p",

                    _ => $"{format.Height}p"
                };

            var size =
                FormatSize(format);

            buttons.Add(
                InlineKeyboardButton.WithCallbackData(
                    $"{quality} • {size}",
                    $"format:{format.FormatId}"));
        }

        var rows =
            new List<IEnumerable<InlineKeyboardButton>>();

        for (var i = 0; i < buttons.Count; i += 2)
        {
            var row =
                new List<InlineKeyboardButton>
                {
                    buttons[i]
                };

            if (i + 1 < buttons.Count)
            {
                row.Add(buttons[i + 1]);
            }

            rows.Add(row);
        }

        return new InlineKeyboardMarkup(rows);
    }

    private static string FormatSize(
        VideoFormat format)
    {
        if (format.TotalSize is null)
        {
            return "размер неизвестен";
        }

        var megabytes =
            format.TotalSize.Value /
            1024.0 /
            1024.0;

        var prefix =
            format.IsApproximateSize
                ? "~"
                : "";

        return
            $"{prefix}{megabytes:F1} MB";
    }
}
