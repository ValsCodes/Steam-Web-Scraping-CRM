using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SteamApp.WebAPI.Migrations;

namespace SteamApp.Tests.Migrations;

[TestFixture]
public sealed class GameUrlItemGroupMigrationTests
{
    [Test]
    public void UpOperations_OptionalGroup_AddsNullableColumnIndexAndRestrictedForeignKey()
    {
        var operations = new AddItemGroupToGameUrls().UpOperations;
        var column = operations.OfType<AddColumnOperation>().Single();
        var index = operations.OfType<CreateIndexOperation>().Single();
        var foreignKey = operations.OfType<AddForeignKeyOperation>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(column.Table, Is.EqualTo("game_url"));
            Assert.That(column.Name, Is.EqualTo("item_group_id"));
            Assert.That(column.IsNullable, Is.True);
            Assert.That(column.DefaultValue, Is.Null);
            Assert.That(index.Table, Is.EqualTo("game_url"));
            Assert.That(index.Columns, Is.EqualTo(new[] { "item_group_id" }));
            Assert.That(foreignKey.Table, Is.EqualTo("game_url"));
            Assert.That(foreignKey.Columns, Is.EqualTo(new[] { "item_group_id" }));
            Assert.That(foreignKey.PrincipalTable, Is.EqualTo("item_group"));
            Assert.That(foreignKey.PrincipalColumns, Is.EqualTo(new[] { "id" }));
            Assert.That(foreignKey.OnDelete, Is.EqualTo(ReferentialAction.Restrict));
            Assert.That(operations, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public void DownOperations_OptionalGroup_RemovesOnlyTheGroupRelationship()
    {
        var operations = new AddItemGroupToGameUrls().DownOperations;

        Assert.Multiple(() =>
        {
            Assert.That(operations.OfType<DropForeignKeyOperation>().Single().Table, Is.EqualTo("game_url"));
            Assert.That(operations.OfType<DropIndexOperation>().Single().Name, Is.EqualTo("IX_game_url_item_group_id"));
            Assert.That(operations.OfType<DropColumnOperation>().Single().Name, Is.EqualTo("item_group_id"));
            Assert.That(operations, Has.Count.EqualTo(3));
        });
    }
}
