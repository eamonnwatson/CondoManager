using System.Linq;
using CondoScope.Persistence.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CondoScope.Persistence.UnitTests.Migrations;

[TestClass]
public class InitialCreateTests
{
    private sealed class TestableInitialCreate : InitialCreate
    {
        public void InvokeUp(MigrationBuilder migrationBuilder) => Up(migrationBuilder);

        public void InvokeDown(MigrationBuilder migrationBuilder) => Down(migrationBuilder);
    }

    private static MigrationBuilder CreateBuilder() => new("Microsoft.EntityFrameworkCore.Sqlite");

    [TestMethod]
    public void Up_Invoked_CreatesSixTables()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var createTableOps = migrationBuilder.Operations.OfType<CreateTableOperation>().ToList();
        var tableNames = createTableOps.Select(op => op.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { "FeeCharges", "Owners", "Units", "FeeChargeUnits", "Payments", "UnitOwners" },
            tableNames);
    }

    [TestMethod]
    public void Up_Invoked_CreatesFourIndexes()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var createIndexOps = migrationBuilder.Operations.OfType<CreateIndexOperation>().ToList();
        var indexNames = createIndexOps.Select(op => op.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "IX_FeeChargeUnits_UnitId",
                "IX_Payments_UnitId",
                "IX_UnitOwners_OwnerId",
                "IX_UnitOwners_UnitId_EffectiveFrom",
            },
            indexNames);
    }

    [TestMethod]
    public void Up_Invoked_FeeChargesTableHasExpectedColumnsAndPrimaryKey()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var feeCharges = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "FeeCharges");

        var columnNames = feeCharges.Columns.Select(c => c.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[]
            {
                "Id", "DueDate", "Description", "Category", "Scope", "Amount",
                "CreatedAtUtc", "CreatedBy", "LastModifiedAtUtc", "LastModifiedBy", "RowVersion",
            },
            columnNames);

        Assert.IsNotNull(feeCharges.PrimaryKey);
        Assert.AreEqual("PK_FeeCharges", feeCharges.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(new[] { "Id" }, feeCharges.PrimaryKey.Columns);

        var descriptionColumn = feeCharges.Columns.Single(c => c.Name == "Description");
        Assert.AreEqual(500, descriptionColumn.MaxLength);
        Assert.IsFalse(descriptionColumn.IsNullable);

        var amountColumn = feeCharges.Columns.Single(c => c.Name == "Amount");
        Assert.AreEqual(18, amountColumn.Precision);
        Assert.AreEqual(2, amountColumn.Scale);

        var rowVersionColumn = feeCharges.Columns.Single(c => c.Name == "RowVersion");
        Assert.IsTrue(rowVersionColumn.IsRowVersion);
        Assert.IsTrue(rowVersionColumn.IsNullable);
    }

    [TestMethod]
    public void Up_Invoked_OwnersTableHasExpectedColumnsAndPrimaryKey()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var owners = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "Owners");

        Assert.IsNotNull(owners.PrimaryKey);
        Assert.AreEqual("PK_Owners", owners.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(new[] { "Id" }, owners.PrimaryKey.Columns);

        var nameColumn = owners.Columns.Single(c => c.Name == "Name");
        Assert.AreEqual(200, nameColumn.MaxLength);
        Assert.IsFalse(nameColumn.IsNullable);

        var emailColumn = owners.Columns.Single(c => c.Name == "Email");
        Assert.AreEqual(200, emailColumn.MaxLength);
        Assert.IsTrue(emailColumn.IsNullable);

        var phoneColumn = owners.Columns.Single(c => c.Name == "Phone");
        Assert.AreEqual(30, phoneColumn.MaxLength);
        Assert.IsTrue(phoneColumn.IsNullable);
    }

    [TestMethod]
    public void Up_Invoked_UnitsTableHasExpectedColumnsAndPrimaryKey()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var units = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "Units");

        Assert.IsNotNull(units.PrimaryKey);
        Assert.AreEqual("PK_Units", units.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(new[] { "Id" }, units.PrimaryKey.Columns);

        var unitNumberColumn = units.Columns.Single(c => c.Name == "UnitNumber");
        Assert.AreEqual(30, unitNumberColumn.MaxLength);

        var addressColumn = units.Columns.Single(c => c.Name == "Address");
        Assert.AreEqual(300, addressColumn.MaxLength);

        var isActiveColumn = units.Columns.Single(c => c.Name == "IsActive");
        Assert.IsFalse(isActiveColumn.IsNullable);
    }

    [TestMethod]
    public void Up_Invoked_FeeChargeUnitsTableHasCompositePrimaryKeyAndForeignKeys()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var feeChargeUnits = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "FeeChargeUnits");

        Assert.IsNotNull(feeChargeUnits.PrimaryKey);
        Assert.AreEqual("PK_FeeChargeUnits", feeChargeUnits.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(
            new[] { "FeeChargeId", "UnitId" },
            feeChargeUnits.PrimaryKey.Columns);

        Assert.AreEqual(2, feeChargeUnits.ForeignKeys.Count);

        var fkToFeeCharges = feeChargeUnits.ForeignKeys
            .Single(fk => fk.Name == "FK_FeeChargeUnits_FeeCharges_FeeChargeId");
        Assert.AreEqual("FeeCharges", fkToFeeCharges.PrincipalTable);
        Assert.AreEqual("Id", fkToFeeCharges.PrincipalColumns!.Single());
        Assert.AreEqual(ReferentialAction.Cascade, fkToFeeCharges.OnDelete);

        var fkToUnits = feeChargeUnits.ForeignKeys
            .Single(fk => fk.Name == "FK_FeeChargeUnits_Units_UnitId");
        Assert.AreEqual("Units", fkToUnits.PrincipalTable);
        Assert.AreEqual("Id", fkToUnits.PrincipalColumns!.Single());
        Assert.AreEqual(ReferentialAction.Cascade, fkToUnits.OnDelete);
    }

    [TestMethod]
    public void Up_Invoked_PaymentsTableHasExpectedColumnsAndForeignKey()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var payments = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "Payments");

        Assert.IsNotNull(payments.PrimaryKey);
        Assert.AreEqual("PK_Payments", payments.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(new[] { "Id" }, payments.PrimaryKey.Columns);

        var amountColumn = payments.Columns.Single(c => c.Name == "Amount");
        Assert.AreEqual(18, amountColumn.Precision);
        Assert.AreEqual(2, amountColumn.Scale);

        var referenceColumn = payments.Columns.Single(c => c.Name == "Reference");
        Assert.AreEqual(100, referenceColumn.MaxLength);
        Assert.IsTrue(referenceColumn.IsNullable);

        var notesColumn = payments.Columns.Single(c => c.Name == "Notes");
        Assert.AreEqual(1000, notesColumn.MaxLength);
        Assert.IsTrue(notesColumn.IsNullable);

        Assert.AreEqual(1, payments.ForeignKeys.Count);
        var foreignKey = payments.ForeignKeys.Single();
        Assert.AreEqual("FK_Payments_Units_UnitId", foreignKey.Name);
        Assert.AreEqual("Units", foreignKey.PrincipalTable);
        Assert.AreEqual("Id", foreignKey.PrincipalColumns!.Single());
        Assert.AreEqual(ReferentialAction.Restrict, foreignKey.OnDelete);
    }

    [TestMethod]
    public void Up_Invoked_UnitOwnersTableHasExpectedColumnsAndForeignKeys()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var unitOwners = migrationBuilder.Operations.OfType<CreateTableOperation>()
            .Single(op => op.Name == "UnitOwners");

        Assert.IsNotNull(unitOwners.PrimaryKey);
        Assert.AreEqual("PK_UnitOwners", unitOwners.PrimaryKey!.Name);
        CollectionAssert.AreEquivalent(new[] { "Id" }, unitOwners.PrimaryKey.Columns);

        var effectiveToColumn = unitOwners.Columns.Single(c => c.Name == "EffectiveTo");
        Assert.IsTrue(effectiveToColumn.IsNullable);

        var effectiveFromColumn = unitOwners.Columns.Single(c => c.Name == "EffectiveFrom");
        Assert.IsFalse(effectiveFromColumn.IsNullable);

        Assert.AreEqual(2, unitOwners.ForeignKeys.Count);

        var fkToOwners = unitOwners.ForeignKeys
            .Single(fk => fk.Name == "FK_UnitOwners_Owners_OwnerId");
        Assert.AreEqual("Owners", fkToOwners.PrincipalTable);
        Assert.AreEqual("Id", fkToOwners.PrincipalColumns!.Single());
        Assert.AreEqual(ReferentialAction.Restrict, fkToOwners.OnDelete);

        var fkToUnits = unitOwners.ForeignKeys
            .Single(fk => fk.Name == "FK_UnitOwners_Units_UnitId");
        Assert.AreEqual("Units", fkToUnits.PrincipalTable);
        Assert.AreEqual("Id", fkToUnits.PrincipalColumns!.Single());
        Assert.AreEqual(ReferentialAction.Restrict, fkToUnits.OnDelete);
    }

    [TestMethod]
    public void Up_Invoked_UnitOwnersOwnerIdIndexIsUnique()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var index = migrationBuilder.Operations.OfType<CreateIndexOperation>()
            .Single(op => op.Name == "IX_UnitOwners_OwnerId");
        Assert.IsTrue(index.IsUnique);
        Assert.AreEqual("UnitOwners", index.Table);
        CollectionAssert.AreEquivalent(new[] { "OwnerId" }, index.Columns);
    }

    [TestMethod]
    public void Up_Invoked_UnitOwnersUnitIdEffectiveFromIndexIsUniqueCompositeColumns()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var index = migrationBuilder.Operations.OfType<CreateIndexOperation>()
            .Single(op => op.Name == "IX_UnitOwners_UnitId_EffectiveFrom");
        Assert.IsTrue(index.IsUnique);
        Assert.AreEqual("UnitOwners", index.Table);
        CollectionAssert.AreEqual(new[] { "UnitId", "EffectiveFrom" }, index.Columns);
    }

    [TestMethod]
    public void Up_Invoked_NonUniqueIndexesAreNotUnique()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeUp(migrationBuilder);

        // Assert
        var feeChargeUnitsIndex = migrationBuilder.Operations.OfType<CreateIndexOperation>()
            .Single(op => op.Name == "IX_FeeChargeUnits_UnitId");
        Assert.IsFalse(feeChargeUnitsIndex.IsUnique);

        var paymentsIndex = migrationBuilder.Operations.OfType<CreateIndexOperation>()
            .Single(op => op.Name == "IX_Payments_UnitId");
        Assert.IsFalse(paymentsIndex.IsUnique);
    }

    [TestMethod]
    public void Down_Invoked_DropsAllSixTables()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeDown(migrationBuilder);

        // Assert
        var dropTableOps = migrationBuilder.Operations.OfType<DropTableOperation>().ToList();
        var tableNames = dropTableOps.Select(op => op.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { "FeeChargeUnits", "Payments", "UnitOwners", "FeeCharges", "Owners", "Units" },
            tableNames);
        Assert.AreEqual(6, dropTableOps.Count);
    }

    [TestMethod]
    public void Down_Invoked_DropsDependentTablesBeforePrincipalTables()
    {
        // Arrange
        var migration = new TestableInitialCreate();
        var migrationBuilder = CreateBuilder();

        // Act
        migration.InvokeDown(migrationBuilder);

        // Assert
        var dropTableOps = migrationBuilder.Operations.OfType<DropTableOperation>().ToList();
        var feeChargeUnitsIndex = dropTableOps.FindIndex(op => op.Name == "FeeChargeUnits");
        var feeChargesIndex = dropTableOps.FindIndex(op => op.Name == "FeeCharges");
        var unitsIndex = dropTableOps.FindIndex(op => op.Name == "Units");
        var unitOwnersIndex = dropTableOps.FindIndex(op => op.Name == "UnitOwners");
        var ownersIndex = dropTableOps.FindIndex(op => op.Name == "Owners");
        var paymentsIndex = dropTableOps.FindIndex(op => op.Name == "Payments");

        Assert.IsTrue(feeChargeUnitsIndex < feeChargesIndex);
        Assert.IsTrue(feeChargeUnitsIndex < unitsIndex);
        Assert.IsTrue(unitOwnersIndex < ownersIndex);
        Assert.IsTrue(unitOwnersIndex < unitsIndex);
        Assert.IsTrue(paymentsIndex < unitsIndex);
    }
}
