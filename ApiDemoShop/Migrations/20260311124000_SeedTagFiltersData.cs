using ApiDemoShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    [DbContext(typeof(DemoShopDbContext))]
    [Migration("20260311124000_SeedTagFiltersData")]
    public partial class SeedTagFiltersData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [ProductType] WHERE [Title] = N'материал')
                    INSERT INTO [ProductType] ([Title]) VALUES (N'материал');

                IF NOT EXISTS (SELECT 1 FROM [ProductType] WHERE [Title] = N'тип посуды')
                    INSERT INTO [ProductType] ([Title]) VALUES (N'тип посуды');

                IF NOT EXISTS (SELECT 1 FROM [ProductType] WHERE [Title] = N'назначение')
                    INSERT INTO [ProductType] ([Title]) VALUES (N'назначение');

                DECLARE @TagSeed TABLE
                (
                    [TypeTitle] NVARCHAR(255) NOT NULL,
                    [TagTitle] NVARCHAR(255) NOT NULL
                );

                INSERT INTO @TagSeed ([TypeTitle], [TagTitle])
                VALUES
                    (N'материал', N'дерево'),
                    (N'материал', N'стекло'),
                    (N'материал', N'металл'),
                    (N'материал', N'керамика'),
                    (N'тип посуды', N'кружка'),
                    (N'тип посуды', N'кастрюля'),
                    (N'тип посуды', N'сковорода'),
                    (N'тип посуды', N'нож'),
                    (N'назначение', N'для хранения'),
                    (N'назначение', N'для приготовления'),
                    (N'назначение', N'для сервировки');

                INSERT INTO [Tag] ([Title], [TypeId])
                SELECT s.[TagTitle], pt.[id]
                FROM @TagSeed s
                INNER JOIN [ProductType] pt ON pt.[Title] = s.[TypeTitle]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [Tag] t
                    WHERE t.[Title] = s.[TagTitle]
                      AND t.[TypeId] = pt.[id]
                );

                DECLARE @ProductTagSeed TABLE
                (
                    [ProductRow] INT NOT NULL,
                    [TypeTitle] NVARCHAR(255) NOT NULL,
                    [TagTitle] NVARCHAR(255) NOT NULL
                );

                INSERT INTO @ProductTagSeed ([ProductRow], [TypeTitle], [TagTitle])
                VALUES
                    (1, N'тип посуды', N'кружка'),
                    (1, N'материал', N'керамика'),
                    (2, N'тип посуды', N'кастрюля'),
                    (2, N'материал', N'металл'),
                    (2, N'назначение', N'для приготовления'),
                    (3, N'тип посуды', N'сковорода'),
                    (3, N'материал', N'металл'),
                    (3, N'назначение', N'для приготовления'),
                    (4, N'тип посуды', N'нож'),
                    (4, N'материал', N'металл'),
                    (5, N'материал', N'стекло'),
                    (5, N'назначение', N'для сервировки'),
                    (6, N'материал', N'дерево'),
                    (6, N'назначение', N'для хранения');

                ;WITH ProductRows AS
                (
                    SELECT p.[id], ROW_NUMBER() OVER (ORDER BY p.[id]) AS [rn]
                    FROM [Product] p
                )
                INSERT INTO [ProductTag] ([ProductId], [TagId])
                SELECT pr.[id], t.[id]
                FROM @ProductTagSeed seed
                INNER JOIN ProductRows pr ON pr.[rn] = seed.[ProductRow]
                INNER JOIN [ProductType] pt ON pt.[Title] = seed.[TypeTitle]
                INNER JOIN [Tag] t ON t.[TypeId] = pt.[id] AND t.[Title] = seed.[TagTitle]
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM [ProductTag] existing
                    WHERE existing.[ProductId] = pr.[id]
                      AND existing.[TagId] = t.[id]
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
