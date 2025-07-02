using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using Newtonsoft.Json;

namespace CrmAPI.Infrastructure.Services;

public class TranslationsService : ITranslationsService
{
    private readonly ITranslationApiClient _apiClient;
    private readonly Dictionary<string, Dictionary<string, object>> _translates = new();

    public TranslationsService(ITranslationApiClient apiClient)
    {
        _apiClient = apiClient;

        // Inicialización de los diccionarios de cada idioma
        _translates["es"] = new Dictionary<string, object>();
        _translates["en"] = new Dictionary<string, object>();
        _translates["it"] = new Dictionary<string, object>();
        _translates["pt"] = new Dictionary<string, object>();
        _translates["fr"] = new Dictionary<string, object>();
        _translates["de"] = new Dictionary<string, object>();
    }

    public async Task GetTranslationsDictionary(string languageCode, CancellationToken cancellationToken)
    {
        if (_translates[languageCode.ToLower()].Count > 0)
        {
            return;
        }

        var response = await _apiClient.GetForModuleAndLanguage(languageCode, cancellationToken)
            .ConfigureAwait(false);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        _translates[languageCode.ToLower()] = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
    }

    public string translateVariablesFromTemplate(string template, Contact contact, string languageCode, CancellationToken cancellationToken)
    {

        // SALUDO DE BUENOS DÍAS
        if (template.Contains("$GREETINGS$"))
        {
            int hour = DateTime.Now.Hour;
            string greetings;

            if (hour is >= 1 and < 12)
            {
                greetings = GetWordTranslation("greetings.goodMorning", languageCode);
            }
            else if (hour is >= 12 and < 20)
            {
                greetings = GetWordTranslation("greetings.goodAfternoon", languageCode);
            }
            else
            {
                greetings = GetWordTranslation("greetings.goodEvening", languageCode);
            }
            greetings = char.ToUpper(greetings[0]) + greetings.Substring(1);
            template = template.Replace("$GREETINGS$", greetings);
        }


        // TRANSFORMAMOS EL ENUM EN MINÚSCULA EL PRIMER CARACTER PARA PODER HACER LA TRADDUCCIÓN
        if (template.Contains("$CONTACT_COURTESY_TITLE$"))
        {
            var title = contact.Title.ToString();
            if (title is not null && !title.Equals(""))
            {
                title = char.ToLower(title[0]) + title.Substring(1);
            }
            else
            {
                title = "";
            }

            // TRADUCCIÓN DEL GÉNERO
            var gender = contact.Gender.Label;
            gender = char.ToLower(gender[0]) + gender.Substring(1);


            // CORTESÍA DEL CLIENTE (SR, SRA, DC, DTRA)
            if (gender != "" && title != "")
            {
                var courtesyTitle = GetWordTranslation("courtesyTitlesAbbreviation." + gender + "." + title, languageCode);
                template = template.Replace("$CONTACT_COURTESY_TITLE$", courtesyTitle);
            }
            else
            {
                template = template.Replace("$CONTACT_COURTESY_TITLE$", "");
            }
        }

        if (template.Contains("$CONTACT_FULL_NAME$"))
        {
            // NOMBRE DEL CONTACTO
            template = template.Replace("$CONTACT_FULL_NAME$", contact.Name + " " + contact.FirstSurName +
                                                               (contact.SecondSurName != "" ? " " + contact.SecondSurName : ""));
        }

        return CleanWhiteSpaces(template);
    }

    private string CleanWhiteSpaces(string template)
    {
        template = Regex.Replace(template, "&nbsp;", " ");
        template = Regex.Replace(template, @"<i>(?:\s+|&nbsp;)*<\/i>", "");
        template = Regex.Replace(template, @"\s+", " ");

        return template.Trim();
    }

    public string GetWordTranslation(string key, string languageCode)
    {
        var result = "";
        var translationsDictionary = _translates[languageCode.ToLower()];

        if (translationsDictionary.Count <= 0)
        {
            return result;
        }

        var keyArray = key.Split(".");

        try
        {
            foreach (var item in keyArray)
            {
                if (!item.Equals(keyArray.Last()))
                {
                    translationsDictionary = JsonConvert.DeserializeObject<Dictionary<string,object>>(translationsDictionary[item].ToString());
                }
                else
                {
                    result = translationsDictionary[item].ToString();
                }
            }
        }
        catch (Exception e)
        {
            Console.Write(e);
            return keyArray.Last();
        }

        return result!;
    }
}
