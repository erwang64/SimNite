namespace SimNite.Services.Interfaces;

public interface IDownloadService
{
	Task<string> DownloadFileAsync(
		string downloadUrl,
		string destinationDirectory,
		IProgress<double>? progress,
		CancellationToken cancellationToken);
}

