using AutoMapper;
using CourseApi.Data.Domain;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Utils;
using ErrorOr;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrmAPI.Application.WebEnrollments.Services;

/// <summary>
///     <inheritdoc /><br />
///     This implementation communicates directly with respective databases (FP and University e.g. "normal").
/// </summary>
/// <remarks>
///     At the time of creation, the necessity to do so came from intense overload of respective API. <br />
///     Should be swapped out by new microservice API in near future.
/// </remarks>
public class CourseWebEnrollmentService : ICourseWebEnrollmentService
{
    private readonly Lazy<ICourseFPApiClient> _lazyUnitOfWorkFp;
    private readonly Lazy<IUnitOfWork<ICoursesUnDbContext>> _lazyUnitOfWorkUniversity;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly ICurrentUserService _currentUserService;

    public CourseWebEnrollmentService(
        Lazy<ICourseFPApiClient> lazyUnitOfWorkFp,
        Lazy<IUnitOfWork<ICoursesUnDbContext>> lazyUnitOfWorkUniversity,
        IConfiguration config,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _lazyUnitOfWorkFp = lazyUnitOfWorkFp;
        _lazyUnitOfWorkUniversity = lazyUnitOfWorkUniversity;
        _config = config;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public Task<ErrorOr<Created>> SendNewWebEnrollment(WebEnrollmentDto serviceDto, CancellationToken ct)
    {
        if (serviceDto.University == University.TechFP)
        {
            serviceDto.University = University.TechUniversity;
        }

        return InsertNewCrmEnrollmentInfoToDb(serviceDto, _lazyUnitOfWorkUniversity.Value,_config, _mapper, ct, _currentUserService);
    }

    private static async Task<ErrorOr<Created>> InsertNewCrmEnrollmentInfoToDb<TContext>(
        WebEnrollmentDto serviceDto,
        IUnitOfWork<TContext> unitOfWork,
        IConfiguration config,
        IMapper mapper,
        CancellationToken ct, 
        ICurrentUserService currentUserService
        )
    {
        int savedCount;

        try
        {
            var entity = mapper.Map<CrmEnrollment>(serviceDto);

            var existing = await unitOfWork
                .Set<CrmEnrollment>()
                .FirstOrDefaultAsync(e =>
                    e.ContactId == entity.ContactId &&
                    e.ProcessId == entity.ProcessId, ct);

            if (existing != null)
            {
                // Actualiza los campos existentes
                mapper.Map(serviceDto, existing);
                existing.LastModifiedBy = currentUserService.Email;
                existing.ModificationDate = DateTime.Now;
                unitOfWork.Entry(existing).State = EntityState.Modified;
            }
            else
            {
                entity.ValidUntil = Helper.GetUntilValidTime(config);
                entity.CreationDate = DateTime.Now;
                entity.CreatedBy = currentUserService.Email;
                unitOfWork.Entry(entity).State = EntityState.Added;
            }

            savedCount = await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            return CreateError(serviceDto.University.ToString(), ex);
        }

        return savedCount > 0
            ? Result.Created
            : CreateEmptySaveError(serviceDto.University.ToString());
    }


    //private static async Task<ErrorOr<Created>> InsertNewCrmEnrollmentInfoToDb<TContext>(
    //    WebEnrollmentDto serviceDto,
    //    IUnitOfWork<TContext> unitOfWork,
    //    IConfiguration config,
    //    CancellationToken ct)
    //{
    //    int savedCount;
    //    try
    //    {
    //        var entity = MapToCrmEnrollment(serviceDto, GetUntilValidTime(config)); // Use the passed config

    //        var entityEntry = unitOfWork.Entry(entity);

    //        entityEntry.State = EntityState.Added;

    //        savedCount = await unitOfWork.SaveChangesAsync(ct);
    //    }
    //    catch (Exception ex)
    //    {
    //        return CreateError(serviceDto.University.ToString(), ex);
    //    }

    //    return savedCount > 0
    //        ? Result.Created
    //        : CreateEmptySaveError(serviceDto.University.ToString());
    //}

    //private static Task<ErrorOr<Created>> SendNewCrmEnrollmentInfoToFpApi(
    //    WebEnrollmentDto serviceDto,
    //    ICourseFPApiClient fpApiClient,
    //    CancellationToken ct) =>
    //    fpApiClient.DoEnrollment(serviceDto, ct);

    //private static CrmEnrollment MapToCrmEnrollment(WebEnrollmentDto dto, DateTime validUntil) => new()
    //{
    //    Guid = dto.Guid?.ToString(),
    //    Price = dto.Price ?? 0m,
    //    PriceFormatted = dto.PriceFormatted,
    //    FinalPrice = dto.FinalPrice ?? 0m,
    //    FinalPriceFormatted = dto.FinalPriceFormatted,
    //    DiscountPercentage = dto.DiscountPercentage ?? 0m,
    //    EnrollmentPrice = dto.EnrollmentPrice ?? 0m,
    //    EnrollmentPriceFormatted = dto.EnrollmentPriceFormatted,
    //    Fees = dto.Fees,
    //    FeePrice = dto.FeePrice ?? 0m,
    //    FeePriceFormatted = dto.FeePriceFormatted,
    //    Name = dto.Name,
    //    FirstSurname = dto.FirstSurName,
    //    SecondSurname = dto.SecondSurName,
    //    IdCard = dto.IdCard,
    //    PhonePrefix = dto.PhonePrefix,
    //    PhonePrefixCountryCode = dto.PhonePrefixCountryCode,
    //    Phone = dto.Phone,
    //    ValidUntil = validUntil,
    //    CountryCode = dto.CountryCode,
    //    ProcessId = dto.ProcessId,
    //    ContactId = dto.ContactId,
    //    VendorId = dto.VendorId,
    //    CourseTypeBaseCode = dto.CourseTypeBaseCode,
    //    CourseCode = dto.CourseCode,
    //    LanguageCode = dto.LanguageCode,
    //    ContactEmail = dto.ContactEmail,
    //    Token = dto.Token,
    //    ConvocationDate = dto.ConvocationDate ?? DateTime.MinValue,
    //    CurrencyCode = dto.CurrencyCode,
    //    Currency = dto.CurrencySymbol,
    //};

    private static Error CreateError(string institution, Exception ex) => Error.Failure(
        $"{nameof(CourseWebEnrollmentService)}.{nameof(InsertNewCrmEnrollmentInfoToDb)}.DatabaseProblem",
        $"Problem inserting new WebEnrollment (CrmEnrollment) into '{institution}' Course Database",
        new() { { "Exception", ex } });

    
    private static Error CreateEmptySaveError(string institution) => Error.Unexpected(
        $"{nameof(CourseWebEnrollmentService)}.{nameof(InsertNewCrmEnrollmentInfoToDb)}.UnknownProblem",
        $"Problem inserting new WebEnrollment (CrmEnrollment) into '{institution}' Course Database: no entities ");

  
}
