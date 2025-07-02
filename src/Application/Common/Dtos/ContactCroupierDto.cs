using System;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using CroupierAPI.Contracts.Events;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Dtos;

public class ContactCroupierDto : IMapFrom<Contact>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string FirstSurName { get; set; }
    public string SecondSurName { get; set; }
    public Guid Guid { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactCroupierDto, ContactGetted>().ReverseMap();
    }
    
    
}