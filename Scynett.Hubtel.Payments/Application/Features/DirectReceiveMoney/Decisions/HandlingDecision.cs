namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public sealed record HandlingDecision(
       string Code,
       string Description,
       NextAction NextAction,
       ResponseCategory Category,
       bool IsSuccess = false,
       bool IsFinal = true,
       bool ShouldRetry = false,
       string? CustomerMessage = null,
       string? DeveloperHint = null);