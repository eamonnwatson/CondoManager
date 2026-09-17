using System.ComponentModel.DataAnnotations;

namespace CondoScope.Web.Components.Dialogs.Models;

internal class AddPaymentFormModel
{
    [Required] public DateTime? PaymentDate { get; set; }
    [Required] public string? UnitId { get; set; }
    [Required] public decimal? Amount { get; set; }
    [Required] public int? PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
