using CondoScope.Application.Ledger;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CondoScope.Application.Statement.Pdf;

internal class StatementPdfGenerator : IStatementPdfGenerator
{
    private static readonly string PrimaryColor = Colors.Blue.Medium;

    public byte[] Generate(StatementDto statement, DateOnly asOfDate)
    {
        var lastPaymentActivity = statement.AccountActivities
            .Where(a => a.Payment.HasValue)
            .OrderByDescending(a => a.ActivityDate)
            .FirstOrDefault();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().BorderBottom(3).BorderColor(PrimaryColor).PaddingBottom(5).Column(header =>
                    {
                        header.Item().Text("Unit Statement").FontSize(20).Bold();
                        header.Item().Text("Owner account statement").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(unitColumn =>
                        {
                            unitColumn.Item().Text($"Unit {statement.UnitNumber}").Bold().FontSize(13).FontColor(PrimaryColor);
                            unitColumn.Item().Text(statement.Address);
                        });

                        row.RelativeItem().AlignRight().Column(dateColumn =>
                        {
                            dateColumn.Item().AlignRight().Text("Statement As Of").Bold().FontColor(PrimaryColor);
                            dateColumn.Item().AlignRight().Text(asOfDate.ToString("yyyy-MM-dd"));
                        });
                    });

                    column.Item().Column(owner =>
                    {
                        owner.Item().Text("Owner").Bold().FontSize(13).FontColor(PrimaryColor);
                        owner.Item().Text(statement.OwnerName);
                        owner.Item().Text(statement.EmailAddress);

                        if (!string.IsNullOrWhiteSpace(statement.PhoneNumber))
                            owner.Item().Text(statement.PhoneNumber);

                    });

                    column.Item().Column(summary =>
                    {
                        summary.Item().Text("Account Summary").Bold().FontSize(13).FontColor(PrimaryColor);

                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Current Balance");
                            row.RelativeItem().AlignRight().Text(statement.CurrentBalance.ToString("C2")).Bold().FontSize(14).FontColor(PrimaryColor);
                        });
                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Status");
                            row.RelativeItem().AlignRight().Text(FormatStatus(statement.AccountStatus));
                        });
                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Last Payment");
                            row.RelativeItem().AlignRight().Text(lastPaymentActivity?.ActivityDate.ToString("yyyy-MM-dd") ?? "N/A");
                        });
                        summary.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Last Payment Amount");
                            row.RelativeItem().AlignRight().Text(lastPaymentActivity?.Payment?.ToString("C2") ?? "N/A");
                        });
                    });

                    column.Item().Column(activity =>
                    {
                        activity.Item().Text("Account Activity").Bold().FontSize(13).FontColor(PrimaryColor);
                        activity.Item().Text($"All charges and payments for Unit {statement.UnitNumber}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(PrimaryColor).Padding(5).Text("Date").FontColor(Colors.White).Bold();
                            header.Cell().Background(PrimaryColor).Padding(5).Text("Description").FontColor(Colors.White).Bold();
                            header.Cell().Background(PrimaryColor).Padding(5).AlignRight().Text("Charges").FontColor(Colors.White).Bold();
                            header.Cell().Background(PrimaryColor).Padding(5).AlignRight().Text("Payments").FontColor(Colors.White).Bold();
                            header.Cell().Background(PrimaryColor).Padding(5).AlignRight().Text("Balance").FontColor(Colors.White).Bold();
                        });

                        foreach (var (activityRow, index) in statement.AccountActivities.Select((a, i) => (a, i)))
                        {
                            var background = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            table.Cell().Background(background).Padding(5).Text(activityRow.ActivityDate.ToString("yyyy-MM-dd"));
                            table.Cell().Background(background).Padding(5).Text(activityRow.Description);
                            table.Cell().Background(background).Padding(5).AlignRight().Text(activityRow.Charge?.ToString("C2") ?? string.Empty);
                            table.Cell().Background(background).Padding(5).AlignRight().Text(activityRow.Payment?.ToString("C2") ?? string.Empty);
                            table.Cell().Background(background).Padding(5).AlignRight().Text(activityRow.Balance.ToString("C2"));
                        }
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatStatus(AccountStatus status) => status switch
    {
        AccountStatus.PaidInFull => "Paid In Full",
        AccountStatus.Credit => "Credit",
        AccountStatus.Outstanding => "Outstanding",
        _ => status.ToString()
    };
}
