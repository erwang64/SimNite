using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class DatabaseService : IDatabaseService
{
	private static readonly HttpClient HttpClient = new();

	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		ReadCommentHandling = JsonCommentHandling.Skip,
		AllowTrailingCommas = true,
		Converters = { new JsonStringEnumConverter() }
	};

	public async Task<ModDatabase> LoadLocalDatabaseAsync(string filePath, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			throw new ArgumentException("Database file path cannot be empty.", nameof(filePath));
		}

		try
		{
			Stream stream;
			if (File.Exists(filePath))
			{
				stream = File.OpenRead(filePath);
			}
			else
			{
				stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SimNite.Data.mods.json")
					?? throw new FileNotFoundException("Local database file and embedded resource were not found.", filePath);
			}

			await using (stream)
			{
				var database = await JsonSerializer.DeserializeAsync<ModDatabase>(stream, SerializerOptions, cancellationToken)
					?? new ModDatabase();

				ValidateDatabase(database);
				return database;
			}
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load local database from '{filePath}'.", ex);
		}
	}

	public async Task<ModDatabase> LoadRemoteDatabaseAsync(string databaseUrl, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(databaseUrl))
		{
			throw new ArgumentException("Database URL cannot be empty.", nameof(databaseUrl));
		}

		if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out _))
		{
			throw new ArgumentException("Database URL must be an absolute URL.", nameof(databaseUrl));
		}

		try
		{
			await using var stream = await HttpClient.GetStreamAsync(databaseUrl, cancellationToken);
			var database = await JsonSerializer.DeserializeAsync<ModDatabase>(stream, SerializerOptions, cancellationToken)
				?? new ModDatabase();

			ValidateDatabase(database);
			return database;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to load remote database from '{databaseUrl}'.", ex);
		}
	}

	public async Task<IReadOnlyList<Mod>> GetModsAsync(
		string localFilePath,
		string? remoteDatabaseUrl,
		CancellationToken cancellationToken)
	{
		if (!string.IsNullOrWhiteSpace(remoteDatabaseUrl))
		{
			try
			{
				var remoteDatabase = await LoadRemoteDatabaseAsync(remoteDatabaseUrl, cancellationToken);
				return remoteDatabase.Mods;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch
			{
				// Fallback to local source when remote access fails.
			}
		}

		var localDatabase = await LoadLocalDatabaseAsync(localFilePath, cancellationToken);
		return localDatabase.Mods;
	}

	private static void ValidateDatabase(ModDatabase database)
	{
		if (database.Mods is null)
		{
			throw new InvalidOperationException("Database content is invalid: Mods cannot be null.");
		}

		var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		for (var index = 0; index < database.Mods.Count; index++)
		{
			var mod = database.Mods[index];
			if (string.IsNullOrWhiteSpace(mod.Id))
			{
				throw new InvalidOperationException($"Database content is invalid: Mods[{index}].Id is required.");
			}

			if (!ids.Add(mod.Id))
			{
				throw new InvalidOperationException($"Database content is invalid: duplicate mod id '{mod.Id}'.");
			}

			if (string.IsNullOrWhiteSpace(mod.Name))
			{
				throw new InvalidOperationException($"Database content is invalid: Mod '{mod.Id}' has no name.");
			}

			if (string.IsNullOrWhiteSpace(mod.DownloadUrl) ||
				!Uri.TryCreate(mod.DownloadUrl, UriKind.Absolute, out _))
			{
				throw new InvalidOperationException($"Database content is invalid: Mod '{mod.Id}' has an invalid DownloadUrl.");
			}

			if (mod.SizeBytes < 0)
			{
				throw new InvalidOperationException($"Database content is invalid: Mod '{mod.Id}' has negative SizeBytes.");
			}
		}
	}
}
