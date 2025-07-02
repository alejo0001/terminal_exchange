using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Interfaces;

public interface ITranslationsService
{
    string translateVariablesFromTemplate(string template, Contact contact, string languageCode, CancellationToken cancellationToken);
    Task GetTranslationsDictionary(string languageCode, CancellationToken cancellationToken);
    string GetWordTranslation(string key, string languageCode);
}