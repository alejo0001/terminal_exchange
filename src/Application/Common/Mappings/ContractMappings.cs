using AutoMapper;
using CrmAPI.Application.Annotations.Commands.CreateAnnotation;
using CrmAPI.Application.ContactLeads.Commands.CreateOrUpdateContactLead;
using CrmAPI.Application.Contacts.Commands.CreateContact;
using CrmAPI.Application.Contacts.Commands.CreateContactLead;
using CrmAPI.Application.Contacts.Commands.UpdateContact;
using CrmAPI.Application.Contacts.Commands.UpdateContactLead;
using CrmAPI.Application.Processes.Commands.CloseProcesses;
using CrmAPI.Application.Processes.Commands.CreateProcess;
using CroupierAPI.Contracts.Commands;
using CroupierAPI.Contracts.Dtos;
using CroupierAPI.Contracts.Enums;
using CroupierAPI.Contracts.Events;
using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application.Common.Mappings;

public class ContractMappings: Profile
{
    public ContractMappings()
    {
        CreateMap<IntranetMigrator.Domain.Enums.Colour, ProcessColour>().ReverseMap();
        CreateMap<IntranetMigrator.Domain.Enums.ProcessOrigin, ProcessOrigin>().ReverseMap();
        CreateMap<IntranetMigrator.Domain.Enums.ProcessOutcome, ProcessOutcome>().ReverseMap();
        CreateMap<IntranetMigrator.Domain.Enums.ProcessStatus, ProcessStatus>().ReverseMap();
        CreateMap<IntranetMigrator.Domain.Enums.ProcessType, ProcessType>().ReverseMap();
        CreateMap<IntranetMigrator.Domain.Enums.ContactLeadType, ContactLeadType>().ReverseMap();
        
        CreateMap<CrmAPI.Application.Common.Dtos.ContactPhoneUpdateDto, ContactPhoneUpdateDto>().ReverseMap();
        CreateMap<CrmAPI.Application.Common.Dtos.ContactEmailUpdateDto, ContactEmailUpdateDto>().ReverseMap();
        CreateMap<CrmAPI.Application.Common.Dtos.ContactAddressUpdateDto, ContactAddressUpdateDto>().ReverseMap();
        CreateMap<CrmAPI.Application.Common.Dtos.ContactTitleUpdateDto, ContactTitleUpdateDto>().ReverseMap();
        CreateMap<CrmAPI.Application.Common.Dtos.ContactLanguageUpdateDto, ContactLanguageUpdateDto>().ReverseMap();
        
        CreateMap<CrmAPI.Application.Common.Dtos.FacultyDto, FacultyDto>().ReverseMap();
        CreateMap<CrmAPI.Application.Common.Dtos.SpecialityDto, SpecialityDto>().ReverseMap();
        
        CreateMap<CreateContact, CreateContactCommand>()
            .ForMember(c => c.Guid, opt =>
                opt.MapFrom(dom => dom.CorrelationId));
        CreateMap<Contact, ContactGetted>().ReverseMap();
        CreateMap<CreateProcess, Process>();
        CreateMap<CreateProcess, CreateProcessCommand>()
            .ForMember(c => c.Guid, opt =>
                opt.MapFrom(dom => dom.CorrelationId));
        
        CreateMap<UpdateContact, UpdateContactCommand>()
            .ForMember(c => c.Guid, opt =>
                opt.MapFrom(dom => dom.CorrelationId));
        
        CreateMap<ContactLeadDto, UpdateContactLeadCommand>()
            .ForMember(c => c.ContactLeadId, opt =>
                opt.MapFrom(dom => dom.Id));
        
        CreateMap<ContactLeadDto, CreateContactLeadCommand>();
        CreateMap<ContactLeadDto, CreateOrUpdateContactLeadCommand>();
        CreateMap<ContactLead, CreateOrUpdateContactLeadCommand>().ReverseMap();        
        CreateMap<AnnotationCreateDto, CreateAnnotationCommand>().ReverseMap();        
        CreateMap<CloseProcesses, CloseProcessesCommand>();
    }
}