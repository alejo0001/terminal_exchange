using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.Interfaces;
using CrmAPI.Application.Common.Mappings;
using IntranetMigrator.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Application.Courses.Queries.GetTopSellingByFacultiesAndCountry;

public class GetTopSellingByFacultiesAndCountryQuery: IRequest<List<TopSellingCourseDto>>
{
    public string CountryCode { get; set; }
    public List<int>? FacultiesId { get; set; }
    public int? Quantity { get; set; }
}

public class GetTopSellingByFacultiesAndCountryQueryHandler : IRequestHandler<GetTopSellingByFacultiesAndCountryQuery, List<TopSellingCourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
        
    public GetTopSellingByFacultiesAndCountryQueryHandler(IApplicationDbContext context,
        IMapper mapper, IConfiguration configuration)
    {
        _context = context;
        _mapper = mapper;
        _configuration = configuration;
    }


    public async Task<List<TopSellingCourseDto>> Handle(GetTopSellingByFacultiesAndCountryQuery request, CancellationToken ct)
    {
        int quantity = request.Quantity ?? int.Parse(_configuration["Constants:QuantityTopSelling"]);

        IQueryable<TopSellingCourse> topSellingCourses = _context.TopSellingCourses
            .Include(ts => ts.Country)
            .Where(ts => ts.Country.CountryCode == request.CountryCode);

        if (request.FacultiesId != null)
        {
            topSellingCourses = topSellingCourses
                .Where(ts => request.FacultiesId.Contains(ts.FacultyId));
        }

        var topCourses = await topSellingCourses
            .OrderByDescending(ts => ts.Total)
            .Take(quantity)
            .ToListAsync(ct);

        // Extraer todos los CourseCodes únicos
        var courseCodes = topCourses.Select(ts => ts.CourseCode).Distinct().ToList();

        // Traer los cursos y su tipo base
        var coursesWithTypeBase = await _context.Courses
            .Where(c => courseCodes.Contains(c.Code))
            .Select(c => new
            {
                c.Code,
                CourseTypeBaseCode = c.CourseTypeBase.Code
            })
            .ToListAsync(ct);

        // Convertir en diccionario para acceso rápido
        var courseCodeToTypeBase = coursesWithTypeBase
            .GroupBy(c => c.Code)
            .ToDictionary(g => g.Key, g => g.First().CourseTypeBaseCode);

        var result = topCourses.Select(ts => new TopSellingCourseDto
        {
            FacultyId = ts.FacultyId,
            Title = ts.Title,
            CourseCode = ts.CourseCode,
            CourseTypeBaseCode = courseCodeToTypeBase.GetValueOrDefault(ts.CourseCode, String.Empty),
            CountryId = ts.Country.Id,
            Total = ts.Total
        }).ToList();

        return result;
    }
}