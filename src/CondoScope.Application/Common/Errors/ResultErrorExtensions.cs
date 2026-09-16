using FluentResults;

namespace CondoScope.Application.Common.Errors;

internal static class ResultErrorExtensions
{
    public static ResultBase WithContext(this ResultBase result, ErrorContext ctx)
    {
        foreach (var err in result.Errors)
        {
            err.Metadata["TraceId"] = ctx.TraceId;
            err.Metadata["Feature"] = ctx.Feature;
            err.Metadata["Handler"] = ctx.Handler;
            err.Metadata["Layer"] = ctx.Layer;
        }

        return result;
    }

}
