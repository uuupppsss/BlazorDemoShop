using ApiDemoShop.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiDemoShop.Migrations
{
    [DbContext(typeof(DemoShopDbContext))]
    [Migration("20260309110000_SeedTestProducts")]
    public partial class SeedTestProducts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Product] WHERE [Name] = N'[SEED] Кружка Terra 350мл')
                BEGIN
                    INSERT INTO [Product] ([Count], [Description], [Name], [Price], [TimeBought])
                    VALUES
                        (120, N'Керамическая кружка для кофе и чая, объем 350 мл.', N'[SEED] Кружка Terra 350мл', 490.00, 17),
                        (40, N'Компактный чайник из нержавеющей стали для ежедневного использования.', N'[SEED] Чайник Nordic 1.2л', 2290.00, 9),
                        (65, N'Термос с двойными стенками, держит тепло до 8 часов.', N'[SEED] Термос Steel 0.5л', 1390.00, 13),
                        (80, N'Набор из шести столовых ложек из нержавеющей стали.', N'[SEED] Набор ложек Maple x6', 790.00, 11),
                        (95, N'Обеденная тарелка из керамики диаметром 24 см.', N'[SEED] Тарелка Sand 24см', 520.00, 7),
                        (55, N'Разделочная доска из дуба с масляной пропиткой.', N'[SEED] Доска Oak 30см', 1290.00, 16),
                        (30, N'Поварской нож с лезвием 20 см для универсальной нарезки.', N'[SEED] Нож Chef Pro 20см', 1990.00, 18),
                        (36, N'Антипригарная сковорода 28 см с утолщенным дном.', N'[SEED] Сковорода Iron 28см', 2590.00, 14),
                        (28, N'Кастрюля объемом 3 литра с крышкой из жаропрочного стекла.', N'[SEED] Кастрюля Home 3л', 2890.00, 6),
                        (110, N'Герметичный контейнер для хранения продуктов, 1 литр.', N'[SEED] Контейнер Fresh 1л', 390.00, 10),
                        (24, N'Погружной блендер мощностью 600 Вт с двумя насадками.', N'[SEED] Блендер Mini 600W', 3190.00, 8),
                        (32, N'Электрическая кофемолка для зерен с чашей 70 г.', N'[SEED] Кофемолка Bean 150W', 2190.00, 5),
                        (70, N'Цифровые кухонные весы с пределом взвешивания 5 кг.', N'[SEED] Весы Kitchen Max 5кг', 990.00, 12),
                        (60, N'Льняной фартук с карманом, регулируемая длина.', N'[SEED] Фартук Linen Brown', 890.00, 4),
                        (88, N'Набор хлопковых салфеток для сервировки, 4 штуки.', N'[SEED] Салфетки Cotton x4', 690.00, 3),
                        (26, N'Деревянный поднос размером 40 см с бортиками.', N'[SEED] Поднос Walnut 40см', 1590.00, 9),
                        (34, N'Стеклянный графин на 1 литр для воды и лимонадов.', N'[SEED] Графин Glass 1л', 1190.00, 7),
                        (75, N'Набор из двух стаканов по 300 мл из закаленного стекла.', N'[SEED] Стакан Frost 300мл x2', 840.00, 15),
                        (102, N'Ланчбокс на 900 мл с плотной защелкой.', N'[SEED] Ланчбокс City 900мл', 740.00, 6),
                        (66, N'Спортивная бутылка для воды на 700 мл.', N'[SEED] Бутылка Sport 700мл', 620.00, 8),
                        (90, N'Силиконовая лопатка с термостойкой рабочей частью.', N'[SEED] Лопатка Silicone Red', 360.00, 13),
                        (44, N'Настенная полка-органайзер для банок со специями.', N'[SEED] Полка Spice Rack', 1890.00, 2),
                        (58, N'Механический кухонный таймер до 99 минут.', N'[SEED] Таймер Cook 99мин', 540.00, 5),
                        (39, N'Базовый набор специй из 8 баночек для кухни.', N'[SEED] Набор специй Basic x8', 1490.00, 11);

                    INSERT INTO [ProductImage] ([Image], [ProductId])
                    SELECT src.[Image], p.[id]
                    FROM (VALUES
                        (N'[SEED] Кружка Terra 350мл', N'https://picsum.photos/seed/demoshop-01/900/600'),
                        (N'[SEED] Чайник Nordic 1.2л', N'https://picsum.photos/seed/demoshop-02/900/600'),
                        (N'[SEED] Термос Steel 0.5л', N'https://picsum.photos/seed/demoshop-03/900/600'),
                        (N'[SEED] Набор ложек Maple x6', N'https://picsum.photos/seed/demoshop-04/900/600'),
                        (N'[SEED] Тарелка Sand 24см', N'https://picsum.photos/seed/demoshop-05/900/600'),
                        (N'[SEED] Доска Oak 30см', N'https://picsum.photos/seed/demoshop-06/900/600'),
                        (N'[SEED] Нож Chef Pro 20см', N'https://picsum.photos/seed/demoshop-07/900/600'),
                        (N'[SEED] Сковорода Iron 28см', N'https://picsum.photos/seed/demoshop-08/900/600'),
                        (N'[SEED] Кастрюля Home 3л', N'https://picsum.photos/seed/demoshop-09/900/600'),
                        (N'[SEED] Контейнер Fresh 1л', N'https://picsum.photos/seed/demoshop-10/900/600'),
                        (N'[SEED] Блендер Mini 600W', N'https://picsum.photos/seed/demoshop-11/900/600'),
                        (N'[SEED] Кофемолка Bean 150W', N'https://picsum.photos/seed/demoshop-12/900/600'),
                        (N'[SEED] Весы Kitchen Max 5кг', N'https://picsum.photos/seed/demoshop-13/900/600'),
                        (N'[SEED] Фартук Linen Brown', N'https://picsum.photos/seed/demoshop-14/900/600'),
                        (N'[SEED] Салфетки Cotton x4', N'https://picsum.photos/seed/demoshop-15/900/600'),
                        (N'[SEED] Поднос Walnut 40см', N'https://picsum.photos/seed/demoshop-16/900/600'),
                        (N'[SEED] Графин Glass 1л', N'https://picsum.photos/seed/demoshop-17/900/600'),
                        (N'[SEED] Стакан Frost 300мл x2', N'https://picsum.photos/seed/demoshop-18/900/600'),
                        (N'[SEED] Ланчбокс City 900мл', N'https://picsum.photos/seed/demoshop-19/900/600'),
                        (N'[SEED] Бутылка Sport 700мл', N'https://picsum.photos/seed/demoshop-20/900/600'),
                        (N'[SEED] Лопатка Silicone Red', N'https://picsum.photos/seed/demoshop-21/900/600'),
                        (N'[SEED] Полка Spice Rack', N'https://picsum.photos/seed/demoshop-22/900/600'),
                        (N'[SEED] Таймер Cook 99мин', N'https://picsum.photos/seed/demoshop-23/900/600'),
                        (N'[SEED] Набор специй Basic x8', N'https://picsum.photos/seed/demoshop-24/900/600')
                    ) AS src([Name], [Image])
                    INNER JOIN [Product] p ON p.[Name] = src.[Name];
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE b
                FROM [BasketItem] b
                INNER JOIN [Product] p ON p.[id] = b.[product_id]
                WHERE p.[Name] LIKE N'[SEED] %';

                DELETE oi
                FROM [OrderItem] oi
                INNER JOIN [Product] p ON p.[id] = oi.[ProductId]
                WHERE p.[Name] LIKE N'[SEED] %';

                DELETE sp
                FROM [SavedProduct] sp
                INNER JOIN [Product] p ON p.[id] = sp.[product_id]
                WHERE p.[Name] LIKE N'[SEED] %';

                DELETE pt
                FROM [ProductTag] pt
                INNER JOIN [Product] p ON p.[id] = pt.[ProductId]
                WHERE p.[Name] LIKE N'[SEED] %';

                DELETE pi
                FROM [ProductImage] pi
                INNER JOIN [Product] p ON p.[id] = pi.[ProductId]
                WHERE p.[Name] LIKE N'[SEED] %';

                DELETE FROM [Product]
                WHERE [Name] LIKE N'[SEED] %';
                """);
        }
    }
}
