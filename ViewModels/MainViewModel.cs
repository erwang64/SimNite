using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class MainViewModel : BaseViewModel
{
	private BaseViewModel _currentViewModel;
	private readonly ObservableCollection<NavigationStep> _steps;

	public MainViewModel(IDatabaseService databaseService, IProfileService profileService)
	{
		WelcomeViewModel = new WelcomeViewModel(this);
		ModListViewModel = new ModListViewModel(databaseService);
		InstallViewModel = new InstallViewModel();
		ProfileViewModel = new ProfileViewModel(profileService, ModListViewModel);
		SettingsViewModel = new SettingsViewModel(profileService);

		// Initialiser les étapes de navigation
		_steps = new ObservableCollection<NavigationStep>
		{
			new NavigationStep(1, "Bienvenue", "Découvrez SimNite", ShowWelcomeCommand),
			new NavigationStep(2, "Catalogue", "Explorez les mods disponibles", ShowModListCommand),
			new NavigationStep(3, "Installation", "Installez vos mods", ShowInstallCommand),
			new NavigationStep(4, "Profils", "Gérez vos configurations", ShowProfileCommand)
		};

		// Commencer par la page d'accueil
		_currentViewModel = WelcomeViewModel;
		UpdateStepStatus();

		ShowWelcomeCommand = new RelayCommand(_ => NavigateToWelcome());
		ShowModListCommand = new RelayCommand(_ => NavigateToModList());
		ShowInstallCommand = new RelayCommand(_ => NavigateToInstall());
		ShowProfileCommand = new RelayCommand(_ => NavigateToProfile());
		ShowSettingsCommand = new RelayCommand(_ => NavigateToSettings());
	}

	public BaseViewModel CurrentViewModel
	{
		get => _currentViewModel;
		private set 
		{
			if (SetProperty(ref _currentViewModel, value))
			{
				UpdateStepStatus();
			}
		}
	}

	// Méthode publique pour permettre la navigation depuis d'autres ViewModels
	public void NavigateToViewModel(BaseViewModel viewModel)
	{
		CurrentViewModel = viewModel;
	}

	public ObservableCollection<NavigationStep> Steps => _steps;

	public WelcomeViewModel WelcomeViewModel { get; }

	public ModListViewModel ModListViewModel { get; }

	public InstallViewModel InstallViewModel { get; }

	public ProfileViewModel ProfileViewModel { get; }

	public SettingsViewModel SettingsViewModel { get; }

	public ICommand ShowWelcomeCommand { get; }

	public ICommand ShowModListCommand { get; }

	public ICommand ShowInstallCommand { get; }

	public ICommand ShowProfileCommand { get; }

	public ICommand ShowSettingsCommand { get; }

	private void UpdateStepStatus()
	{
		for (int i = 0; i < _steps.Count; i++)
		{
			var step = _steps[i];
			step.IsCurrent = false;
			step.IsCompleted = false;

			if (_currentViewModel == WelcomeViewModel && i == 0)
				step.IsCurrent = true;
			else if (_currentViewModel == ModListViewModel && i == 1)
				step.IsCurrent = true;
			else if (_currentViewModel == InstallViewModel && i == 2)
				step.IsCurrent = true;
			else if (_currentViewModel == ProfileViewModel && i == 3)
				step.IsCurrent = true;

			// Marquer les étapes précédentes comme complétées
			var currentIndex = _steps.IndexOf(_steps.FirstOrDefault(s => s.IsCurrent));
			if (currentIndex > -1)
			{
				for (int j = 0; j < currentIndex; j++)
				{
					_steps[j].IsCompleted = true;
				}
			}
		}
	}

	private void NavigateToWelcome() => CurrentViewModel = WelcomeViewModel;
	private void NavigateToModList() => CurrentViewModel = ModListViewModel;
	private void NavigateToInstall() => CurrentViewModel = InstallViewModel;
	private void NavigateToProfile() => CurrentViewModel = ProfileViewModel;
	private void NavigateToSettings() => CurrentViewModel = SettingsViewModel;
}
