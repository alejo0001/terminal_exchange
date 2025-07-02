using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using System;

namespace CrmAPI.Application.Common.Dtos;

public record  WebEnrollmentDto: IMapFrom<WebEnrollment>
{
    public Guid? Guid { get; set; }
    public Decimal? Price { get; set; }
    public string PriceFormatted { get; set; }
    public Decimal? FinalPrice { get; set; }
    public string FinalPriceFormatted { get; set; }
    public Decimal? DiscountPercentage { get; set; }
    public Decimal? EnrollmentPrice { get; set;  }
    public string EnrollmentPriceFormatted { get; set; }
    public int Fees { get; set;  }
    public Decimal? FeePrice { get; set;  }
    public string FeePriceFormatted { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public string IdCard { get; set; }
    public string PhonePrefix { get; set; }
    public string PhonePrefixCountryCode { get; set; }
    public string Phone { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string CountryCode { get; set; }
    public int ProcessId { get; set;  }
    public int ContactId { get; set;  }
    public int VendorId { get; set;  }
    public string CourseTypeBaseCode { get; set; }
    public string CourseCode { get; set; }
    public string LanguageCode { get; set; }
    public string ContactEmail { get; set; }
    public string? Token { get; set; }
    public DateTime? ConvocationDate { get; set; }
    public University University { get; set; }
    public string CurrencyCode { get; set; }
    public string CurrencySymbol { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<WebEnrollment, WebEnrollmentDto>().ReverseMap();
        profile.CreateMap<WebEnrollmentDto, CrmEnrollment>();
    }

}
