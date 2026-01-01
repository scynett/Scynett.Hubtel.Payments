using System.Diagnostics;

namespace Scynett.Hubtel.Payments.Infrastructure.Diagnostics;

internal static class HubtelDiagnostics
{
    public static readonly ActivitySource ActivitySource = new("Scynett.Hubtel.Payments");
}
