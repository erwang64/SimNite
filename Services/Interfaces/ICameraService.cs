using SimNite.Models;
using System.Threading.Tasks;

namespace SimNite.Services.Interfaces;

public interface ICameraService
{
	string? DetectSimObjectsPath();
	Task BackupCamerasAsync(string sourcePath, string outputZipFilePath);
	Task RestoreCamerasAsync(string inputZipFilePath, string destPath);
	int GetCustomCamerasCount(string sourcePath);
}