using SimNite.ViewModels;

namespace SimNite.Models;

public class InstallTask : BaseViewModel
{
    private InstallStatus _status = InstallStatus.Pending;
    private double _progress;
    private string _message = string.Empty;

    public Mod Mod { get; set; } = new();

    public InstallStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
    
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}
