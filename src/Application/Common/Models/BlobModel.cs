using System.IO;

namespace CrmAPI.Application.Common.Models;

public class BlobModel
{
    
    public string? Uri { get; set; }
    public string? Name { get; set; }
    public string? ContentType { get; set; }
    public Stream? Content { get; set; }
}