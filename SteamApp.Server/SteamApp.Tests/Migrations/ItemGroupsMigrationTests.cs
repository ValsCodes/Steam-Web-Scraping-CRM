using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SteamApp.WebAPI.Migrations;

namespace SteamApp.Tests.Migrations;

[TestFixture]
public sealed class ItemGroupsMigrationTests
{
    [Test]
    public void UpOperations_RenameExistingTagTypeStorageWithoutDroppingRows()
    {
        var migration = new ItemGroupsForManualCheckPresets();
        var operations = migration.UpOperations;

        Assert.Multiple(() =>
        {
            Assert.That(
                operations.OfType<RenameTableOperation>().Any(x =>
                    x.Name == "tag_type" && x.NewName == "item_group"),
                Is.True);
            Assert.That(
                operations.OfType<RenameColumnOperation>().Any(x =>
                    x.Table == "tag" &&
                    x.Name == "tag_type_id" &&
                    x.NewName == "item_group_id"),
                Is.True);
            Assert.That(
                operations.OfType<AddColumnOperation>().Any(x =>
                    x.Table == "manual_check_preset" &&
                    x.Name == "item_group_id" &&
                    x.IsNullable),
                Is.True);
            Assert.That(operations.OfType<DropTableOperation>(), Is.Empty);
            Assert.That(operations.OfType<CreateTableOperation>(), Is.Empty);
        });
    }
}
