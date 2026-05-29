using ApiDemoShop.Model;
using LibDemoShop;

namespace ApiDemoShop.Services
{
    public static class PromotionHelper
    {
        public static Promotion? GetActivePromotion(Product product)
        {
            var now = DateTime.Now;

            return product.Promotions
                .FirstOrDefault(x =>
                    x.StartDate <= now &&
                    x.EndDate >= now);
        }

        public static PromoInfo FillDiscountInfo(Product product)
        {
            PromoInfo result = new PromoInfo();
            var promotion = GetActivePromotion(product);

            if (promotion == null)
            {
                result.HasDiscount = false;
                result.FinalPrice = (double)product.Price;
                result.DiscountPercent = 0;

                return result;
            }

            result.HasDiscount = true;

            result.OldPrice = (double)product.Price;

            result.DiscountPercent = promotion.Discount;

            result.FinalPrice =
                (double)(product.Price -
                product.Price *
                (decimal)promotion.Discount / 100m);
            result.EndDate=promotion.EndDate;

            return result;
        }

        public static decimal GetFinalPrice(Product product)
        {
            var promotion = GetActivePromotion(product);

            if (promotion == null)
                return product.Price;

            return product.Price -
                   product.Price *
                   (decimal)promotion.Discount / 100m;
        }
    }
}
