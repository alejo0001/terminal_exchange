using System;

namespace CrmAPI.Domain.Leads.Entities;

public class Lead
{
    public int id {get;set;}
    public string? nombre {get;set;}
    public string? apellidos {get;set;}
    public string? email {get;set;}
    public string? telefonos {get;set;}
    public string? telefonos_viejos {get;set;}
    public string? pais {get;set;}
    public string? pais_viejo {get;set;}
    public string? programas {get;set;}
    public string? facultades {get;set;}
    public string? especialidades {get;set;}
    public DateTime? fecha {get;set;}
    public string? estado {get;set;}
    public string? tabla_procedencia {get;set;}
    public string? id_procedencia {get;set;}
    public string? Profession {get;set;}
    public string? Careers {get;set;}
    public string? especialidad1 {get;set;}
    public string? nombre_listado {get;set;}
    public int? id_listado {get;set;}
    public string? tags { get; set; }
    public DateTime? fecha_reparto { get; set; }
    public int filterid { get; set; }
    public string? facultad1 { get; set; }
    public string? estado1 { get; set; }
    public string? zona {get;set;}
    public string? cursos_recomendados {get;set;}
    public string? doc_identidad {get;set;}
    public string? nacionalidad {get;set;}
    public string? genero {get;set;}
    public string? cod_postal {get;set;}
    public string? poblacion {get;set;}
    public string? direccion {get;set;}
    public string? titulacion {get;set;}
    public string? universidad {get;set;}
    public string? provincia {get;set;}
    public string? idiomas {get;set;}
    public DateTime? fecha_ultimaventa {get;set;}
    public DateTime? fecha_proximoContacto {get;set;}
    public DateTime? fecha_rematricula {get;set;}
}