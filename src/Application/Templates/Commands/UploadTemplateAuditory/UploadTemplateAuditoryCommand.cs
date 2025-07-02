using System;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using CrmAPI.Application.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CrmAPI.Application.Templates.Commands.UploadTemplateAuditory;

[Authorize(Roles = "Auditor")]
public class UploadTemplateAuditoryCommand : IRequest<Unit>
{
    public required IFormFile File { get; set; }
    public  DateTime? Date { get; set; }
}

[UsedImplicitly]
public class UploadTemplateAuditoryCommandHandler : IRequestHandler<UploadTemplateAuditoryCommand, Unit>
{

    private readonly IBlobStorageService _blobStorageService;
    private readonly string _baseUriAuditory;
    private readonly string _containerNameAuditory;
    private readonly string _accessTokenAuditory;
   
    public UploadTemplateAuditoryCommandHandler(
        IBlobStorageService blobStorageService,
        IOptionsSnapshot<DataBlobAuditoryConnectionSettings> options)
    {
        _blobStorageService = blobStorageService;
        var settings = options.Value;

        _baseUriAuditory = settings.BaseUriAuditory;
        _containerNameAuditory = settings.ContainerNameAuditory;
        _accessTokenAuditory = settings.AccessTokenAuditory;
    }
    
    /// <summary>
    /// Handles the process of uploading a file to blob storage for auditory purposes.
    /// It formats the provided date (or defaults to the next day if not provided),
    /// generates a filename by replacing "empty" in the original file name with the formatted date,
    /// and constructs a connection string for uploading the file to the blob storage.
    /// If the file has content, it uploads the file using the BlobStorageService. 
    /// </summary>
    /// <param name="request">The command containing the file and date</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>Returns a Unit indicating the task has been completed.</returns>
    public async Task<Unit> Handle(UploadTemplateAuditoryCommand request, CancellationToken ct)
    {
        var formattedDate = (request.Date ?? DateTime.Now.AddDays(1)).ToString("yyyy-MM-dd");
        var filename = request.File.FileName.Replace("empty", formattedDate);
        var connectionString = $"{_baseUriAuditory}/{_containerNameAuditory}/{filename}/{_accessTokenAuditory}";
        
        if (request.File is { Length: > 0 })
        {
            await _blobStorageService.UploadAsync(
                request.File.OpenReadStream(),
                filename, 
                connectionString, 
                _containerNameAuditory, 
                true);
        }

        return Unit.Value;
    }
    
}