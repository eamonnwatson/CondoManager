using System.ComponentModel.DataAnnotations;

namespace CondoScope.Web.Components.Dialogs.Models;

internal class AddUnitFormModel
{
    [Required]
    public string UnitNumber { get; set; } = default!;
    [Required]
    public string Address { get; set; } = default!;
}
