using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using CroupierAPI.Contracts.Events;

namespace CrmAPI.Application.Common.Dtos;

public class ContactProcessDto: IMapFrom<ContactCreated>
{
    public int ContactId { get; set; }
    public int ProcessId { get; set; }
    
    
    

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ContactProcessDto, ContactCreated>().ReverseMap();
    }
}