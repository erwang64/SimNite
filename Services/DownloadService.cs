using System.IO;
using System.Net.Http;
using SimNite.Services.Interfaces;

namespace SimNite.Services;

public class DownloadService : IDownloadService
{
	private static readonly HttpClient HttpClient = new();

	public async Task<string> DownloadFileAsync(
		string downloadUrl,
		string destinationDirectory,
		IProgress<double>? progress,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(downloadUrl))
		{
			throw new ArgumentException("Download URL cannot be empty.", nameof(downloadUrl));
		}

		if (string.IsNullOrWhiteSpace(destinationDirectory))
		{
			throw new ArgumentException("Destination directory cannot be empty.", nameof(destinationDirectory));
		}

		try
		{
			Directory.CreateDirectory(destinationDirectory);

			using var response = await HttpClient.GetAsync(
				downloadUrl,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken);
			response.EnsureSuccessStatusCode();

			var contentLength = response.Content.Headers.ContentLength;
			var fileName = ResolveFileName(response, downloadUrl);
			var destinationPath = Path.Combine(destinationDirectory, fileName);

			await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
			await using var destinationStream = new FileStream(
				destinationPath,
				FileMode.Create,
				FileAccess.Write,
				FileShare.None,
				bufferSize: 81920,
				useAsync: true);

			var buffer = new byte[81920];
			long totalRead = 0;
			int bytesRead;

			while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken)) > 0)
			{
				await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
				totalRead += bytesRead;

				if (contentLength.HasValue && contentLength.Value > 0)
				{
					var percent = (double)totalRead / contentLength.Value * 100d;
					progress?.Report(percent);
				}
			}

			if (!contentLength.HasValue)
			{
				progress?.Report(100d);
			}

			return destinationPath;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException($"Download failed for '{downloadUrl}'.", ex);
		}
	}

	private static string ResolveFileName(HttpResponseMessage response, string downloadUrl)
	{
		var headerName = response.Content.Headers.ContentDisposition?.FileNameStar
						 ?? response.Content.Headers.ContentDisposition?.FileName;
		if (!string.IsNullOrWhiteSpace(headerName))
		{
			return headerName.Trim('"');
		}

		if (Uri.TryCreate(downloadUrl, UriKind.Absolute, out var uri))
		{
			var uriFileName = Path.GetFileName(uri.LocalPath);
			if (!string.IsNullOrWhiteSpace(uriFileName))
			{
				return uriFileName;
			}
		}

		return $"download-{Guid.NewGuid():N}.bin";
	}
}
