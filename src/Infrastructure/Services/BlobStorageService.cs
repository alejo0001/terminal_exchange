using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Models;
using System.Threading;
using System.Text;

namespace CrmAPI.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(ILogger<BlobStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<BlobResponseModel> UploadAsync(
        Stream blob,
        string fileName,
        string connectionString,
        string containerName,
        bool overwrite)
    {
        // Create new upload response object that we can return to the requesting method
        BlobResponseModel response = new();

        var container = new BlobContainerClient(connectionString, containerName);

        // await container.CreateAsync();
        try
        {
            // Get a reference to the blob just uploaded from the API in a container from configuration settings
            var client = container.GetBlobClient(fileName);

            // Upload the file async
            await client.UploadAsync(blob, overwrite);

            // Everything is OK and file got uploaded
            response.Status = $"File {fileName} Uploaded Successfully";
            response.Error = false;
            response.Blob.Uri = client.Uri.AbsoluteUri;
            response.Blob.Name = client.Name;
        }
        // If the file already exists, we catch the exception and do not upload it
        catch (RequestFailedException ex)
            when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
        {
            _logger.LogError(ex,
                "File with name {FileName} already exists in the container {ContainerName}. Please use a different name.",
                fileName, containerName);
            response.Status = $"File with name {fileName} already exists. Please use another name to store your file.";
            response.Error = true;
            return response;
        }
        // If we get an unexpected error, we catch it here and return the error message
        catch (RequestFailedException ex)
        {
            // Log error to console and create a new response we can return to the requesting method
            _logger.LogError(ex, "Unhandled exception occurred. Error Message: {ErrorMessage}, StackTrace ID: {StackTraceID}", ex.Message, ex.StackTrace);
            response.Status = "Unexpected error occurred. Check the logs for more details.";
            response.Error = true;
            return response;
        }

        // Return the BlobUploadResponse object
        return response;
    }

    public async Task<string> GetBlobContentAsync(string blobName, string connectionString, string containerName, CancellationToken ct)
    {
        var containerClient = new BlobContainerClient(connectionString, containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(ct))
        {
            return string.Empty;
        }

        var stream = await blobClient.OpenReadAsync(cancellationToken: ct);
        var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(ct);
    }
}
