using SimNite.Models;

namespace SimNite.Services.Interfaces;

public interface IInstallService
{
	Task InstallModAsync(
		Mod mod,
		string downloadedFilePath,
		string communityFolderPath,
		CancellationToken cancellationToken);

	Task ExtractZipToCommunityAsync(
		string zipFilePath,
		string communityFolderPath,
		CancellationToken cancellationToken);

	Task LaunchInstallerAsync(
		string installerPath,
		CancellationToken cancellationToken);
}
