using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.ViewModels;

public class ModListViewModel : BaseViewModel
{
	private readonly IDatabaseService _databaseService;
	private readonly string _localDatabasePath;
	private readonly string? _remoteDatabaseUrl;
	private readonly List<Mod> _allMods = new();
	private CancellationTokenSource? _loadCancellationTokenSource;
	private string _searchText = string.Empty;
	private ModCategory? _selectedCategory;
	private bool _isLoading;
	private string? _errorMessage;

	public ModListViewModel(IDatabaseService databaseService, string? localDatabasePath = null, string? remoteDatabaseUrl = null)
	{
		_databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
		_localDatabasePath = localDatabasePath ?? Path.Combine(AppContext.BaseDirectory, "Data", "mods.json");
		_remoteDatabaseUrl = remoteDatabaseUrl;

		FilteredMods = new ObservableCollection<Mod>();
		Categories = Enum.GetValues<ModCategory>();

                // Mock Mod for FBW Installer testing
                var fbwMod = new Mod
                {
                        Id = "fbw-installer",
                        Name = "FlyByWire Installer",
                        Description = "Installeur officiel pour les avions FlyByWire Simulations (A32NX, A380X).",
                        Author = "FlyByWire Simulations",
                        Version = "v1.0",
                        DownloadUrl = "https://flybywirecdn.com/installer/release/FlyByWire_Installer_Setup.exe",
                        Category = ModCategory.Aircraft,
                        Type = ModType.ExternalInstaller,
                        InstallMode = InstallMode.Auto,
                        SizeBytes = 780000,
                        IsChecked = false
                };
                _allMods.Add(fbwMod);
                FilteredMods.Add(fbwMod);

                LoadModsCommand = new RelayCommand(_ => LoadModsAsync(), _ => !IsLoading);
                RefreshModsCommand = new RelayCommand(_ => LoadModsAsync(forceRefresh: true), _ => !IsLoading);
                ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => !IsLoading);
                CancelLoadCommand = new RelayCommand(_ => CancelLoading(), _ => IsLoading);
        }

        public ObservableCollection<Mod> FilteredMods { get; }

        public IReadOnlyList<ModCategory> Categories { get; }

        public ICommand LoadModsCommand { get; }

        public ICommand RefreshModsCommand { get; }

        public ICommand ClearFiltersCommand { get; }

        public ICommand CancelLoadCommand { get; }

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

			_allMods.Clear();
			_allMods.AddRange(mods);
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
	}
}
