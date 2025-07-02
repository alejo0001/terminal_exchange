namespace CrmAPI.Application.Common.Interfaces;

public interface IFileService
{
    string SaveFile(string fileName, string base64);
    string GetSavePath(string fileName);
    bool DeleteFile(string path);
}