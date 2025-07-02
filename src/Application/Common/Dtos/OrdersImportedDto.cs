using System;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class OrdersImportedDto : IMapFrom<IntranetMigrator.Domain.Entities.OrdersImported>
{
    public int Id { get; set; }
    public int? OrderNumber { get; set; }
    public int? CourseId { get; set; }
    public string? StudentNif { get; set; }
    public string? StudentName { get; set; }
    public string? StudentSurName { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PostsalCode { get; set; }
    public string? Province { get; set; }
    public string? Town { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? Observations { get; set; }
    public string? Title { get; set; }
    public int? Gender { get; set; }
    public string? WebUrl { get; set; }
    public string? Discount { get; set; }
    public float? AmountBase { get; set; }
    public float? AmountFinal { get; set; }
    public float? AmountRegistration { get; set; }
    public float? AmountDeadLines { get; set; }
    public int? NumberDeadLines { get; set; }
    public float? FirstPaymentInEuro { get; set; }
    public string? Nationality { get; set; }
    public string? Occupation { get; set; }
    public string? AcademicTitle { get; set; }
    public string? University { get; set; }
    public string? Teleoperator { get; set; }
    public string? RegistrationAgain { get; set; }
    public string? Enterprise { get; set; }
    public DateTime? InitDate { get; set; }
    public int? DurationCourseInDays { get; set; }
    public string? Area { get; set; }
    public string? SalesCountry { get; set; }
    public string? CurrencyCountry { get; set; }
    public int? EndorsementPersonId { get; set; }
    public int? StudentId { get; set; }
    public int? Intensive { get; set; }
    public string? CourseCode { get; set; }
    public string? Study { get; set; }
    public string? ProgramType { get; set; }
    public int? InvoiceNumber { get; set; }
    public int? TeamId { get; set; }
    public int? OrderOriginNumber { get; set; }
    public int? ClientNotificationSent { get; set; }
    public int? IsEnrollmentUpload { get; set; }
    public int? OrderHeaderId { get; set; }
    public int? InvoiceHeaderId { get; set; }
    public int? NewStudentId { get; set; }
    public int? NewCustomerId { get; set; }
    public int? ProcessId { get; set; }
    public int? ActionId { get; set; }
    public int? ContactId { get; set; }
    public int? CountryId { get; set; }
    public string AffiliateCode { get; set; }
    public bool ImportedFromTlmk { get; set; }
    public string PaymentType { get; set; }
    public CountryDto BusinessCountry { get; set; }
    public CountryDto CurrencySaleCountry { get; set; }
    public ProcessChildDto Process { get; set; }
    public ActionChildDto Action { get; set; }
    public ContactChildDto Contact { get; set; }

    public void Mapping(MappingProfile profile)
    {
        profile.CreateMap<IntranetMigrator.Domain.Entities.OrdersImported, OrdersImportedDto>().ReverseMap();
    }
}