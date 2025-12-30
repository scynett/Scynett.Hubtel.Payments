using Microsoft.AspNetCore.Http;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.ProblemDetails;

internal static class ProblemDetailsExtensions
{
    internal static IResult ToProblemDetails(string code, string description, int statusCode = StatusCodes.Status400BadRequest)
        => Results.Problem(
            title: code,
            detail: description,
            statusCode: statusCode);
}