using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiDemoShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly DemoShopDbContext _context;
        public ReviewsController(DemoShopDbContext context)
        {
            _context=context;
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponseDto>>> GetProductReviews(int productId)
        {
             var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

                return Ok(reviews);
            }

        // Добавить отзыв

        [Authorize(Roles = "user")]
        [HttpPost("{productId}")]
        public async Task<ActionResult> CreateReview(int productId, CreateUpdateReviewDto dto)
        {
            var userId = GetUserId();

            var productExists = await _context.Products
                .AnyAsync(p => p.Id == productId);

            if (!productExists)
                return NotFound("Товар не найден");

            // Один отзыв на товар от пользователя
            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == productId && r.UserId == userId);

            if (alreadyReviewed)
                return BadRequest("Вы уже оставляли отзыв");

            if (dto.Rating < 0 || dto.Rating > 5)
                return BadRequest("Рейтинг должен быть от 0 до 5");

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================================
        // Редактировать свой отзыв
        // =========================================
        [Authorize (Roles ="user")]
        [HttpPut("{reviewId}")]
        public async Task<ActionResult> UpdateReview(int reviewId, CreateUpdateReviewDto dto)
        {
            var userId = GetUserId();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return NotFound();

            // Только владелец
            if (review.UserId != userId)
                return Forbid();

            if (dto.Rating < 0 || dto.Rating > 5)
                return BadRequest("Рейтинг должен быть от 0 до 5");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================================
        // Удалить свой отзыв
        // =========================================
        [Authorize (Roles="user")]
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult> DeleteOwnReview(int reviewId)
        {
            var userId = GetUserId();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return NotFound();

            // Только владелец
            if (review.UserId != userId)
                return Forbid();

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================================
        // Админ удаляет любой отзыв
        // =========================================
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{reviewId}")]
        public async Task<ActionResult> AdminDeleteReview(int reviewId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // =========================================
        // Получение ID пользователя из JWT
        // =========================================
        private int GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.Parse(userId!);
        }

    }
}
