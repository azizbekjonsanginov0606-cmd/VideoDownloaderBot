namespace VideoDownloaderBot.Services;

public sealed class PlatformService
{
    public string DetectPlatform(string url)
    {
        if (!Uri.TryCreate(
                url,
                UriKind.Absolute,
                out var uri))
        {
            return "Unknown";
        }

        return uri.Host.ToLowerInvariant() switch
        {
            "youtube.com" => "YouTube",
            "www.youtube.com" => "YouTube",
            "youtu.be" => "YouTube",

            "tiktok.com" => "TikTok",
            "www.tiktok.com" => "TikTok",

            "instagram.com" => "Instagram",
            "www.instagram.com" => "Instagram",

            "vkvideo.ru" => "VK",
            "www.vkvideo.ru" => "VK",

            _ => "Unknown"
        };
    }
}