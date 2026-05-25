using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    public class PromotionsController : ControllerBase
    {
        private readonly DemoShopDbContext _context;

        public PromotionsController(DemoShopDbContext context)
        {
            _context = context;
        }

        // Получить все акции
        [HttpGet]
        public async Task<ActionResult<List<PromotionDto>>> GetAll()
        {
            var promotions = await _context.Promotions
                .Include(x => x.Product)
                .Select(x => new PromotionDto
                {
                    Id = x.Id,
                    Discount = x.Discount,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name
                })
                .ToListAsync();

            return Ok(promotions);
        }

        // Добавить акцию
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(CreateUpdatePromotionDto dto)
        {
            var promotion = new Promotion
            {
                Discount = dto.Discount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProductId = dto.ProductId
            };

            _context.Promotions.Add(promotion);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Обновить акцию
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(
            int id,
            CreateUpdatePromotionDto dto)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (promotion == null)
                return NotFound();

            promotion.Discount = dto.Discount;
            promotion.StartDate = dto.StartDate;
            promotion.EndDate = dto.EndDate;
            promotion.ProductId = dto.ProductId;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // Удалить акцию
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (promotion == null)
                return NotFound();

            _context.Promotions.Remove(promotion);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
