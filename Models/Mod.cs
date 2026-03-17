namespace SimNite.Models;

public class Mod
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public ModCategory Category { get; set; }
    public ModType Type { get; set; }
    public InstallMode InstallMode { get; set; }
    public long SizeBytes { get; set; }
    public bool IsChecked { get; set; }
    public List<string> Tags { get; set; } = new();
}
