using System;

namespace Domain.Tlmk.Entities
{
    public class PedidoTlmk
    {
        public int NumPedido { get; set; }
        public string Nombre { get; set; }
        public double? NumPedidoAnterior { get; set; }
        public int? NumPedidoOriginal { get; set; }
        public string? Nif { get; set; }
        public string? Apellidos { get; set; }
        public string? Direccion { get; set; }
        public string? Codpos { get; set; }
        public string? Provincia { get; set; }
        public string? Pobl { get; set; }
        public string? Pais { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime? FechaPedido { get; set; }
        public string? TipoPago { get; set; }
        public string? Observaciones { get; set; }
        public string? Titulo { get; set; }
        public int? Unidades { get; set; }
        public float? Precio { get; set; }
        public int? Sexo { get; set; }
        public int? Id_Curso { get; set; }
        public string? Web { get; set; }
        public string? Descuento { get; set; }
        public float? Precio_Final { get; set; }
        public float? Precio_Matricula { get; set; }
        public float? Precio_Plazos { get; set; }
        public int? Nplazos { get; set; }
        public string? Nacionalidad { get; set; }
        public string? Profesion { get; set; }
        public string? Titulacion { get; set; }
        public string? Universidad { get; set; }
        public string? Teleoperadora { get; set; }
        public string? Ntarjeta { get; set; }
        public string? TokenPedido { get; set; }
        public string? Ncuenta { get; set; }
        public string? Rematricula { get; set; }
        public string? Empresa { get; set; }
        public string? Ref_Redsys { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public DateTime? FechaInicio { get; set; }
        public string? Area { get; set; }
        public string? Pais_venta { get; set; }
        public string? Pais_moneda { get; set; }
        public int? IdStudent { get; set; }
        public bool? Intensive { get; set; }
        public string? AffiliateCode { get; set; }
        public float? AffiliateComissionPercent { get; set; }
        public string? PromotionalCode { get; set; }
        public string? CourseCode { get; set; }
        public string? Study { get; set; }
        public string? ProgramType { get; set; }
        public float? PrimerPagoEUR { get; set; }
        public float? PrimerPago { get; set; }
        public string? DivisaPrimerPago { get; set; }
        public decimal? Ratio { get; set; }
        public int? IdFactura { get; set; }
        public bool? PoliticasPrivacidadAceptadas { get; set; }
        public bool? CondicionesContratacionAceptadas { get; set; }
        public float? ImporteRenovacion { get; set; }
        public int? NumPedidoOrigen { get; set; }
        public bool? ClientNotificationsSent { get; set; }
        public bool? IsEnrollmentUpload { get; set; }
        public int? DurationCourseInDays { get; set; }
        public string? Idioma { get; set; }
        public string? Titulo_Idioma { get; set; }
        public int? Id_avalista { get; set; }
        public int? IdEquipo { get; set; }
    }
}