using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class SimulatorSettingsService : ISimulatorSettingsService
{
	private const string UserCfgFileName = "UserCfg.opt";

	public string? DetectUserCfgPath()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		var candidates = new[]
		{
			Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", UserCfgFileName),
			Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", UserCfgFileName),
			Path.Combine(roamingAppData, "Microsoft Flight Simulator", UserCfgFileName)
		};

		return candidates.FirstOrDefault(File.Exists);
	}

	public async Task BackupUserCfgAsync(string userCfgPath, string outputZipFilePath)
	{
		if (string.IsNullOrWhiteSpace(userCfgPath) || !File.Exists(userCfgPath))
			throw new FileNotFoundException("UserCfg.opt file not found.", userCfgPath);

		var directory = Path.GetDirectoryName(outputZipFilePath);
		if (!string.IsNullOrWhiteSpace(directory))
		{
			Directory.CreateDirectory(directory);
		}

		if (File.Exists(outputZipFilePath))
		{
			File.Delete(outputZipFilePath);
		}

		await Task.Run(() =>
		{
			using var zipStream = ZipFile.Open(outputZipFilePath, ZipArchiveMode.Create);

			// On stocke seulement `UserCfg.opt` pour rester fidèle au “backup unique”.
			var entry = zipStream.CreateEntry(UserCfgFileName, CompressionLevel.Optimal);
			using var entryStream = entry.Open();
			using var fileStream = File.OpenRead(userCfgPath);

			fileStream.CopyTo(entryStream);
		});
	}

	public async Task RestoreUserCfgAsync(string inputZipFilePath, string destUserCfgPath)
	{
		if (!File.Exists(inputZipFilePath))
			throw new FileNotFoundException("Backup ZIP file not found.", inputZipFilePath);

		if (string.IsNullOrWhiteSpace(destUserCfgPath))
			throw new DirectoryNotFoundException("Destination path not valid.");

		var destDirectory = Path.GetDirectoryName(destUserCfgPath);
		if (string.IsNullOrWhiteSpace(destDirectory))
			throw new DirectoryNotFoundException("Destination folder not valid.");

		Directory.CreateDirectory(destDirectory);

		await Task.Run(() =>
		{
			using var zipStream = ZipFile.OpenRead(inputZipFilePath);
			var entry = zipStream.Entries.FirstOrDefault(e => string.Equals(e.Name, UserCfgFileName, StringComparison.OrdinalIgnoreCase));

			if (entry is null)
				throw new FileNotFoundException($"Backup entry '{UserCfgFileName}' not found in ZIP.", inputZipFilePath);

			// L'extraction écrase le fichier existant (si présent).
			entry.ExtractToFile(destUserCfgPath, overwrite: true);
		});
	}
}

