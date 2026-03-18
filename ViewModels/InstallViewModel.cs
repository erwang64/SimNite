using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class InstallViewModel : BaseViewModel
{
	private readonly IDownloadService _downloadService;
	private readonly IInstallService _installService;
	private string _currentStep = "Ready";
	private double _overallProgress;
	private bool _isInstalling;

	public InstallViewModel(IDownloadService downloadService, IInstallService installService)
	{
		_downloadService = downloadService ?? throw new ArgumentNullException(nameof(downloadService));
		_installService = installService ?? throw new ArgumentNullException(nameof(installService));

		Tasks = new ObservableCollection<InstallTask>();
		StartSimulationCommand = new RelayCommand(_ => StartInstallAsync(), _ => !IsInstalling);
		ResetCommand = new RelayCommand(_ => Reset(), _ => !IsInstalling);
	}

	public ObservableCollection<InstallTask> Tasks { get; }

	public string CurrentStep
	{
		get => _currentStep;
		private set => SetProperty(ref _currentStep, value);
	}

	public double OverallProgress
	{
		get => _overallProgress;
		private set => SetProperty(ref _overallProgress, value);
	}

	public bool IsInstalling
	{
		get => _isInstalling;
		private set
		{
			if (SetProperty(ref _isInstalling, value))
			{
				(StartSimulationCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(ResetCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public ICommand StartSimulationCommand { get; }

	public ICommand ResetCommand { get; }

	public void InitializeTasks(IEnumerable<Mod> mods)
	{
		Tasks.Clear();
		foreach (var mod in mods)
		{
			Tasks.Add(new InstallTask { Mod = mod });
		}
		OverallProgress = 0;
		CurrentStep = "Ready to install.";
	}

	private async Task StartInstallAsync()
	{
		if (IsInstalling || Tasks.Count == 0)
		{
			return;
		}

		try
		{
			IsInstalling = true;
			CurrentStep = "Preparing install session...";
			OverallProgress = 0;

			int completedCount = 0;
			int totalTasks = Tasks.Count;
			var tempDir = Path.Combine(Path.GetTempPath(), "SimNite", "Downloads");
			Directory.CreateDirectory(tempDir);

			// File d'attente pour télécharger 1 fichier à la fois en arrière-plan sans engorger le réseau
			using var downloadSemaphore = new SemaphoreSlim(1, 1);

			// Initialiser la phase de téléchargement en arrière-plan
			var downloadTasks = Tasks.Select(async task => 
			{
				await downloadSemaphore.WaitAsync();
				try
				{
					task.Status = InstallStatus.Downloading;
					var progressIndicator = new Progress<double>(p => 
					{
						task.Progress = p;
						// Petite estimation globale pendant le téléchargement (Optionnel)
					});
					
					var path = await _downloadService.DownloadFileAsync(task.Mod.DownloadUrl, tempDir, progressIndicator, CancellationToken.None);
					task.Status = InstallStatus.Pending; // Passe en attente d'installation
					return path;
				}
				finally
				{
					downloadSemaphore.Release();
				}
			}).ToList();

			// Phase d'installation séquentielle
			for (int i = 0; i < totalTasks; i++)
			{
				var task = Tasks[i];
				
				if (task.Status == InstallStatus.Downloading || task.Status == InstallStatus.Pending) 
				{
					CurrentStep = $"Attente du téléchargement de {task.Mod.Name}...";
				}

				// On attend le fichier de notre mod spécifique (téléchargé en fond)
				var downloadedFile = await downloadTasks[i];

				task.Status = InstallStatus.Extracting;
				CurrentStep = $"Lancement de l'installateur : {task.Mod.Name}...";
				
				await _installService.InstallModAsync(task.Mod, downloadedFile, "C:\\TempCommunityManager", CancellationToken.None);

				task.Status = InstallStatus.Completed;
				task.Progress = 100;
				completedCount++;
				OverallProgress = (completedCount * 100.0) / totalTasks;
			}

			CurrentStep = "Toutes les installations sont terminées.";
		}
		catch (Exception ex)
		{
			CurrentStep = $"Installation failed: {ex.Message}";
		}
		finally
		{
			IsInstalling = false;
		}
	}

	private void Reset()
	{
		Tasks.Clear();
		OverallProgress = 0;
		CurrentStep = "Ready";
	}
}
