using System;
using System.Linq;

namespace CrmAPI.Application.UtcTimeZone.Queries
{
    public class GetUtcTimeZoneQueryValidation
    {
        public static void ValidateCountryIso(string countryIso)
        {
            if (string.IsNullOrWhiteSpace(countryIso) || countryIso.Length < 2)
            {
                throw new ArgumentException("El código ISO del país debe contener al menos 2 caracteres.");
            }

            if (countryIso.Any(char.IsDigit))
            {
                throw new ArgumentException("El código ISO del país no puede contener números.");
            }
        }
    }
}
