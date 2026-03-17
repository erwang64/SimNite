using System.IO;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class SettingsViewModel : BaseViewModel
{
	private readonly IProfileService _profileService;
	private string _settingsPath;
	private string _communityFolderPath = string.Empty;
	private string _downloadTempPath;
	private int _maxConcurrentDownloads = 3;
	private string _statusMessage = "Ready";
	private bool _isBusy;

	public SettingsViewModel(IProfileService profileService)
	{
		_profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
		_settingsPath = Path.Combine(AppContext.BaseDirectory, "Data", "settings.json");
		_downloadTempPath = Path.Combine(Path.GetTempPath(), "SimNite");

		LoadSettingsCommand = new RelayCommand(_ => LoadSettingsAsync(), _ => !IsBusy);
		SaveSettingsCommand = new RelayCommand(_ => SaveSettingsAsync(), _ => !IsBusy);
		DetectCommunityFolderCommand = new RelayCommand(_ => DetectCommunityFolder(), _ => !IsBusy);

		LoadSettingsCommand.Execute(null);
	}

	public ICommand LoadSettingsCommand { get; }

	public ICommand SaveSettingsCommand { get; }

	public ICommand DetectCommunityFolderCommand { get; }

	public string SettingsPath
	{
		get => _settingsPath;
		set => SetProperty(ref _settingsPath, value);
	}

	public string CommunityFolderPath
	{
		get => _communityFolderPath;
		set => SetProperty(ref _communityFolderPath, value);
	}

	public string DownloadTempPath
	{
		get => _downloadTempPath;
		set => SetProperty(ref _downloadTempPath, value);
	}

	public int MaxConcurrentDownloads
	{
		get => _maxConcurrentDownloads;
		set => SetProperty(ref _maxConcurrentDownloads, value);
	}

	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	public bool IsBusy
	{
		get => _isBusy;
		private set
		{
			if (SetProperty(ref _isBusy, value))
			{
				RaiseCommandStates();
			}
		}
	}

	private async Task LoadSettingsAsync()
	{
		if (IsBusy)
		{
			return;
		}

		try
		{
			IsBusy = true;
			var settings = await _profileService.LoadSettingsAsync(SettingsPath, CancellationToken.None);

			CommunityFolderPath = settings.CommunityFolderPath;
			DownloadTempPath = settings.DownloadTempPath;
			MaxConcurrentDownloads = settings.MaxConcurrentDownloads <= 0 ? 3 : settings.MaxConcurrentDownloads;

			StatusMessage = "Settings loaded.";
		}
		catch (Exception ex)
		{
			StatusMessage = $"Load failed: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task SaveSettingsAsync()
	{
		if (IsBusy)
		{
			return;
		}

		try
		{
			IsBusy = true;

			var settings = new SimSettings
			{
				CommunityFolderPath = CommunityFolderPath,
				DownloadTempPath = DownloadTempPath,
				MaxConcurrentDownloads = MaxConcurrentDownloads < 1 ? 1 : MaxConcurrentDownloads
			};

			await _profileService.SaveSettingsAsync(SettingsPath, settings, CancellationToken.None);
			StatusMessage = "Settings saved.";
		}
		catch (Exception ex)
		{
			StatusMessage = $"Save failed: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private void DetectCommunityFolder()
	{
		var detected = _profileService.DetectCommunityFolderPath();
		if (string.IsNullOrWhiteSpace(detected))
		{
			StatusMessage = "No Community folder detected.";
			return;
		}

		CommunityFolderPath = detected;
		StatusMessage = "Community folder detected.";
	}

	private void RaiseCommandStates()
	{
		(LoadSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(SaveSettingsCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(DetectCommunityFolderCommand as RelayCommand)?.RaiseCanExecuteChanged();
	}
}
