using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrmAPI.Application.Common.Dtos;

namespace CrmAPI.Application.Common.Interfaces;

public interface ICroupierApiClient
{
    [Obsolete("CrmAPI itself should do this task, that currently happens in CroupierAPI.")]
    Task<HttpResponseMessage> UpdateContactFromIntranet(LeadDto lead, CancellationToken ct);

    [Obsolete("CrmAPI itself should do this task, that currently happens in CroupierAPI.")]
    Task<HttpResponseMessage> UpdateContactStatusFromIntranet(int originalContactId, int status, CancellationToken ct);
}
