namespace SimNite.Models;

public class SimSettings
{
    public string CommunityFolderPath { get; set; } = string.Empty;
    public string DownloadTempPath { get; set; } = string.Empty;
    public int MaxConcurrentDownloads { get; set; } = 3;
}
