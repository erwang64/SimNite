using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class SimulatorSettingsViewModel : BaseViewModel
{
	private readonly ISimulatorSettingsService _simulatorSettingsService;

	private string _userCfgPath = string.Empty;
	private string _backupDestinationPath = string.Empty;
	private string _restoreZipPath = string.Empty;
	private string _statusMessage = "Ready";
	private bool _isBusy;

	public SimulatorSettingsViewModel(ISimulatorSettingsService simulatorSettingsService)
	{
		_simulatorSettingsService = simulatorSettingsService ?? throw new ArgumentNullException(nameof(simulatorSettingsService));

		BackupCommand = new RelayCommand(_ => BackupAsync(), _ => !IsBusy && CanBackup());
		RestoreCommand = new RelayCommand(_ => RestoreAsync(), _ => !IsBusy && CanRestore());

		BrowseUserCfgCommand = new RelayCommand(_ => BrowseUserCfg(), _ => !IsBusy);
		BrowseBackupPathCommand = new RelayCommand(_ => BrowseBackupPath(), _ => !IsBusy);
		BrowseRestoreZipCommand = new RelayCommand(_ => BrowseRestoreZip(), _ => !IsBusy);

		BackupDestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimNite", "UserCfgBackups");
		var defaultZip = Path.Combine(BackupDestinationPath, "UserCfg.opt_Backup.zip");
		if (File.Exists(defaultZip))
		{
			RestoreZipPath = defaultZip;
		}

		var detected = _simulatorSettingsService.DetectUserCfgPath();
		if (!string.IsNullOrWhiteSpace(detected))
		{
			UserCfgPath = detected;
			StatusMessage = "MSFS UserCfg.opt detected.";
		}
		else
		{
			StatusMessage = "MSFS UserCfg.opt not detected. Please browse manually.";
		}
	}

	public string UserCfgPath
	{
		get => _userCfgPath;
		set
		{
			if (SetProperty(ref _userCfgPath, value))
			{
				(RestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public string BackupDestinationPath
	{
		get => _backupDestinationPath;
		set
		{
			if (SetProperty(ref _backupDestinationPath, value))
			{
				(RestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public string RestoreZipPath
	{
		get => _restoreZipPath;
		set
		{
			if (SetProperty(ref _restoreZipPath, value))
			{
				(RestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public string StatusMessage
	{
		get => _statusMessage;
		set => SetProperty(ref _statusMessage, value);
	}

	public bool IsBusy
	{
		get => _isBusy;
		set
		{
			if (SetProperty(ref _isBusy, value))
			{
				(BackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(RestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BrowseUserCfgCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BrowseBackupPathCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BrowseRestoreZipCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public ICommand BackupCommand { get; }
	public ICommand RestoreCommand { get; }
	public ICommand BrowseUserCfgCommand { get; }
	public ICommand BrowseBackupPathCommand { get; }
	public ICommand BrowseRestoreZipCommand { get; }

	private bool CanBackup()
	{
		return !string.IsNullOrWhiteSpace(UserCfgPath) && File.Exists(UserCfgPath) &&
		       !string.IsNullOrWhiteSpace(BackupDestinationPath);
	}

	private bool CanRestore()
	{
		if (string.IsNullOrWhiteSpace(UserCfgPath))
		{
			return false;
		}

		var destDirectory = Path.GetDirectoryName(UserCfgPath);
		return !string.IsNullOrWhiteSpace(destDirectory) &&
		       !string.IsNullOrWhiteSpace(RestoreZipPath) && File.Exists(RestoreZipPath);
	}

	private void BrowseUserCfg()
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Title = "Select MSFS UserCfg.opt",
			Filter = "UserCfg.opt (*.opt)|*.opt",
			// Allow restore even if the file doesn't exist yet (it can be recreated).
			CheckFileExists = false
		};

		if (dialog.ShowDialog() == true)
		{
			UserCfgPath = dialog.FileName;
			StatusMessage = "UserCfg.opt selected.";
		}
	}

	private void BrowseBackupPath()
	{
		var dialog = new Microsoft.Win32.OpenFolderDialog
		{
			Title = "Select Backup Destination Folder"
		};

		if (dialog.ShowDialog() == true)
		{
			BackupDestinationPath = dialog.FolderName;
		}
	}

	private void BrowseRestoreZip()
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Title = "Select UserCfg.opt Backup ZIP",
			Filter = "ZIP Archives (*.zip)|*.zip",
			CheckFileExists = true
		};

		if (dialog.ShowDialog() == true)
		{
			RestoreZipPath = dialog.FileName;
		}
	}

	private async Task BackupAsync()
	{
		try
		{
			IsBusy = true;
			StatusMessage = "Backing up UserCfg.opt...";

			var zipFilePath = Path.Combine(BackupDestinationPath, "UserCfg.opt_Backup.zip");
			await _simulatorSettingsService.BackupUserCfgAsync(UserCfgPath, zipFilePath);

			StatusMessage = $"Backup successful to {zipFilePath} !";

			// Auto-remplissage du champ restore.
			if (string.IsNullOrWhiteSpace(RestoreZipPath) || !File.Exists(RestoreZipPath))
			{
				RestoreZipPath = zipFilePath;
			}
		}
		catch (Exception ex)
		{
			StatusMessage = $"Error: {ex.Message}";
			System.Windows.MessageBox.Show(
				$"Backup error:\n{ex.Message}",
				"Backup Error",
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Error);
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task RestoreAsync()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(UserCfgPath))
			{
				StatusMessage = "Destination UserCfg.opt path is not set.";
				return;
			}

			if (string.IsNullOrWhiteSpace(RestoreZipPath) || !File.Exists(RestoreZipPath))
			{
				StatusMessage = "Backup ZIP file not found. Please browse for the file.";
				return;
			}

			IsBusy = true;
			StatusMessage = "Restoring UserCfg.opt from ZIP...";

			await _simulatorSettingsService.RestoreUserCfgAsync(RestoreZipPath, UserCfgPath);

			StatusMessage = "UserCfg.opt restored successfully!";
			System.Windows.MessageBox.Show(
				"MSFS UserCfg.opt has been restored.\n\nTip: restart the simulator to apply the changes.",
				"Restore Complete",
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Information);
		}
		catch (Exception ex)
		{
			StatusMessage = $"Error: {ex.Message}";
			System.Windows.MessageBox.Show(
				$"An error occurred during restoration:\n{ex.Message}",
				"Restore Error",
				System.Windows.MessageBoxButton.OK,
				System.Windows.MessageBoxImage.Error);
		}
		finally
		{
			IsBusy = false;
		}
	}
}

