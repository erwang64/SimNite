using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class CameraService : ICameraService
{
	public string? DetectSimObjectsPath()
	{
		var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		var candidates = new[]
		{
			Path.Combine(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache", "SimObjects"),
			Path.Combine(roamingAppData, "Microsoft Flight Simulator", "SimObjects"),
			Path.Combine(localAppData, "Packages", "Microsoft.Limitless_8wekyb3d8bbwe", "LocalCache", "SimObjects")
		};

		return candidates.FirstOrDefault(Directory.Exists);
	}

	public int GetCustomCamerasCount(string sourcePath)
	{
		if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath)) return 0;
		
		try
		{
			return Directory.GetFiles(sourcePath, "cameras.CFG", SearchOption.AllDirectories).Length;
		}
		catch
		{
			return 0;
		}
	}

	public async Task BackupCamerasAsync(string sourcePath, string outputZipFilePath)
	{
		if (string.IsNullOrWhiteSpace(sourcePath) || !Directory.Exists(sourcePath)) 
			throw new DirectoryNotFoundException("SimObjects path not found.");
			
		var directory = Path.GetDirectoryName(outputZipFilePath);
		if (!string.IsNullOrWhiteSpace(directory))
		{
			Directory.CreateDirectory(directory);
		}

		// Supprimer l'ancienne sauvegarde si elle existe
		if (File.Exists(outputZipFilePath))
		{
			File.Delete(outputZipFilePath);
		}
		
		await Task.Run(() => 
		{
			// Compresse tout le dossier SimObjects (qui contient Airplanes, Rotorcraft, etc.)
			ZipFile.CreateFromDirectory(sourcePath, outputZipFilePath, CompressionLevel.Optimal, includeBaseDirectory: false);
		});
	}

	public async Task RestoreCamerasAsync(string inputZipFilePath, string destPath)
	{
		if (!File.Exists(inputZipFilePath)) 
			throw new FileNotFoundException("Backup ZIP file not found.", inputZipFilePath);
		
		if (string.IsNullOrWhiteSpace(destPath)) 
			throw new DirectoryNotFoundException("Destination SimObjects path not valid.");
			
		Directory.CreateDirectory(destPath);
		
		await Task.Run(() => 
		{
			// Écrase les fichiers existants par ceux de l'archive
			ZipFile.ExtractToDirectory(inputZipFilePath, destPath, overwriteFiles: true);
		});
	}
}