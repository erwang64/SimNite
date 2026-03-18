using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using SimNite.Models;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class InstallService : IInstallService
{
	public async Task InstallModAsync(
		Mod mod,
		string downloadedFilePath,
		string communityFolderPath,
		CancellationToken cancellationToken)
	{
		if (mod is null)
		{
			throw new ArgumentNullException(nameof(mod));
		}

		try
		{
			if (mod.Type == ModType.CommunityZip)
			{
				await ExtractZipToCommunityAsync(downloadedFilePath, communityFolderPath, cancellationToken);
				return;
			}

			if (mod.Type == ModType.ExternalInstaller && mod.InstallMode == InstallMode.Assisted)
			{
				await LaunchInstallerAsync(downloadedFilePath, cancellationToken);
				return;
			}

			throw new NotSupportedException(
				$"Unsupported install combination: Type={mod.Type}, Mode={mod.InstallMode}.");
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				$"Installation failed for mod '{mod.Name}' ({mod.Id}).",
				ex);
		}
	}

	public async Task ExtractZipToCommunityAsync(
		string zipFilePath,
		string communityFolderPath,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(zipFilePath))
		{
			throw new ArgumentException("Zip file path cannot be empty.", nameof(zipFilePath));
		}

		if (string.IsNullOrWhiteSpace(communityFolderPath))
		{
			throw new ArgumentException("Community folder path cannot be empty.", nameof(communityFolderPath));
		}

		if (!File.Exists(zipFilePath))
		{
			throw new FileNotFoundException("Zip file was not found.", zipFilePath);
		}

		try
		{
			Directory.CreateDirectory(communityFolderPath);

			await Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();
				ZipFile.ExtractToDirectory(zipFilePath, communityFolderPath, overwriteFiles: true);
			}, cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Extraction failed for '{zipFilePath}'.", ex);
		}
	}

	public async Task LaunchInstallerAsync(string installerPath, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(installerPath))
		{
			throw new ArgumentException("Installer path cannot be empty.", nameof(installerPath));
		}

		if (!File.Exists(installerPath))
		{
			throw new FileNotFoundException("Installer file was not found.", installerPath);
		}

		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			var startInfo = new ProcessStartInfo
			{
				FileName = installerPath,
				UseShellExecute = true,
				WorkingDirectory = Path.GetDirectoryName(installerPath) ?? string.Empty
			};

			var process = Process.Start(startInfo)
				?? throw new InvalidOperationException($"Unable to start installer '{installerPath}'.");

			await process.WaitForExitAsync(cancellationToken);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Failed to launch installer '{installerPath}'.", ex);
		}
	}
}
