using System.ComponentModel.DataAnnotations;

namespace CondoScope.Web.Components.Dialogs.Models;

internal class AddFeeChargeFormModel : IValidatableObject
{
    [Required] public DateTime? DueDate { get; set; }
    [Required] public string Description { get; set; } = default!;
    [Required] public decimal Amount { get; set; }
    [Required] public int Category { get; set; } = default!;
    [Required] public int Scope { get; set; } = default!;
    public string? UnitId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(UnitId) && Scope == 1)
        {
            yield return new ValidationResult(
                "Unit is required when Scope is set to Specific Unit.",
                [nameof(UnitId)]);
        }
    }

}
