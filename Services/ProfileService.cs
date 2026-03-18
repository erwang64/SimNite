using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class ProfileService : IProfileService
{
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

	public async Task SaveProfileAsync(string filePath, ModProfile profile, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Profile path cannot be empty.", nameof(filePath));
		}

		if (profile is null)
		{
			throw new ArgumentNullException(nameof(profile));
		}

		try
		{
			EnsureParentDirectory(filePath);
			await using var stream = File.Create(filePath);
			await JsonSerializer.SerializeAsync(stream, profile, SerializerOptions, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to save profile to '{filePath}'.", ex);
		}
	}

	public async Task<ModProfile> LoadProfileAsync(string filePath, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Profile path cannot be empty.", nameof(filePath));
		}

		if (!File.Exists(filePath))
		{
			throw new FileNotFoundException("Profile file was not found.", filePath);
		}

		try
		{
			await using var stream = File.OpenRead(filePath);
			var profile = await JsonSerializer.DeserializeAsync<ModProfile>(stream, SerializerOptions, cancellationToken);
			return profile ?? new ModProfile();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load profile from '{filePath}'.", ex);
		}
	}

	public Task<IReadOnlyList<string>> ScanCommunityFolderAsync(string communityFolderPath, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(communityFolderPath))
		{
			throw new ArgumentException("Community folder path cannot be empty.", nameof(communityFolderPath));
		}

		if (!Directory.Exists(communityFolderPath))
		{
			throw new DirectoryNotFoundException($"Community folder was not found: '{communityFolderPath}'.");
		}

		cancellationToken.ThrowIfCancellationRequested();

		var modFolders = Directory.GetDirectories(communityFolderPath)
			.Select(Path.GetFileName)
			.Where(name => !string.IsNullOrWhiteSpace(name))
			.Cast<string>()
			.ToList();

		return Task.FromResult<IReadOnlyList<string>>(modFolders);
	}

	public async Task SaveSettingsAsync(string filePath, SimSettings settings, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Settings path cannot be empty.", nameof(filePath));
		}

		if (settings is null)
		{
			throw new ArgumentNullException(nameof(settings));
		}

		try
		{
			EnsureParentDirectory(filePath);
			await using var stream = File.Create(filePath);
			await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to save settings to '{filePath}'.", ex);
		}
	}

	public async Task<SimSettings> LoadSettingsAsync(string filePath, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Settings path cannot be empty.", nameof(filePath));
		}

		if (!File.Exists(filePath))
		{
			return new SimSettings();
		}

		try
		{
			await using var stream = File.OpenRead(filePath);
			var settings = await JsonSerializer.DeserializeAsync<SimSettings>(stream, SerializerOptions, cancellationToken);
			return settings ?? new SimSettings();
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load settings from '{filePath}'.", ex);
		}
	}

	public string? DetectCommunityFolderPath()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		// First, try reading UserCfg.opt to find custom installation paths (e.g D:\MSFS)
		var userCfgCandidates = new[]
		{
			Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", "UserCfg.opt"),
			Path.Combine(roamingAppData, "Microsoft Flight Simulator", "UserCfg.opt"),
			Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", "UserCfg.opt")
		};

		foreach (var cfgPath in userCfgCandidates)
		{
			if (File.Exists(cfgPath))
			{
				try 
				{
					var lines = File.ReadAllLines(cfgPath);
					var installLine = lines.LastOrDefault(l => l.StartsWith("InstalledPackagesPath"));
					if (!string.IsNullOrWhiteSpace(installLine))
					{
						// Extract path from quotes: InstalledPackagesPath "D:\MSFS"
						var parts = installLine.Split('"', StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length >= 2)
						{
							var customPath = parts[1];
							var customCommunity = Path.Combine(customPath, "Community");
							if (Directory.Exists(customCommunity))
							{
								return customCommunity;
							}
						}
					}
				}
				catch 
				{
					// Ignore read errors and fallback to defaults
				}
			}
		}

		// Fallback to default locations if UserCfg.opt parsing fails or doesn't have it
		var defaultCandidates = new[]
		{
			Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", "Packages", "Community"),
			Path.Combine(roamingAppData, "Microsoft Flight Simulator", "Packages", "Community"),
			Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", "Packages", "Community")
		};

		return defaultCandidates.FirstOrDefault(Directory.Exists);
	}

	private static void EnsureParentDirectory(string filePath)
	{
		var parentDirectory = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrWhiteSpace(parentDirectory))
		{
			Directory.CreateDirectory(parentDirectory);
		}
	}
}
