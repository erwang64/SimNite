using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class CameraViewModel : BaseViewModel
{
	private readonly ICameraService _cameraService;
	private string _statusMessage = "Ready";
	private string _simObjectsPath = string.Empty;
	private string _backupDestinationPath = string.Empty;
	private string _restoreZipPath = string.Empty;
	private int _cameraCount;
	private bool _isBusy;

	public CameraViewModel(ICameraService cameraService)
	{
		_cameraService = cameraService ?? throw new ArgumentNullException(nameof(cameraService));
		
		BackupCommand = new RelayCommand(_ => BackupAsync(), _ => !IsBusy && CameraCount > 0 && !string.IsNullOrWhiteSpace(BackupDestinationPath));
		RestoreCommand = new RelayCommand(_ => RestoreAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(SimObjectsPath) && !string.IsNullOrWhiteSpace(RestoreZipPath));
		
		BrowseSimObjectsCommand = new RelayCommand(_ => BrowseSimObjects(), _ => !IsBusy);
		BrowseBackupPathCommand = new RelayCommand(_ => BrowseBackupPath(), _ => !IsBusy);
		BrowseRestoreZipCommand = new RelayCommand(_ => BrowseRestoreZip(), _ => !IsBusy);
		RefreshCountCommand = new RelayCommand(_ => RefreshCount(), _ => !IsBusy);
		
		BackupDestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SimNite", "CameraBackups");
        
		var defaultZip = Path.Combine(BackupDestinationPath, "SimObjects_Backup.zip");
		if (File.Exists(defaultZip))
		{
			RestoreZipPath = defaultZip;
		}

		var detected = _cameraService.DetectSimObjectsPath();
		if (!string.IsNullOrWhiteSpace(detected))
		{
			SimObjectsPath = detected;
		}
		else
		{
			StatusMessage = "SimObjects path not detected. Please browse manually.";
		}
	}

	public string SimObjectsPath
	{
		get => _simObjectsPath;
		set
		{
			if (SetProperty(ref _simObjectsPath, value))
			{
				RefreshCount();
				(RestoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

	public int CameraCount
	{
		get => _cameraCount;
		set 
		{
			if (SetProperty(ref _cameraCount, value))
			{
				(BackupCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
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
				(BrowseSimObjectsCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BrowseBackupPathCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(BrowseRestoreZipCommand as RelayCommand)?.RaiseCanExecuteChanged();
				(RefreshCountCommand as RelayCommand)?.RaiseCanExecuteChanged();
			}
		}
	}

	public ICommand BackupCommand { get; }
	public ICommand RestoreCommand { get; }
	public ICommand BrowseSimObjectsCommand { get; }
	public ICommand BrowseBackupPathCommand { get; }
	public ICommand BrowseRestoreZipCommand { get; }
	public ICommand RefreshCountCommand { get; }

	private void BrowseSimObjects()
	{
		var dialog = new Microsoft.Win32.OpenFolderDialog
		{
			Title = "Select SimObjects Folder"
		};
		if (dialog.ShowDialog() == true)
		{
			SimObjectsPath = dialog.FolderName;
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
			Title = "Select Camera Backup ZIP",
			Filter = "ZIP Archives (*.zip)|*.zip",
			CheckFileExists = true
		};
		if (dialog.ShowDialog() == true)
		{
			RestoreZipPath = dialog.FileName;
		}
	}

	private void RefreshCount()
	{
		try
		{
			CameraCount = _cameraService.GetCustomCamerasCount(SimObjectsPath);
			StatusMessage = $"Found {CameraCount} custom camera files.";
		}
		catch (Exception ex)
		{
			StatusMessage = "Cannot scan SimObjects folder.";
		}
	}

	private async Task BackupAsync()
	{
		try
		{
			IsBusy = true;
			StatusMessage = "Backing up cameras...";
			var zipFilePath = Path.Combine(BackupDestinationPath, "SimObjects_Backup.zip");
			await _cameraService.BackupCamerasAsync(SimObjectsPath, zipFilePath);
			StatusMessage = $"Backup successful to {zipFilePath} !";
			
			// Si le champ restore était vide (première fois qu'on fait un backup), on l'auto-remplit
			if (string.IsNullOrWhiteSpace(RestoreZipPath) || !File.Exists(RestoreZipPath))
			{
				RestoreZipPath = zipFilePath;
			}
		}
		catch (Exception ex)
		{
			StatusMessage = $"Error: {ex.Message}";
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
			if (string.IsNullOrWhiteSpace(RestoreZipPath) || !File.Exists(RestoreZipPath))
			{
				StatusMessage = "Backup ZIP file not found. Please browse for the file.";
				return;
			}

			IsBusy = true;
			StatusMessage = "Restoring cameras from ZIP...";
			await _cameraService.RestoreCamerasAsync(RestoreZipPath, SimObjectsPath);
			StatusMessage = "Cameras restored successfully!";
			RefreshCount();

			System.Windows.MessageBox.Show(
				"Custom cameras and panel states have been successfully restored to Microsoft Flight Simulator!", 
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