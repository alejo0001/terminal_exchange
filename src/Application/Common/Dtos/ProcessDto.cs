using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Dtos;

public class ProcessDto : IMapFrom<Process>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string FirstSurname { get; set; }
    public int ContactLanguageId;
    public ContactLanguageDto? ContactLanguage { get; set; }
    public int ContactId { get; set; }
    public int OrdersImportedId { get; set; }
    public DateTime Created { get; set; }
    public UserDto User { get; set; }
    public ContactDto Contact { get; set; }
    public OrdersImportedChildDto OrdersImported { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Outcome { get; set; }
    public string Description { get; set; }
    public string? Colour { get; set; }
    public int Attempts { get; set; }
    public bool ActiveCall { get; set; }
    public DateTime? LastActionDate { get; set; }
    public LastActionProcessDto? LastAction { get; set; }
    public AppointmentDto? NextAction { get; set; }
    public DateTime NextInteractionDate { get; set; }
    public List<ActionChildDto> Actions { get; set; }
    public List<FacultyDto>? Faculties { get; set; }
    public List<SpecialityDto>? Specialities { get; set; }
    public DateTime? InitialDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Process, ProcessDto>()
            .ForMember(d => d.Type, opt =>
                opt.MapFrom(dom => dom.Type.ToString().ToLowerInvariant()))
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(dom => dom.Status.ToString().ToLowerInvariant()))
            .ForMember(d => d.Outcome, opt =>
                opt.MapFrom(dom => dom.Outcome.ToString().ToLowerInvariant()))
            .ForMember(d => d.Colour, opt =>
                opt.MapFrom(dom => dom.Colour.ToString().ToLowerInvariant()))
            .ForMember(d => d.Attempts, opt =>
                opt.MapFrom(dom => dom.Actions
                    .Count(a => !a.IsDeleted && a.Type == ActionType.Call)))
            .ForMember(d => d.Actions, opt =>
                opt.MapFrom(dom => dom.Actions))
            .ForMember(d => d.Faculties, opt =>
                opt.MapFrom(dom => dom.Contact.Faculties))
            .ForMember(d => d.Specialities, opt =>
                opt.MapFrom(dom => dom.Contact.Specialities))
            .ForMember(d => d.LastAction, opt =>
                opt.MapFrom(dom => dom.Actions.OrderByDescending(a => a.Date).FirstOrDefault()))
            .ForMember(d => d.NextAction, opt =>
                opt.MapFrom(dom => dom.Appointments.OrderByDescending(a => a.Date).FirstOrDefault()))
            .ForMember(d => d.ActiveCall, opt =>
                opt.MapFrom(dom => dom.Actions.Any(a => a.FinishDate == null && a.Type == ActionType.Call)))
            .ForMember(d => d.LastActionDate, opt =>
                opt.MapFrom(dom =>
                    dom.Actions.Any()
                        ? dom.Actions.OrderByDescending(a => a.Date).FirstOrDefault().Date
                        : (DateTime?)null));

    }
}