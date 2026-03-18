using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimNite.Models;

public class Mod : INotifyPropertyChanged
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public ModCategory Category { get; set; }
    public ModType Type { get; set; }
    public InstallMode InstallMode { get; set; }
    public long SizeBytes { get; set; }

    private bool _isChecked;
    public bool IsChecked 
    { 
        get => _isChecked; 
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }
    }
    
    public List<string> Tags { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
