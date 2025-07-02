namespace CrmAPI.Application.Common.Interfaces;

public interface IAppSettingsService
{
    string this[string key] { get; }
}