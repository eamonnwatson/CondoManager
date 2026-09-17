using System.ComponentModel.DataAnnotations;

namespace CondoScope.Web.Components.Dialogs.Models;

public class AddOwnerFormModel : IValidatableObject
{
    [Required]
    public string Name { get; set; } = default!;

    [EmailAddress]
    public string? EmailAddress { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
    public string? UnitId { get; set; }
    public DateTime? EffectiveDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(UnitId) && EffectiveDate is null)
        {
            yield return new ValidationResult(
                "Effective Date is required when a Unit is selected.",
                [nameof(EffectiveDate)]);
        }
    }
}
