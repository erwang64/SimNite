using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class ModListViewModel : BaseViewModel
{
	private readonly IDatabaseService _databaseService;
	private readonly IProfileService _profileService;
	private readonly string _localDatabasePath;
	private readonly string? _remoteDatabaseUrl;
	private readonly List<Mod> _allMods = new();
	private CancellationTokenSource? _loadCancellationTokenSource;
	private string _searchText = string.Empty;
	private ModCategory? _selectedCategory;
	private bool _isLoading;
	private string? _errorMessage;

	public ModListViewModel(IDatabaseService databaseService, IProfileService profileService, string? localDatabasePath = null, string? remoteDatabaseUrl = null)
	{
		_databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
		_profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
		_localDatabasePath = localDatabasePath ?? Path.Combine(AppContext.BaseDirectory, "Data", "mods.json");
		_remoteDatabaseUrl = remoteDatabaseUrl;

		FilteredMods = new ObservableCollection<Mod>();
		SelectedMods = new ObservableCollection<Mod>();
		Categories = Enum.GetValues<ModCategory>();

        // Removed hardcoded mock mod so that the database loads correctly

                LoadModsCommand = new RelayCommand(_ => LoadModsAsync(), _ => !IsLoading);
                RefreshModsCommand = new RelayCommand(_ => LoadModsAsync(forceRefresh: true), _ => !IsLoading);
                ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => !IsLoading);
                CancelLoadCommand = new RelayCommand(_ => CancelLoading(), _ => IsLoading);
                InstallSelectionCommand = new RelayCommand(_ => 
                {
                    if (SelectedMods.Count > 0)
                    {
                        InstallRequested?.Invoke(SelectedMods.ToList());
                    }
                }, _ => SelectedMods.Count > 0);
				
                ImportProfileCommand = new RelayCommand(_ => ImportProfileAsync(), _ => !IsLoading);
                ExportProfileCommand = new RelayCommand(_ => ExportProfileAsync(), _ => SelectedMods.Count > 0 && !IsLoading);

                // Event subscriptions handled in LoadModsAsync
        }

        public event Action<IEnumerable<Mod>>? InstallRequested;

        public ObservableCollection<Mod> FilteredMods { get; }
        
        public IEnumerable<IGrouping<ModCategory, Mod>> GroupedMods => FilteredMods.GroupBy(m => m.Category);
        
        public ObservableCollection<Mod> SelectedMods { get; }

        public IReadOnlyList<ModCategory> Categories { get; }

        public ICommand LoadModsCommand { get; }

        public ICommand RefreshModsCommand { get; }

        public ICommand ClearFiltersCommand { get; }

        public ICommand CancelLoadCommand { get; }

        public ICommand InstallSelectionCommand { get; }
        
        public ICommand ImportProfileCommand { get; }
        
        public ICommand ExportProfileCommand { get; }

	public string SearchText
	{
		get => _searchText;
		set
		{
			if (SetProperty(ref _searchText, value))
			{
				ApplyFilters();
			}
		}
	}

	public ModCategory? SelectedCategory
	{
		get => _selectedCategory;
		set
		{
			if (SetProperty(ref _selectedCategory, value))
			{
				ApplyFilters();
			}
		}
	}

	public bool IsLoading
	{
		get => _isLoading;
		private set
		{
			if (SetProperty(ref _isLoading, value))
			{
				RaiseCommandStates();
			}
		}
	}

	public string? ErrorMessage
	{
		get => _errorMessage;
		private set => SetProperty(ref _errorMessage, value);
	}

	public IReadOnlyList<Mod> GetSelectedMods()
	{
		return _allMods.Where(mod => mod.IsChecked).ToList();
	}

	public void ApplySelectedModIds(IEnumerable<string> selectedIds)
	{
		var selectedSet = new HashSet<string>(selectedIds, StringComparer.OrdinalIgnoreCase);

		foreach (var mod in _allMods)
		{
			mod.IsChecked = selectedSet.Contains(mod.Id);
		}

		ApplyFilters();
	}

	private void OnModPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(Mod.IsChecked) && sender is Mod mod)
		{
			if (mod.IsChecked && !SelectedMods.Contains(mod))
			{
				SelectedMods.Add(mod);
			}
			else if (!mod.IsChecked && SelectedMods.Contains(mod))
			{
				SelectedMods.Remove(mod);
			}
			
			OnPropertyChanged(nameof(SelectedMods));
			(InstallSelectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
		}
	}

	private async Task LoadModsAsync(bool forceRefresh = false)
	{
		if (IsLoading)
		{
			return;
		}

		_loadCancellationTokenSource?.Dispose();
		_loadCancellationTokenSource = new CancellationTokenSource();

		IsLoading = true;
		ErrorMessage = null;

		try
		{
			if (!forceRefresh && _allMods.Count > 0)
			{
				ApplyFilters();
				return;
			}

			var mods = await _databaseService.GetModsAsync(
				_localDatabasePath,
				_remoteDatabaseUrl,
				_loadCancellationTokenSource.Token);

			foreach (var mod in _allMods)
			{
				mod.PropertyChanged -= OnModPropertyChanged;
			}

			_allMods.Clear();
			_allMods.AddRange(mods);
			
			foreach (var mod in _allMods)
			{
				mod.PropertyChanged += OnModPropertyChanged;
			}
			
			UpdateSelectedModsList();
			ApplyFilters();
		}
		catch (OperationCanceledException)
		{
			ErrorMessage = "Loading was canceled.";
		}
		catch (Exception ex)
		{
			ErrorMessage = ex.Message;
		}
		finally
		{
			IsLoading = false;
		}
	}

	private void UpdateSelectedModsList()
	{
		SelectedMods.Clear();
		foreach (var mod in _allMods.Where(m => m.IsChecked))
		{
			SelectedMods.Add(mod);
		}
		OnPropertyChanged(nameof(SelectedMods));
		(InstallSelectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
	}

	private void ClearFilters()
	{
		SearchText = string.Empty;
		SelectedCategory = null;
	}

	private void CancelLoading()
	{
		_loadCancellationTokenSource?.Cancel();
	}

	private void ApplyFilters()
	{
		var filtered = _allMods.Where(MatchesFilters);

		FilteredMods.Clear();
		foreach (var mod in filtered)
		{
			FilteredMods.Add(mod);
		}
		OnPropertyChanged(nameof(GroupedMods));
	}

	private bool MatchesFilters(Mod mod)
	{
		if (SelectedCategory.HasValue && mod.Category != SelectedCategory.Value)
		{
			return false;
		}

		if (string.IsNullOrWhiteSpace(SearchText))
		{
			return true;
		}

		var query = SearchText.Trim();
		return mod.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
			   || mod.Id.Contains(query, StringComparison.OrdinalIgnoreCase)
			   || mod.Tags.Any(tag => tag.Contains(query, StringComparison.OrdinalIgnoreCase));
	}

	private void RaiseCommandStates()
	{
		(LoadModsCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(RefreshModsCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(ClearFiltersCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(CancelLoadCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(ImportProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
		(ExportProfileCommand as RelayCommand)?.RaiseCanExecuteChanged();
	}
	
	private async Task ImportProfileAsync()
	{
		var dialog = new Microsoft.Win32.OpenFileDialog
		{
			Title = "Import SimNite Profile",
			Filter = "JSON Profiles (*.json)|*.json|All files (*.*)|*.*",
			CheckFileExists = true
		};

		if (dialog.ShowDialog() == true)
		{
			try
			{
				IsLoading = true;
				var profile = await _profileService.LoadProfileAsync(dialog.FileName, CancellationToken.None);
				
				if (profile.Mods != null)
				{
					ApplySelectedModIds(profile.Mods.Select(m => m.Id));
				}
				ErrorMessage = null;
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Failed to import profile: {ex.Message}";
			}
			finally
			{
				IsLoading = false;
			}
		}
	}

	private async Task ExportProfileAsync()
	{
		var dialog = new Microsoft.Win32.SaveFileDialog
		{
			Title = "Export SimNite Profile",
			Filter = "JSON Profiles (*.json)|*.json",
			FileName = "MyProfile.json",
			DefaultExt = ".json"
		};

		if (dialog.ShowDialog() == true)
		{
			try
			{
				IsLoading = true;
				var profile = new ModProfile
				{
					Id = Guid.NewGuid().ToString(),
					Name = Path.GetFileNameWithoutExtension(dialog.FileName),
					CreatedAt = DateTime.Now,
					Mods = SelectedMods.ToList()
				};
				
				await _profileService.SaveProfileAsync(dialog.FileName, profile, CancellationToken.None);
				ErrorMessage = null;
			}
			catch (Exception ex)
			{
				ErrorMessage = $"Failed to export profile: {ex.Message}";
			}
			finally
			{
				IsLoading = false;
			}
		}
	}
}
