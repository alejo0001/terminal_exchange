using System;
using System.IO;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Infrastructure.Emails.Models;

public class AttachElment : IMapFrom<Attachment>
{
    public string fileName { get; set; }
    public string dataBase64 { get; set; }
        
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Attachment, AttachElment>()
            .ForMember(d => d.dataBase64, opt =>
                opt.MapFrom(dto => Convert.ToBase64String(File.ReadAllBytes(dto.Path))));
    }
}