using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;

namespace CrmAPI.Application.Common.Dtos;

public class PedidoTlmkDto : IMapFrom<IntranetMigrator.Domain.Entities.OrdersImported>
{
    public int NumPedido { get; set; }
    public double? NumPedidoAnterior { get; set; }
    public int? NumPedidoOriginal { get; set; }
    public int IdCurso { get; set; }
    public int IdWeb { get; set; }
    public string Nif { get; set; }
    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Direccion { get; set; }
    public string Codpos { get; set; }
    public string Provincia { get; set; }
    public string Pobl { get; set; }
    public string Pais { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public DateTime? FechaPedido { get; set; }
    public string TipoPago { get; set; }
    public string Observaciones { get; set; }
    public string Titulo { get; set; }
    public int? Unidades { get; set; }
    public float? Precio { get; set; }
    public int Sexo { get; set; }
    public string Web { get; set; }
    public string Descuento { get; set; }
    public float? PrecioFinal { get; set; }
    public float? PrecioMatricula { get; set; }
    public float? PrecioPlazos { get; set; }
    public int? Nplazos { get; set; }
    public string Nacionalidad { get; set; }
    public string Profesion { get; set; }
    public string Titulacion { get; set; }
    public string Universidad { get; set; }
    public string Teleoperadora { get; set; }
    public string Ntarjeta { get; set; }
    public string TokenPedido { get; set; }
    public string Ncuenta { get; set; }
    public string Rematricula { get; set; }
    public string Empresa { get; set; }
    public string RefRedsys { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public DateTime? FechaInicio { get; set; }
    public string Area { get; set; }
    public string PaisVenta { get; set; }
    public string PaisMoneda { get; set; }
    public int? IdAvalista { get; set; }
    public int? IdStudent { get; set; }
    public bool Intensive { get; set; }
    public string AffiliateCode { get; set; }
    public float? AffiliateComissionPercent { get; set; }
    public string PromotionalCode { get; set; }
    public string CourseCode { get; set; }
    public string Study { get; set; }
    public string ProgramType { get; set; }
    public string CodigoPedidoRedsys { get; set; }
    public float ImporteCobroRedsys { get; set; }
    public string PlataformaPago { get; set; }
    public string Duracion { get; set; }
    public DateTime? FechaFin { get; set; }
    public float Creditos { get; set; }
    public float? PrimerPagoEUR { get; set; }
    public float? PrimerPago { get; set; }
    public string? DivisaPrimerPago { get; set; }
    public decimal? Ratio { get; set; }
    public int? IdFactura { get; set; }
    public bool? PoliticasPrivacidadAceptadas { get; set; }
    public bool? CondicionesContratacionAceptadas { get; set; }
    public bool IsRenewal { get; set; }
    public float? ImporteRenovacion { get; set; }
    public int? NumPedidoOrigen { get; set; }
    public bool? ClientNotificationsSent { get; set; }
    public bool? IsEnrollmentUpload { get; set; }
    public int DurationCourseInDays { get; set; }
    public int? IdPago { get; set; }
    public int? IdIdioma { get; set; }
    public string? Idioma { get; set; }
    public string? TituloIdioma { get; set; }
    public string? AreaIdioma { get; set; }
    public string? EstudioIdioma { get; set; }
    public int? ProcessId { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<PedidoTlmkDto, IntranetMigrator.Domain.Entities.OrdersImported>()
            .ForMember(d => d.OrderNumber, opt =>
                opt.MapFrom(dom => dom.NumPedido))
            .ForMember(d => d.OrderNumberOld, opt =>
                opt.MapFrom(dom => dom.NumPedidoAnterior))
            .ForMember(d => d.OrderNumberOriginal, opt =>
                opt.MapFrom(dom => dom.NumPedidoOriginal))
            .ForMember(d => d.CourseId, opt =>
                opt.MapFrom(dom => dom.IdCurso))
            .ForMember(d => d.WebId, opt =>
                opt.MapFrom(dom => dom.IdWeb))
            .ForMember(d => d.StudentNif, opt =>
                opt.MapFrom(dom => dom.Nif))
            .ForMember(d => d.StudentName, opt =>
                opt.MapFrom(dom => dom.Nombre))
            .ForMember(d => d.StudentSurName, opt =>
                opt.MapFrom(dom => dom.Apellidos))
            .ForMember(d => d.Address, opt =>
                opt.MapFrom(dom => dom.Direccion))
            .ForMember(d => d.BirthDate, opt =>
                opt.MapFrom(dom => dom.FechaNacimiento))
            .ForMember(d => d.PostalCode, opt =>
                opt.MapFrom(dom => dom.Codpos))
            .ForMember(d => d.Province, opt =>
                opt.MapFrom(dom => dom.Provincia))
            .ForMember(d => d.Town, opt =>
                opt.MapFrom(dom => dom.Pobl))
            .ForMember(d => d.Country, opt =>
                opt.MapFrom(dom => dom.Pais))
            .ForMember(d => d.Phone, opt =>
                opt.MapFrom(dom => dom.Telefono))
            .ForMember(d => d.Email, opt =>
                opt.MapFrom(dom => dom.Email))
            .ForMember(d => d.OrderDate, opt =>
                opt.MapFrom(dom => dom.FechaPedido))
            .ForMember(d => d.PaymentType, opt =>
                opt.MapFrom(dom => dom.TipoPago))
            .ForMember(d => d.Observations, opt =>
                opt.MapFrom(dom => dom.Observaciones))
            .ForMember(d => d.Title, opt =>
                opt.MapFrom(dom => dom.Titulo))
            .ForMember(d => d.Quantity, opt =>
                opt.MapFrom(dom => dom.Unidades))
            .ForMember(d => d.AmountBase, opt =>
                opt.MapFrom(dom => dom.Precio))
            .ForMember(d => d.Gender, opt =>
                opt.MapFrom(dom => dom.Sexo))
            .ForMember(d => d.WebUrl, opt =>
                opt.MapFrom(dom => dom.Web))
            .ForMember(d => d.Discount, opt =>
                opt.MapFrom(dom => dom.Descuento))
            .ForMember(d => d.AmountFinal, opt =>
                opt.MapFrom(dom => dom.PrecioFinal))
            .ForMember(d => d.AmountRegistration, opt =>
                opt.MapFrom(dom => dom.PrecioMatricula))
            .ForMember(d => d.AmountDeadLines, opt =>
                opt.MapFrom(dom => dom.PrecioPlazos))
            .ForMember(d => d.NumberDeadLines, opt =>
                opt.MapFrom(dom => dom.Nplazos))
            .ForMember(d => d.Nationality, opt =>
                opt.MapFrom(dom => dom.Nacionalidad))
            .ForMember(d => d.Occupation, opt =>
                opt.MapFrom(dom => dom.Profesion))
            .ForMember(d => d.AcademicTitle, opt =>
                opt.MapFrom(dom => dom.Titulacion))
            .ForMember(d => d.University, opt =>
                opt.MapFrom(dom => dom.Universidad))
            .ForMember(d => d.Teleoperator, opt =>
                opt.MapFrom(dom => dom.Teleoperadora))
            .ForMember(d => d.CreditCardNumber, opt =>
                opt.MapFrom(dom => dom.Ntarjeta))
            .ForMember(d => d.BankAccountNumber, opt =>
                opt.MapFrom(dom => dom.Ncuenta))
            .ForMember(d => d.RegistrationAgain, opt =>
                opt.MapFrom(dom => dom.Rematricula))
            .ForMember(d => d.Enterprise, opt =>
                opt.MapFrom(dom => dom.Empresa))
            .ForMember(d => d.RedSysReferential, opt =>
                opt.MapFrom(dom => dom.RefRedsys))
            .ForMember(d => d.InitDate, opt =>
                opt.MapFrom(dom => dom.FechaInicio))
            .ForMember(d => d.DurationCourseInDays, opt =>
                opt.MapFrom(dom => dom.DurationCourseInDays))
            .ForMember(d => d.Area, opt =>
                opt.MapFrom(dom => dom.Area))
            .ForMember(d => d.SalesCountry, opt =>
                opt.MapFrom(dom => dom.PaisVenta))
            .ForMember(d => d.CurrencyCountry, opt =>
                opt.MapFrom(dom => dom.PaisMoneda))
            .ForMember(d => d.EndorsementPersonId, opt =>
                opt.MapFrom(dom => dom.IdAvalista))
            .ForMember(d => d.StudentId, opt =>
                opt.MapFrom(dom => dom.IdStudent))
            .ForMember(d => d.PromotionalCode, opt =>
                opt.MapFrom(dom => dom.PromotionalCode))
            .ForMember(d => d.AffiliateCode, opt =>
                opt.MapFrom(dom => dom.AffiliateCode))
            .ForMember(d => d.Intensive, opt =>
                opt.MapFrom(dom => dom.Intensive))
            .ForMember(d => d.CourseCode, opt =>
                opt.MapFrom(dom => dom.CourseCode))
            .ForMember(d => d.Study, opt =>
                opt.MapFrom(dom => dom.Study))
            .ForMember(d => d.ProgramType, opt =>
                opt.MapFrom(dom => dom.ProgramType))
            .ForMember(d => d.OrderToken, opt =>
                opt.MapFrom(dom => dom.TokenPedido))
            .ForMember(d => d.PrivacyPoliciesAccepted, opt =>
                opt.MapFrom(dom => dom.PoliticasPrivacidadAceptadas))
            .ForMember(d => d.AcceptedContractingConditions, opt =>
                opt.MapFrom(dom => dom.CondicionesContratacionAceptadas))
            .ForMember(d => d.ConversionRatioBetweenCountry, opt =>
                opt.MapFrom(dom => dom.Ratio))
            .ForMember(d => d.ConversionRatioEur, opt =>
                opt.Ignore())
            .ForMember(d => d.FirstPaymentInEuro, opt =>
                opt.MapFrom(dom => dom.PrimerPagoEUR))
            .ForMember(d => d.InvoiceNumber, opt =>
                opt.MapFrom(dom => dom.IdFactura))
            .ForMember(d => d.TeamId, opt =>
                opt.Ignore())
            .ForMember(d => d.AmountRenovation, opt =>
                opt.MapFrom(dom => dom.ImporteRenovacion))
            .ForMember(d => d.OrderOriginNumber, opt =>
                opt.MapFrom(dom => dom.NumPedidoOrigen))
            .ForMember(d => d.ClientNotificationSent, opt =>
                opt.MapFrom(dom => dom.ClientNotificationsSent))
            .ForMember(d => d.AmountApostille, opt =>
                opt.MapFrom(dom => dom.ImporteRenovacion))
            .ForMember(d => d.IsEnrollmentUpload, opt =>
                opt.MapFrom(dom => dom.IsEnrollmentUpload));


    }
}