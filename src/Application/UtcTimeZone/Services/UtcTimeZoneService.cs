using System;
using System.Linq;
using System.Threading;
using CrmAPI.Application.UtcTimeZone.Queries;
using NodaTime;
using NodaTime.TimeZones;
using ArgumentException = System.ArgumentException;

namespace CrmAPI.Application.UtcTimeZone.Services;

public class UtcTimeZoneService
{
    public DateTime ConvertUtcToCountryTimeAsync(DateTime DatetimeFrom, string destinationCountryIso, CancellationToken ct)
    {
        GetUtcTimeZoneQueryValidation.ValidateCountryIso(destinationCountryIso);

        var utcDateTime = DatetimeFrom.ToUniversalTime();
        var instant = Instant.FromDateTimeUtc(utcDateTime);
        var normalizedCountryIso = destinationCountryIso.ToUpperInvariant();

        var countryZones = TzdbDateTimeZoneSource.Default.ZoneLocations
            .Where(z => z.CountryCode.ToUpperInvariant() == normalizedCountryIso)
            .ToList();

        if (!countryZones.Any())
        {
            throw new ArgumentException($"No se encontraron zonas horarias para el código ISO del país: {destinationCountryIso}");
        }

        var destinationZone = DateTimeZoneProviders.Tzdb[countryZones.First().ZoneId];
        var localDateTime = instant.InZone(destinationZone).ToDateTimeUnspecified();

        return localDateTime;
    }
}