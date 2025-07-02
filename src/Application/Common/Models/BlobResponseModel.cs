namespace CrmAPI.Application.Common.Models;

public class BlobResponseModel
{
    public string? Status { get; set; }
    public bool Error { get; set; }
    public BlobModel Blob { get; set; }

    public BlobResponseModel()
    {
        Blob = new BlobModel();
    }
}