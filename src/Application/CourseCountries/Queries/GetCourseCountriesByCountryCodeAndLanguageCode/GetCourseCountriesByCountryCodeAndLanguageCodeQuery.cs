using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Exceptions;
using CrmAPI.Application.Common.Interfaces;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.CourseCountries.Queries.GetCourseCountriesByCountryCodeAndLanguageCode;

public class GetCourseCountriesByCountryCodeAndLanguageCodeQuery : IRequest<CourseCountryDto>
{
    public string CountryCode { get; set; }
    public string LanguageCode { get; set; }
}
    
public class GetCourseCountriesByCountryCodeAndLanguageCodeQueryHandler : IRequestHandler<GetCourseCountriesByCountryCodeAndLanguageCodeQuery, CourseCountryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public GetCourseCountriesByCountryCodeAndLanguageCodeQueryHandler(IApplicationDbContext context, IMapper mapper, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
    }
        
    public async Task<CourseCountryDto> Handle(GetCourseCountriesByCountryCodeAndLanguageCodeQuery request, CancellationToken cancellationToken)
    {
        var cc = await _context.CourseCountries
            .Include(cc => cc.Language)
            .FirstOrDefaultAsync(cc => cc.Code == request.CountryCode
                                       && cc.LanguageCode == request.LanguageCode, cancellationToken);
        if (cc == null)
        {
            cc = await _context.CourseCountries
                .Include(cc => cc.Language)
                .FirstOrDefaultAsync(cc => cc.Code == _configuration["Constants:SpainCountryCode"]
                                           && cc.LanguageCode == _configuration["Constants:DefaultLanguageCode"], cancellationToken);
        }

        if (cc != null)
        {
            cc.CurrencyFormat = (await _context.Currency
                .Where(c => c.CurrencyCode == cc.CurrencyCode)
                .FirstOrDefaultAsync(cancellationToken))?.CurrencyDisplayFormat ?? cc.CurrencyFormat;
        }
        return _mapper.Map<CourseCountryDto>(cc);
    }
}