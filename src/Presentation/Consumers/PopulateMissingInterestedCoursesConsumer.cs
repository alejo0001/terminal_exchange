using System.Threading.Tasks;
using CrmAPI.Application.ContactLeads.Commands.PopulateMissingInterestedCourses.Commands;
using CrmAPI.Contracts.Commands;
using CrmAPI.Contracts.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace CrmAPI.Presentation.Consumers;

[UsedImplicitly]
public class PopulateMissingInterestedCoursesConsumer
    : IConsumer<PopulateMissingInterestedCourses>
{
    private readonly ISender _sender;
    private readonly string _apiKey;

    public PopulateMissingInterestedCoursesConsumer(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _apiKey = configuration["Constants:ApiKey"]!;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<PopulateMissingInterestedCourses> context)
    {
        var request = new PopulateMissingInterestedCoursesCommand(
            context.Message.Dto.Area,
            context.Message.Dto.CountryCode,
            _apiKey,
            context.Message.Dto.MaxJobContacts,
            context.Message.Dto.ContactIds
        );

        var errorOrResult = await _sender.Send(request, context.CancellationToken).ConfigureAwait(false);

        await context.RespondAsync(new MissingInterestedCoursesPopulated(!errorOrResult.IsError));
    }
}
