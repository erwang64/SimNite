using SimNite.Models;

namespace SimNite.Services.Interfaces;

public interface IDatabaseService
{
	Task<ModDatabase> LoadLocalDatabaseAsync(string filePath, CancellationToken cancellationToken);

	Task<ModDatabase> LoadRemoteDatabaseAsync(string databaseUrl, CancellationToken cancellationToken);

	Task<IReadOnlyList<Mod>> GetModsAsync(
		string localFilePath,
		string? remoteDatabaseUrl,
		CancellationToken cancellationToken);
}

