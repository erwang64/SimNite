using System.Windows.Input;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class MainViewModel : BaseViewModel
{
	private BaseViewModel _currentViewModel;

	public MainViewModel(IDatabaseService databaseService, IProfileService profileService)
	{
		ModListViewModel = new ModListViewModel(databaseService);
		InstallViewModel = new InstallViewModel();
		ProfileViewModel = new ProfileViewModel(profileService, ModListViewModel);
		SettingsViewModel = new SettingsViewModel(profileService);

		_currentViewModel = ModListViewModel;

		ShowModListCommand = new RelayCommand(_ => CurrentViewModel = ModListViewModel);
		ShowInstallCommand = new RelayCommand(_ => CurrentViewModel = InstallViewModel);
		ShowProfileCommand = new RelayCommand(_ => CurrentViewModel = ProfileViewModel);
		ShowSettingsCommand = new RelayCommand(_ => CurrentViewModel = SettingsViewModel);
	}

	public BaseViewModel CurrentViewModel
	{
		get => _currentViewModel;
		private set => SetProperty(ref _currentViewModel, value);
	}

	public ModListViewModel ModListViewModel { get; }

	public InstallViewModel InstallViewModel { get; }

	public ProfileViewModel ProfileViewModel { get; }

	public SettingsViewModel SettingsViewModel { get; }

	public ICommand ShowModListCommand { get; }

	public ICommand ShowInstallCommand { get; }

	public ICommand ShowProfileCommand { get; }

	public ICommand ShowSettingsCommand { get; }
}
