using SimNite.Models;

namespace SimNite.Services.Interfaces;

public interface IProfileService
{
	Task SaveProfileAsync(string filePath, ModProfile profile, CancellationToken cancellationToken);

	Task<ModProfile> LoadProfileAsync(string filePath, CancellationToken cancellationToken);

	Task<IReadOnlyList<string>> ScanCommunityFolderAsync(string communityFolderPath, CancellationToken cancellationToken);

	Task SaveSettingsAsync(string filePath, SimSettings settings, CancellationToken cancellationToken);

	Task<SimSettings> LoadSettingsAsync(string filePath, CancellationToken cancellationToken);

	string? DetectCommunityFolderPath();
}
