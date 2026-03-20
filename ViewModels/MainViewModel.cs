using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class MainViewModel : BaseViewModel
{
	private BaseViewModel _currentViewModel;
	private readonly ObservableCollection<NavigationStep> _steps;

	public MainViewModel(
		IDatabaseService databaseService, 
		IProfileService profileService,
		IDownloadService downloadService,
		IInstallService installService)
	{
		WelcomeViewModel = new WelcomeViewModel(this);
		ModListViewModel = new ModListViewModel(databaseService, profileService);
		InstallViewModel = new InstallViewModel(downloadService, installService);
		CameraViewModel = new CameraViewModel(new CameraService());
		SettingsViewModel = new SettingsViewModel(profileService);
		SupportViewModel = new SupportViewModel();

		// Initialiser les étapes de navigation
		_steps = new ObservableCollection<NavigationStep>
		{
			new NavigationStep(1, "Bienvenue", "Découvrez SimNite", ShowWelcomeCommand),
			new NavigationStep(2, "Catalogue", "Explorez les mods disponibles", ShowModListCommand),
			new NavigationStep(3, "Installation", "Installez vos mods", ShowInstallCommand)
		};

		// Commencer par la page d'accueil
		_currentViewModel = WelcomeViewModel;
		UpdateStepStatus();

		ShowWelcomeCommand = new RelayCommand(_ => NavigateToWelcome());
		ShowModListCommand = new RelayCommand(_ => NavigateToModList());
		ShowInstallCommand = new RelayCommand(_ => NavigateToInstall());
		ShowCameraCommand = new RelayCommand(_ => NavigateToCamera());
		ShowSettingsCommand = new RelayCommand(_ => NavigateToSettings());
		ShowSupportCommand = new RelayCommand(_ => NavigateToSupport());

		ModListViewModel.InstallRequested += OnInstallRequested;
	}

	private void OnInstallRequested(System.Collections.Generic.IEnumerable<Mod> mods)
	{
		InstallViewModel.InitializeTasks(mods);
		NavigateToInstall();
		
		if (InstallViewModel.StartSimulationCommand.CanExecute(null))
		{
			InstallViewModel.StartSimulationCommand.Execute(null);
		}
	}

	public BaseViewModel CurrentViewModel
	{
		get => _currentViewModel;
		private set 
		{
			if (SetProperty(ref _currentViewModel, value))
			{
				UpdateStepStatus();
				OnPropertyChanged(nameof(IsModListActive));
				OnPropertyChanged(nameof(IsInstallActive));
				OnPropertyChanged(nameof(IsCameraActive));
				OnPropertyChanged(nameof(IsSettingsActive));
				OnPropertyChanged(nameof(IsSupportActive));
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

	public CameraViewModel CameraViewModel { get; }

	public SettingsViewModel SettingsViewModel { get; }

	public SupportViewModel SupportViewModel { get; }

	public ICommand ShowWelcomeCommand { get; }

	public ICommand ShowModListCommand { get; }

	public ICommand ShowInstallCommand { get; }

	public ICommand ShowCameraCommand { get; }

	public ICommand ShowSettingsCommand { get; }

	public ICommand ShowSupportCommand { get; }

	public bool IsModListActive => CurrentViewModel == ModListViewModel;
	public bool IsInstallActive => CurrentViewModel == InstallViewModel;
	public bool IsCameraActive => CurrentViewModel == CameraViewModel;
	public bool IsSettingsActive => CurrentViewModel == SettingsViewModel;
	public bool IsSupportActive => CurrentViewModel == SupportViewModel;

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
	private void NavigateToCamera() => CurrentViewModel = CameraViewModel;
	private void NavigateToSettings() => CurrentViewModel = SettingsViewModel;
	private void NavigateToSupport() => CurrentViewModel = SupportViewModel;
}
