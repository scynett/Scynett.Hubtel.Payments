# Scynett.Hubtel.Payments.AspNetCore Classes

Generated on 2026-01-01T09:02:54Z

## src\Scynett.Hubtel.Payments.AspNetCore\Common\Http\CorrelationIdMiddleware.cs

 ```csharp 
using Microsoft.AspNetCore.Http;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

internal sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string HeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var id))
        {
            id = Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName] = id;
        }

        context.Response.Headers[HeaderName] = id!;
        await _next(context).ConfigureAwait(false);
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\Common\Http\RequestLoggingMiddleware.cs

 ```csharp 
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

internal sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation(
            "HTTP {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await _next(context).ConfigureAwait(false);
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\Common\Http\StatusEndpointExtensions.cs

 ```csharp 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Http;

public static class StatusEndpointExtensions
{
    public static IEndpointRouteBuilder MapStatusEndpoint(this IEndpointRouteBuilder endpoints, string pattern = "/status")
    {
        endpoints.MapGet(pattern, async context =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"status\":\"ok\"}").ConfigureAwait(false);
        })
        .WithName("Status")
        .WithTags("Status");
        return endpoints;
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\Common\ProblemDetails\ProblemDetailsExtensions.cs

 ```csharp 
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
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\Common\ProblemDetails\ProblemDetailsMapper.cs

 ```csharp 
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
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\Common\Routing\RouteGroups.cs

 ```csharp 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Scynett.Hubtel.Payments.AspNetCore.Common.Routing;

internal static class RouteGroups
{
    internal static IEndpointRouteBuilder MapHubtelGroup(
        this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGroup("/hubtel");
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DependencyInjection\OptionsValidationExtensions.cs

 ```csharp 
using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Options;

namespace Scynett.Hubtel.Payments.AspNetCore.DependencyInjection;

internal static class OptionsValidationExtensions
{
    internal static IServiceCollection AddHubtelOptionsValidation(
        this IServiceCollection services)
    {
        services.AddOptions<HubtelOptions>()
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.ClientId) &&
                !string.IsNullOrWhiteSpace(o.ClientSecret),
                "Hubtel ClientId and ClientSecret must be provided");

        return services;
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Callback\CallbackEndpointOptions.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

public sealed class CallbackEndpointOptions
{
    public string Route { get; init; } =
        RouteConstants.ReceiveMoneyCallback;
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Callback\HubtelReceiveMoneyCallbackEndpoint.cs

 ```csharp 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

internal static class HubtelReceiveMoneyCallbackEndpoint
{
    internal static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            RouteConstants.ReceiveMoneyCallback,
            async (
                ReceiveMoneyCallbackRequest payload,
                IDirectReceiveMoney directReceiveMoney,
                CancellationToken ct) =>
            {
                var result =
                    await directReceiveMoney
                        .HandleCallbackAsync(payload, ct)
                        .ConfigureAwait(false);

                // 200 OK means "callback processed", not "payment succeeded".
                if (result.IsSuccess)
                    return Results.Ok();

                // No dependency on Scynett.Common.Domain here:
                var code = result.Error?.Code ?? "Hubtel.Callback.Error";
                var message = result.Error?.Description ?? "Callback processing failed.";

                return Results.BadRequest(new { error = code, message });
            })
            .AllowAnonymous()
            .WithName("HubtelDirectReceiveMoneyCallback")
            .WithTags("Hubtel", "DirectReceiveMoney");
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Callback\RouteConstants.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

internal static class RouteConstants
{
    public const string ReceiveMoneyCallback =
        "/hubtel/direct-receive-money/callback";
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\DirectReceiveMoneyEndpointExtensions.cs

 ```csharp 
using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;
using Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney;

public static class DirectReceiveMoneyEndpointExtensions
{
    public static IEndpointRouteBuilder MapHubtelDirectReceiveMoney(
        this IEndpointRouteBuilder endpoints)
    {
        HubtelReceiveMoneyCallbackEndpoint.Map(endpoints);
        HubtelTransactionStatusEndpoint.Map(endpoints);
        return endpoints;
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Security\Otp\SendOtpEndpoint.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Security.Otp;

internal sealed class SendOtpEndpoint
{
    public int MyProperty { get; set; }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Security\Otp\VerifyOtpEndpoint.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Security.Otp;

internal sealed class VerifyOtpEndpoint
{
    public int MyProperty { get; set; }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Security\Registration\RegisteredUserPolicy.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Security.Registration;

internal sealed class RegisteredUserPolicy
{
    public int MyProperty { get; set; }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Status\HubtelTransactionStatusEndpoint.cs

 ```csharp 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

internal static class HubtelTransactionStatusEndpoint
{
    internal static void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            RouteConstants.TransactionStatus,
            async (
                string clientReference,
                IDirectReceiveMoney directReceiveMoney,
                CancellationToken ct) =>
            {
                var result =
                    await directReceiveMoney
                        .CheckStatusAsync(new TransactionStatusQuery(clientReference), ct)
                        .ConfigureAwait(false);

                // 200 OK means "status retrieved"
                if (result.IsSuccess)
                    return Results.Ok(result.Value);

                var code = result.Error?.Code ?? "Hubtel.Status.Error";
                var message = result.Error?.Description ?? "Unable to retrieve transaction status.";

                return Results.BadRequest(new { error = code, message });
            })
            .AllowAnonymous()
            .WithName("HubtelDirectReceiveMoneyStatus")
            .WithTags("Hubtel", "DirectReceiveMoney");
    }
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Status\RouteConstants.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

internal static class RouteConstants
{
    public const string TransactionStatus = "/hubtel/direct-receive-money/status";
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\DirectReceiveMoney\Status\StatusEndpointOptions.cs

 ```csharp 
namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Status;

public sealed class StatusEndpointOptions
{
    public string Route { get; init; } =
        RouteConstants.TransactionStatus;
}
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\GlobalSuppressions.cs

 ```csharp 
using System.Diagnostics.CodeAnalysis;

// CA1848: For simple logging scenarios, LoggerExtensions methods are acceptable
// Converting to LoggerMessage delegates would add significant complexity for minimal benefit
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerExtensions are acceptable for simple logging scenarios", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore")]

// CA1031: Catching general exceptions in background services is acceptable for logging and resilience
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Background services catch all exceptions for resilience and logging", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore.Workers")]

// CA1062: Extension method parameters are validated by the framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Extension method parameters are validated by the framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore.Extensions")]

// CA1812: Internal record types used for JSON deserialization are instantiated by System.Text.Json
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via reflection or DI (middleware/endpoints)", Scope = "module")]

[assembly: SuppressMessage(
    "Design",
    "CA1062:Validate arguments of public methods",
    Justification = "Validated by ASP.NET Core pipeline")]
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\obj\Debug\net9.0\.NETCoreApp,Version=v9.0.AssemblyAttributes.cs

 ```csharp 
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\obj\Debug\net9.0\Scynett.Hubtel.Payments.AspNetCore.AssemblyInfo.cs

 ```csharp 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("Scynett")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyCopyrightAttribute("Copyright (c) 2026 Scynett")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("A modern .NET SDK for Hubtel Mobile Money payment integration with built-in resil" +
    "ience, observability, and production-ready patterns.")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+6a0d9d24e4d84ea2ffafde31a2836573a703e743")]
[assembly: System.Reflection.AssemblyProductAttribute("Scynett.Hubtel.Payments")]
[assembly: System.Reflection.AssemblyTitleAttribute("Scynett.Hubtel.Payments.AspNetCore")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyMetadataAttribute("RepositoryUrl", "https://github.com/scynett/Scynett.Hubtel.Payments")]

// Generated by the MSBuild WriteCodeFragment class.

 ``` 

## src\Scynett.Hubtel.Payments.AspNetCore\obj\Debug\net9.0\Scynett.Hubtel.Payments.AspNetCore.GlobalUsings.g.cs

 ```csharp 
// <auto-generated/>
global using System;
global using System.Collections.Generic;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
 ``` 

