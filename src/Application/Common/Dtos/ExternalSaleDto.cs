using System;

namespace CrmAPI.Application.Common.Dtos;

public class ExternalSaleDto
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    
    public string? StudentName { get; set; }  //  nombreApellidos
    public string? IdCard { get; set; }  //  nif
    public string? Email { get; set; }  //  email
    public string? Phone { get; set; }  //  telefono
    
    public string? Address { get; set; }  //  direccion
    public string? Country { get; set; }  //  pais
    public string? Province { get; set; }  //  estado
    public string? PostalCode { get; set; }  //  cp
    public string? City { get; set; }  //  ciudad
    
    public int OrderNumber { get; set; }  //  numPedido
    public DateTime? OrderDate { get; set; }  //  fechaPedido
    public string? AcademicTitle { get; set; }  //  titulo
    public DateTime? InitDate { get; set; }  //  fechaInicio
    public string? PaymentType { get; set; }  //  tipoPago
    public string? CurrencyCountry { get; set; }  //  currencyCode
    public int? NumberDeadLines { get; set; }  //  nplazos
    public string? SalesCountry { get; set; }  //  paisVenta
    public float? AmountRegistration { get; set; }  //  primerPago

}