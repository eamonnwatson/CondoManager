using FluentResults;
using MudBlazor;

namespace CondoScope.Web.Infrastructure;

public static class ResultSnackBarExtensions
{
    public static bool ShowErrorsIfFailed(this Result result, ISnackbar snackbar)
    {
        if (result.IsSuccess)
            return false;

        foreach (var error in result.Errors)
            snackbar.Add(error.Message, Severity.Error);

        return true;
    }

    public static bool ShowErrorsIfFailed<T>(this Result<T> result, ISnackbar snackbar)
    {
        if (result.IsSuccess)
            return false;

        foreach (var error in result.Errors)
            snackbar.Add(error.Message, Severity.Error);

        return true;
    }
}