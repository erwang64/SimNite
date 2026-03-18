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

			foreach (var task in Tasks)
			{
				task.Status = InstallStatus.Downloading;
				
				var tempDir = Path.Combine(Path.GetTempPath(), "SimNite", "Downloads");
				Directory.CreateDirectory(tempDir);

				var progressIndicator = new Progress<double>(p => 
				{
					task.Progress = p;
					OverallProgress = (completedCount * 100.0 + p) / totalTasks;
				});

				CurrentStep = $"Downloading {task.Mod.Name}...";
				var downloadedFile = await _downloadService.DownloadFileAsync(task.Mod.DownloadUrl, tempDir, progressIndicator, CancellationToken.None);

				task.Status = InstallStatus.Extracting;
				CurrentStep = $"Starting installer for {task.Mod.Name}...";
				
				// Assumes community folder path is set somewhere, but for FlyByWire (Assisted/ExternalInstaller), it's ignored anyway.
				await _installService.InstallModAsync(task.Mod, downloadedFile, "C:\\TempCommunityManager", CancellationToken.None);

				task.Status = InstallStatus.Completed;
				task.Progress = 100;
				completedCount++;
				OverallProgress = (completedCount * 100.0) / totalTasks;
			}

			CurrentStep = "Installation session completed.";
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
