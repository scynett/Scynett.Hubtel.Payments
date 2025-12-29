namespace Scynett.Hubtel.Payments.AspNetCore.Common.ProblemDetails;

using Microsoft.AspNetCore.Http;

internal interface IErrorView
{
    string Code { get; }
    string Description { get; }
}

internal static class ProblemDetailsMapper
{
    internal static IResult Map(IErrorView error, int statusCode)
        => Results.Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: statusCode);
}