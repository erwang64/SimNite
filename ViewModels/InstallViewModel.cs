using System.Collections.ObjectModel;
using System.Windows.Input;
using SimNite.Models;

namespace SimNite.ViewModels;

public class InstallViewModel : BaseViewModel
{
	private string _currentStep = "Ready";
	private double _overallProgress;
	private bool _isInstalling;

	public InstallViewModel()
	{
		Tasks = new ObservableCollection<InstallTask>();
		StartSimulationCommand = new RelayCommand(_ => StartSimulationAsync(), _ => !IsInstalling);
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

	private async Task StartSimulationAsync()
	{
		if (IsInstalling)
		{
			return;
		}

		try
		{
			IsInstalling = true;
			Tasks.Clear();
			CurrentStep = "Preparing install session...";
			OverallProgress = 0;

			for (var progress = 0; progress <= 100; progress += 10)
			{
				OverallProgress = progress;
				CurrentStep = $"Installing selected mods... {progress}%";
				await Task.Delay(120);
			}

			CurrentStep = "Installation session completed.";
		}
		catch (Exception ex)
		{
			CurrentStep = $"Simulation failed: {ex.Message}";
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
