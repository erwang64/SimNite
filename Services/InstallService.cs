using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

			bool isZip = IsZipArchive(installerPath);
			string exeToLaunch = installerPath;
			string? tempExtractPath = null;

			try
			{
				if (isZip)
				{
					tempExtractPath = Path.Combine(Path.GetTempPath(), "SimNite", ".tmp_install", Guid.NewGuid().ToString("N"));
					Directory.CreateDirectory(tempExtractPath);

					try
					{
						var di = new DirectoryInfo(tempExtractPath);
						di.Attributes |= FileAttributes.Hidden;
					}
					catch
					{
						// Si on ne peut pas marquer "hidden" (droits/FS), on continue quand même.
					}

					// Extraire en arrière-plan pour ne pas bloquer l'UI
					await Task.Run(
						() =>
						{
							cancellationToken.ThrowIfCancellationRequested();
							ZipFile.ExtractToDirectory(installerPath, tempExtractPath, overwriteFiles: true);
						},
						cancellationToken);

					// Chercher le .exe récursivement
					var exeFiles = Directory.GetFiles(tempExtractPath, "*.exe", SearchOption.AllDirectories);
					if (exeFiles.Length == 0)
					{
						throw new FileNotFoundException("L'archive ne contient aucun fichier .exe installable.", tempExtractPath);
					}

					// Priorité aux exécutables courants (setup/install), sinon premier trouvé
					exeToLaunch =
						exeFiles.FirstOrDefault(f =>
							f.EndsWith($"{Path.DirectorySeparatorChar}setup.exe", StringComparison.OrdinalIgnoreCase) ||
							f.EndsWith($"{Path.DirectorySeparatorChar}install.exe", StringComparison.OrdinalIgnoreCase))
						?? exeFiles[0];
				}

				var startInfo = new ProcessStartInfo
				{
					FileName = exeToLaunch,
					UseShellExecute = true,
					WorkingDirectory = Path.GetDirectoryName(exeToLaunch) ?? string.Empty,
				};

				var process = Process.Start(startInfo)
					?? throw new InvalidOperationException($"Unable to start installer '{exeToLaunch}'.");

				await process.WaitForExitAsync(cancellationToken);
			}
			finally
			{
				// Nettoyage silencieux du dossier temporaire (succès, erreur, annulation)
				if (tempExtractPath != null && Directory.Exists(tempExtractPath))
				{
					try { Directory.Delete(tempExtractPath, recursive: true); } catch { /* best-effort */ }
				}
			}
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

	private static bool IsZipArchive(string filePath)
	{
		// 1) Extension .zip → oui
		if (filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}

		// 2) Signature "PK" (zip) → oui (même sans extension)
		try
		{
			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (fs.Length < 4)
			{
				return false;
			}

			Span<byte> header = stackalloc byte[4];
			int read = fs.Read(header);
			if (read < 4)
			{
				return false;
			}

			// PK\003\004 (local file header), PK\005\006 (empty archive), PK\007\008 (spanned)
			return header[0] == (byte)'P'
				&& header[1] == (byte)'K'
				&& (
					(header[2] == 3 && header[3] == 4) ||
					(header[2] == 5 && header[3] == 6) ||
					(header[2] == 7 && header[3] == 8)
				);
		}
		catch
		{
			return false;
		}
	}
}
