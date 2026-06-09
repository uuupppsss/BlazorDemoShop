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
        [Authorize(Roles = "admin")]
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

        // Получить акции по продукту
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<PromotionDto?>> GetByProductId(int id)
        {
            var found_promotion = await _context.Promotions
                .Include(x => x.Product)
                //.Select(x => new PromotionDto
                //{
                //    Id = x.Id,
                //    Discount = x.Discount,
                //    StartDate = x.StartDate,
                //    EndDate = x.EndDate,
                //    ProductId = x.ProductId,
                //    ProductName = x.Product.Name
                //})
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (found_promotion == null)
                return Ok(new PromotionDto
                {
                    ProductId= id
                });
            var promotion = new PromotionDto()
            {
                Id = found_promotion.Id,
                Discount = found_promotion.Discount,
                StartDate = found_promotion.StartDate,
                EndDate = found_promotion.EndDate,
                ProductId = found_promotion.ProductId,
                ProductName = found_promotion.Product.Name
            };

            return Ok(promotion);
        }

        // Добавить акцию
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(CreateUpdatePromotionDto dto)
        {
            if (dto == null || dto.Discount <= 0)
                return BadRequest("Некоректные данные акции");

            var found_promotion = await _context.Promotions
               .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (found_promotion != null)
                return BadRequest("Акция для этого товара уже существует");
            

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
            if (dto == null || dto.Discount <= 0)
                return BadRequest("Некоректные данные акции");

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
