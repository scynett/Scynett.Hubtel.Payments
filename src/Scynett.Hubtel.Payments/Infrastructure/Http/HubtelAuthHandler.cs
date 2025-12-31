using Microsoft.Extensions.Options;

using Scynett.Hubtel.Payments.Options;

using System.Net.Http.Headers;
using System.Text;

namespace Scynett.Hubtel.Payments.Infrastructure.Http;

internal sealed class HubtelAuthHandler(IOptions<HubtelOptions> options)
    : DelegatingHandler
{
    private readonly string _authValue =
        Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
                $"{options.Value.ClientId}:{options.Value.ClientSecret}"));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", _authValue);

        return base.SendAsync(request, cancellationToken);
    }
}

