namespace SimNite.Models;

public class InstallTask
{
    public Mod Mod { get; set; } = new();
    public InstallStatus Status { get; set; } = InstallStatus.Pending;
    public double Progress { get; set; }
}
