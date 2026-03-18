using System.Windows.Input;

namespace SimNite.ViewModels;

public class WelcomeViewModel : BaseViewModel
{
    private readonly MainViewModel _mainViewModel;

    public WelcomeViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        StartJourneyCommand = new RelayCommand(_ => StartJourney());
    }

    public ICommand StartJourneyCommand { get; }

    private void StartJourney()
    {
        // Naviguer vers le catalogue pour commencer l'expérience
        _mainViewModel.NavigateToViewModel(_mainViewModel.ModListViewModel);
    }
}
