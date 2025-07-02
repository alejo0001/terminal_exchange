using System;
using System.IO;
using CrmAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CrmAPI.Infrastructure.Files;

public class FileService : IFileService
{
    private readonly ILogger<FileService> _logger;
    private readonly IDateTime _dateTime;

    public FileService(ILogger<FileService> logger, IDateTime dateTime)
    {
        _logger = logger;
        _dateTime = dateTime;
    }

    public string GetSavePath(string fileName)
    {
        string path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar
                                                      + "wwwroot" + Path.DirectorySeparatorChar + "files";
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
            
        path += Path.DirectorySeparatorChar + _dateTime.Now.ToString("yyyyMMddHHmmss") + "_" + fileName;

        return path;
    }

    public string SaveFile(string fileName, string base64)
    {
        string path = "";
        try
        {
            path = GetSavePath(fileName);
            File.WriteAllBytes(path, Convert.FromBase64String(base64));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return path;
    }
        
    public bool DeleteFile(string path)
    {
        bool deleted = true;
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            deleted = false;
        }
        return deleted;
    }
}