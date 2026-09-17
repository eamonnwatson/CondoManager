namespace CondoScope.Application.Statement.Pdf;

public interface IStatementPdfGenerator
{
    byte[] Generate(StatementDto statement, DateOnly asOfDate);
}
