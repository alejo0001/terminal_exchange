using CrmAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Presentation.Services;

public class AppSettingsService : IAppSettingsService
{
    private readonly IConfiguration _configuration;

    public AppSettingsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
        
    public string this[string key]
    {
        get => _configuration[key];
    }
}