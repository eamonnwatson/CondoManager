namespace CondoScope.Application.Common.Errors;

public sealed record ErrorContext(string TraceId, string Feature, string Handler, string Layer);