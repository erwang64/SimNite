using System.Windows.Input;
using SimNite.ViewModels;

namespace SimNite.Models;

public class NavigationStep : BaseViewModel
{
    private bool _isCurrent;
    private bool _isCompleted;

    public NavigationStep(int number, string title, string description, ICommand navigateCommand)
    {
        Number = number;
        Title = title;
        Description = description;
        NavigateCommand = navigateCommand;
    }

    public int Number { get; }

    public string Title { get; }

    public string Description { get; }

    public ICommand NavigateCommand { get; }

    public bool IsCurrent
    {
        get => _isCurrent;
        set => SetProperty(ref _isCurrent, value);
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }
}
