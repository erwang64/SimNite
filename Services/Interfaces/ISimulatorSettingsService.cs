using System.Threading.Tasks;

namespace SimNite.Services.Interfaces;

public interface ISimulatorSettingsService
{
	/// <summary>
	/// Retourne le chemin vers le fichier MSFS `UserCfg.opt` s'il est détecté.
	/// </summary>
	string? DetectUserCfgPath();

	Task BackupUserCfgAsync(string userCfgPath, string outputZipFilePath);

	Task RestoreUserCfgAsync(string inputZipFilePath, string destUserCfgPath);
}

