using System.Collections.Generic;

namespace CrmAPI.Application.Common.Dtos;

public class ContactInProgressDto
{
    public string? Name { get; set; }
    public string? FirstSurname { get; set; }
    public string? SecondSurname { get; set; }
    public string? IdCard { get; set; }
    public string? Nationality { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WorkCenter { get; set; }
    public List<FacultiesInProgressDto>? Faculties { get; set; }
    public List<SpecialitiesInProgressDto>? Specialities { get; set; }
}