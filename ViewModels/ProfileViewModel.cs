using System.IO;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class ProfileViewModel : BaseViewModel
{
	private readonly IProfileService _profileService;
	private readonly ModListViewModel _modListViewModel;
	private string _profileName = "My Profile";
	private string _profilePath;
	private string _communityFolderPath;
	private string _statusMessage = "Ready";
	private bool _isBusy;

	public ProfileViewModel(IProfileService profileService, ModListViewModel modListViewModel)
	{
		_profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
		_modListViewModel = modListViewModel ?? throw new ArgumentNullException(nameof(modListViewModel));

		_profilePath = Path.Combine(AppContext.BaseDirectory, "Data", "Profiles", "default-profile.json");
		_communityFolderPath = _profileService.DetectCommunityFolderPath() ?? string.Empty;

		SaveProfileCommand = new RelayCommand(_ => SaveProfileAsync(), _ => !IsBusy);
		LoadProfileCommand = new RelayCommand(_ => LoadProfileAsync(), _ => !IsBusy);
		ScanCommunityCommand = new RelayCommand(_ => ScanCommunityAsync(), _ => !IsBusy);
		BrowseProfilePathCommand = new RelayCommand(_ => BrowseProfilePath(), _ => !IsBusy);
		BrowseSaveProfilePathCommand = new RelayCommand(_ => BrowseSaveProfilePath(), _ => !IsBusy);
	}

	public ICommand SaveProfileCommand { get; }

	public ICommand LoadProfileCommand { get; }

	public ICommand ScanCommunityCommand { get; }

	public ICommand BrowseProfilePathCommand { get; }

	public ICommand BrowseSaveProfilePathCommand { get; }

	public string ProfileName
	{
		get => _profileName;
		set => SetProperty(ref _profileName, value);
	}

	public string ProfilePath
	{
		get => _profilePath;
		set => SetProperty(ref _profilePath, value);
	}

	public string CommunityFolderPath
	{
		get => _communityFolderPath;
		set => SetProperty(ref _communityFolderPath, value);
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

	private async Task SaveProfileAsync()
	{
		if (IsBusy)
		{
			return;
		}

		try
		{
			IsBusy = true;

			var profile = new ModProfile
			{
				Id = Guid.NewGuid().ToString("N"),
				Name = string.IsNullOrWhiteSpace(ProfileName) ? "My Profile" : ProfileName.Trim(),
				CreatedAt = DateTime.UtcNow,
				Mods = _modListViewModel.GetSelectedMods().ToList()
			};

			await _profileService.SaveProfileAsync(ProfilePath, profile, CancellationToken.None);
			StatusMessage = $"Profile saved: {ProfilePath}";
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

	private async Task LoadProfileAsync()
	{
		if (IsBusy)
		{
			return;
		}

		try
		{
			IsBusy = true;

			var profile = await _profileService.LoadProfileAsync(ProfilePath, CancellationToken.None);
			ProfileName = profile.Name;
			_modListViewModel.ApplySelectedModIds(profile.Mods.Select(mod => mod.Id));

			StatusMessage = $"Profile loaded: {profile.Name}";
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

	private async Task ScanCommunityAsync()
	{
		if (IsBusy)
		{
			return;
		}

		try
		{
			IsBusy = true;
			var folders = await _profileService.ScanCommunityFolderAsync(CommunityFolderPath, CancellationToken.None);
			_modListViewModel.ApplySelectedModIds(folders);
			StatusMessage = $"Scan complete: {folders.Count} folder(s) found.";
		}
		catch (Exception ex)
		{
			StatusMessage = $"Scan failed: {ex.Message}";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private void BrowseProfilePath()
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
			DefaultExt = "json"
		};
		if (dialog.ShowDialog() == true)
		{
			ProfilePath = dialog.FileName;
		}
	}

	private void BrowseSaveProfilePath()
	{
		var dialog = new Microsoft.Win32.SaveFileDialog
		{
			Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
			DefaultExt = "json"
		};
		if (dialog.ShowDialog() == true)
		{
			ProfilePath = dialog.FileName;
		}
	}

	private void RaiseCommandStates()
	{
		(SaveProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(LoadProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(ScanCommunityCommand as RelayCommand)?.RaiseCanExecuteChanged();
	}
}
