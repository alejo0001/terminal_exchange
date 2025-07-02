using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Models;

namespace CrmAPI.Application.Common.Interfaces;

public interface IBlobStorageService
{    
    Task<string> GetBlobContentAsync(string blobName, string connectionString, string containerName, CancellationToken cancellationToken);
    public Task<BlobResponseModel> UploadAsync(
        Stream blob,
        string fileName,
        string connectionString,
        string containerName,
        bool overwrite = false);
}
