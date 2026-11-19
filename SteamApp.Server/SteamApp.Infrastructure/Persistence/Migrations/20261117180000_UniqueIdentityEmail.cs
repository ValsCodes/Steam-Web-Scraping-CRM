using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SteamApp.Infrastructure.Persistence;

namespace SteamApp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261117180000_UniqueIdentityEmail")]
public sealed class UniqueIdentityEmail : Migration
{
    internal const string CreateIndexSql = """
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes
            WHERE [object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]')
              AND [name] = N'EmailIndex' AND [is_unique] = 1
        )
        BEGIN
            IF EXISTS (
                SELECT [NormalizedEmail] FROM [dbo].[AspNetUsers]
                WHERE [NormalizedEmail] IS NOT NULL
                GROUP BY [NormalizedEmail] HAVING COUNT(*) > 1
            )
                THROW 51000, 'Resolve duplicate normalized Identity emails before enabling the unique email index.', 1;

            IF EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE [object_id] = OBJECT_ID(N'[dbo].[AspNetUsers]') AND [name] = N'EmailIndex'
            )
                CREATE UNIQUE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail])
                    WHERE [NormalizedEmail] IS NOT NULL WITH (DROP_EXISTING = ON);
            ELSE
                CREATE UNIQUE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail])
                    WHERE [NormalizedEmail] IS NOT NULL;
        END;
        """;

    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql(CreateIndexSql);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("""
            CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]) WITH (DROP_EXISTING = ON);
            """);
}
