using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Security;
using CrmAPI.Application.Common.Utils;
using CrmAPI.Application.Settings;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.WebEnrollments.Commands.CreateWebEnrollment;

[Authorize]
public record CreateWebEnrollmentCommand : WebEnrollmentDto, IRequest<WebEnrollmentDto> { }

[UsedImplicitly]
public class CreateWebEnrollmentCommandHandler : IRequestHandler<CreateWebEnrollmentCommand, WebEnrollmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICourseWebEnrollmentService _webEnrollmentService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ILogger<CreateWebEnrollmentCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateWebEnrollmentCommandHandler(
        IApplicationDbContext context,
        ICourseWebEnrollmentService webEnrollmentService,
        IMapper mapper,
        ILogger<CreateWebEnrollmentCommandHandler> logger,
        IConfiguration config,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _webEnrollmentService = webEnrollmentService;
        _mapper = mapper;
        _logger = logger;
        _config = config;
        _currentUserService = currentUserService;
    }

    //public async Task<WebEnrollmentDto> Handle(CreateWebEnrollmentCommand request, CancellationToken ct)
    //{
    //    var process = await _context.Processes
    //        .Where(p => p.Id == request.ProcessId)
    //        .FirstOrDefaultAsync(ct);

    //    if (process is null)
    //    {
    //        throw new NotFoundException("Process not found!");
    //    }

    //    var contact = await _context.Contact
    //        .Where(c => c.Id == request.ContactId)
    //        .FirstOrDefaultAsync(ct);

    //    if (contact is null)
    //    {
    //        throw new NotFoundException("Contact not found!");
    //    }

    //    var existingEnrollment = await _context.WebEnrollments
    //        .Where(we => we.ContactId == request.ContactId && we.ProcessId == request.ProcessId)
    //        .FirstOrDefaultAsync(ct);
    //    if (existingEnrollment is null) {
    //       //"No existing enrollment found, proceeding with creation.";
    //        request.Token = GetNewToken(6);
    //        request.Guid = Guid.NewGuid();
    //        request.LanguageCode = request.LanguageCode.ToUpper();
    //        Helper.GetUntilValidTime(_config);
    //        var addWebEnrollment = _mapper.Map<WebEnrollment>(request);
    //        _context.WebEnrollments.Add(addWebEnrollment);
    //    }
    //    else
    //    {
    //        existingEnrollment.Name = request.Name;
    //        existingEnrollment.FirstSurName = request.FirstSurName;
    //        existingEnrollment.SecondSurName = request.SecondSurName;
    //        existingEnrollment.IdCard = request.IdCard;
    //        existingEnrollment.PhonePrefix = request.PhonePrefix;
    //        existingEnrollment.PhonePrefixCountryCode = request.PhonePrefixCountryCode;
    //        existingEnrollment.Phone = request.Phone;
    //        existingEnrollment.ContactEmail = request.ContactEmail;
    //        existingEnrollment.CountryCode = request.CountryCode;
    //        existingEnrollment.VendorId = request.VendorId;
    //        existingEnrollment.CourseTypeBaseCode = request.CourseTypeBaseCode;
    //        existingEnrollment.CourseCode = request.CourseCode;
    //        existingEnrollment.ValidUntil = Helper.GetUntilValidTime(_config);
    //        existingEnrollment.ConvocationDate = request.ConvocationDate;
    //        existingEnrollment.Price = request.Price;
    //        existingEnrollment.PriceFormatted = request.PriceFormatted;
    //        existingEnrollment.FinalPrice = request.FinalPrice;
    //        existingEnrollment.FinalPriceFormatted = request.FinalPriceFormatted;
    //        existingEnrollment.DiscountPercentage = request.DiscountPercentage;
    //        existingEnrollment.EnrollmentPrice = request.EnrollmentPrice;
    //        existingEnrollment.EnrollmentPriceFormatted = request.EnrollmentPriceFormatted;
    //        existingEnrollment.Fees = request.Fees;
    //        existingEnrollment.FeePrice = request.FeePrice;
    //        existingEnrollment.FeePriceFormatted = request.FeePriceFormatted;

    //        request.LanguageCode = request.LanguageCode.ToUpper();
    //        _context.WebEnrollments.Update(existingEnrollment);
    //    }

    //    await _context.SaveChangesAsync(ct);
    //    await SendWebEnrollmentToWeb(request, ct);

    //    return request;
    //}

    public async Task<WebEnrollmentDto> Handle(CreateWebEnrollmentCommand request, CancellationToken ct)
    {
        var process = await _context.Processes
            .FirstOrDefaultAsync(p => p.Id == request.ProcessId, ct);

        if (process is null)
        {
            throw new NotFoundException("Process not found!");
        }

        var contact = await _context.Contact
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, ct);

        if (contact is null)
        {
            throw new NotFoundException("Contact not found!");
        }

        var existingEnrollment = await _context.WebEnrollments
            .FirstOrDefaultAsync(we => we.ContactId == request.ContactId && we.ProcessId == request.ProcessId, ct);

        if (existingEnrollment is null)
        {
            // Crear nueva inscripción
            request.Token = GetNewToken(6);
            request.Guid = Guid.NewGuid();
            request.LanguageCode = request.LanguageCode.ToUpper();
            request.ValidUntil = Helper.GetUntilValidTime(_config);
            var addWebEnrollment = _mapper.Map<WebEnrollment>(request);
            addWebEnrollment.Created = DateTime.Now;
            addWebEnrollment.CreatedBy = _currentUserService.Email;
            _context.WebEnrollments.Add(addWebEnrollment);
            await SendWebEnrollmentToWeb(request, ct);
        }
        else
        {
            // Editar inscripción existente
            request.LanguageCode = request.LanguageCode.ToUpper();
            var tokenExist = existingEnrollment.Token;
            var processId = existingEnrollment.ProcessId;
            var contactId = existingEnrollment.ContactId;            
            _mapper.Map(request, existingEnrollment); // Mapea los valores del request al objeto existente
            existingEnrollment.ValidUntil = Helper.GetUntilValidTime(_config);
            existingEnrollment.Token = tokenExist;
            existingEnrollment.ProcessId = processId;
            existingEnrollment.ContactId = contactId;
            existingEnrollment.LastModified = DateTime.Now;
            existingEnrollment.LastModifiedBy = _currentUserService.Email;


            _context.WebEnrollments.Update(existingEnrollment);
            await SendWebEnrollmentToWeb(_mapper.Map<WebEnrollmentDto>(existingEnrollment), ct);
        }

        await _context.SaveChangesAsync(ct);
        

        return _mapper.Map<WebEnrollmentDto>(existingEnrollment ?? _context.WebEnrollments.Local.First());
    }



    private async Task SendWebEnrollmentToWeb(WebEnrollmentDto webEnrollment, CancellationToken ct)
    {   

        var result = await _webEnrollmentService.SendNewWebEnrollment(webEnrollment, ct);
        if (result.IsError)
        {
            var logLevel = result.FirstError.Type switch
            {
                ErrorType.Failure => LogLevel.Error,
                ErrorType.Unexpected => LogLevel.Warning,
                _ => LogLevel.Debug,
            };

            _logger.Log(logLevel, "{@Error}", result.FirstError);
        }
    }

    private static string GetNewToken(int length)
    {
        var random = new Random();
        const string allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var token = new char[length];
        for (var i = 0; i < length; i++)
        {
            token[i] = allowedCharacters[random.Next(allowedCharacters.Length)];
        }

        return new(token);
    }
}
