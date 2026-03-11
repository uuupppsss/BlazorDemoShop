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
                        (120, N'Керамическая кружка для кофе и чая, объем 350 мл.', N' Кружка Terra 350мл', 490.00, 17),
                        (40, N'Компактный чайник из нержавеющей стали для ежедневного использования.', N' Чайник Nordic 1.2л', 2290.00, 9),
                        (65, N'Термос с двойными стенками, держит тепло до 8 часов.', N'Термос Steel 0.5л', 1390.00, 13),
                        (80, N'Набор из шести столовых ложек из нержавеющей стали.', N'Набор ложек Maple x6', 790.00, 11),
                        (95, N'Обеденная тарелка из керамики диаметром 24 см.', N' Тарелка Sand 24см', 520.00, 7),
                        (55, N'Разделочная доска из дуба с масляной пропиткой.', N' Доска Oak 30см', 1290.00, 16),
                        (30, N'Поварской нож с лезвием 20 см для универсальной нарезки.', N' Нож Chef Pro 20см', 1990.00, 18),
                        (36, N'Антипригарная сковорода 28 см с утолщенным дном.', N' Сковорода Iron 28см', 2590.00, 14),
                        (28, N'Кастрюля объемом 3 литра с крышкой из жаропрочного стекла.', N'Кастрюля Home 3л', 2890.00, 6),
                        (110, N'Герметичный контейнер для хранения продуктов, 1 литр.', N' Контейнер Fresh 1л', 390.00, 10),
                        (24, N'Погружной блендер мощностью 600 Вт с двумя насадками.', N' Блендер Mini 600W', 3190.00, 8),
                        (32, N'Электрическая кофемолка для зерен с чашей 70 г.', N'Кофемолка Bean 150W', 2190.00, 5),
                        (70, N'Цифровые кухонные весы с пределом взвешивания 5 кг.', N' Весы Kitchen Max 5кг', 990.00, 12),
                        (60, N'Льняной фартук с карманом, регулируемая длина.', N' Фартук Linen Brown', 890.00, 4),
                        (88, N'Набор хлопковых салфеток для сервировки, 4 штуки.', N' Салфетки Cotton x4', 690.00, 3),
                        (26, N'Деревянный поднос размером 40 см с бортиками.', N' Поднос Walnut 40см', 1590.00, 9),
                        (34, N'Стеклянный графин на 1 литр для воды и лимонадов.', N' Графин Glass 1л', 1190.00, 7),
                        (75, N'Набор из двух стаканов по 300 мл из закаленного стекла.', N' Стакан Frost 300мл x2', 840.00, 15),
                        (102, N'Ланчбокс на 900 мл с плотной защелкой.', N' Ланчбокс City 900мл', 740.00, 6),
                        (66, N'Спортивная бутылка для воды на 700 мл.', N' Бутылка Sport 700мл', 620.00, 8),
                        (90, N'Силиконовая лопатка с термостойкой рабочей частью.', N' Лопатка Silicone Red', 360.00, 13),
                        (44, N'Настенная полка-органайзер для банок со специями.', N' Полка Spice Rack', 1890.00, 2),
                        (58, N'Механический кухонный таймер до 99 минут.', N' Таймер Cook 99мин', 540.00, 5),
                        (39, N'Базовый набор специй из 8 баночек для кухни.', N' Набор специй Basic x8', 1490.00, 11);

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
