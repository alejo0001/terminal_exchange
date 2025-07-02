using System;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class AnnotationDto : IMapFrom<Annotation>
{
    public int Id { get; set; }
    public DateTime Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public string LastModifiedBy { get; set; }
    public int ContactId { get; set; }
    public string Comment { get; set; }
    public string Title { get; set; }
    public string LastEditor  { get; set; }
    public int UserId { get; set; }
    public UserDto User { get; set; }
    public bool Mandatory { get; set; }
    public bool IsPrivate { get; set; }
}