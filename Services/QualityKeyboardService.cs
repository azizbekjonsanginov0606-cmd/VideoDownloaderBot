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
            var label =
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

            buttons.Add(
                InlineKeyboardButton
                    .WithCallbackData(
                        label,
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
}
