using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace ApiDemoShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedProductsController : ControllerBase
    {
        private const string FallbackImageUrl = "https://placehold.co/640x420/efe4d4/5f4638?text=No+Image";
        private readonly DemoShopDbContext _context;

        public SavedProductsController(DemoShopDbContext context)
        {
            _context = context;
        }

        [HttpPost("toggle/{productId}")]
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            var userId = GetCurrentUserId();

            var savedProduct = await _context.SavedProducts
                .FirstOrDefaultAsync(x => x.ProductId == productId &&
                                          x.UserId == userId);

            // Если товар уже в избранном — удалить
            if (savedProduct != null)
            {
                _context.SavedProducts.Remove(savedProduct);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    isFavorite = false,
                    message = "Товар удален из избранного"
                });
            }

            // Если товара нет в избранном — добавить
            var newSavedProduct = new SavedProduct
            {
                ProductId = productId,
                UserId = userId
            };

            _context.SavedProducts.Add(newSavedProduct);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                isFavorite = true,
                message = "Товар добавлен в избранное"
            });
        }

        // Получить все избранные товары пользователя
        [Authorize (Roles ="user")]
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetCurrentUserId();

            var favorites = await _context.SavedProducts
                .Where(x => x.UserId == userId)
                .Select(x => new ProductCardDTO
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.Product.Price,
                    Count = x.Product.Count,
                    MainImage = x.Product.ProductImages
                    .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .FirstOrDefault() ?? FallbackImageUrl
                })
                .ToListAsync();

            return Ok(favorites);
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }
    }
}
