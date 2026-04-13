using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "user")]
    public class BasketController : ControllerBase
    {
        private const string FallbackImageUrl = "https://placehold.co/640x420/efe4d4/5f4638?text=No+Image";
        private readonly DemoShopDbContext _dbContext;

        public BasketController(DemoShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<BasketItemDTO>>> GetBasketItems(CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var items = await BuildBasketItemsQuery(userId).ToListAsync(cancellationToken);
            return Ok(items);
        }

        [HttpPost("items")]
        public async Task<ActionResult<BasketItemDTO>> AddItem(
            [FromBody] CreateBasketItemDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.ProductId <= 0)
            {
                return BadRequest("Некорректный идентификатор товара.");
            }

            var safeCount = Math.Max(1, request.Count);

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var product = await _dbContext.Products
                .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                return NotFound("Товар не найден.");
            }

            if (product.Count < safeCount)
            {
                return BadRequest("Товара недостаточно на складе.");
            }

            var basketItem = await _dbContext.BasketItems
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == request.ProductId, cancellationToken);

            if (basketItem is null)
            {
                basketItem = new BasketItem
                {
                    ProductId = request.ProductId,
                    UserId = userId,
                    Count = safeCount
                };

                _dbContext.BasketItems.Add(basketItem);
            }
            else
            {
                basketItem.Count += safeCount;
            }

            product.Count -= safeCount;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await BuildBasketItemsQuery(userId)
                .FirstAsync(x => x.Id == basketItem.Id, cancellationToken);

            return Ok(response);
        }

        [HttpPut("items/{id:int}")]
        public async Task<ActionResult<BasketItemDTO>> UpdateItemCount(
            int id,
            [FromBody] UpdateBasketItemDTO request,
            CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            if (request.Id > 0 && request.Id != id)
            {
                return BadRequest("Id в теле запроса не совпадает с id в URL.");
            }

            if (request.Count < 1)
            {
                return BadRequest("Количество товара должно быть больше нуля.");
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var basketItem = await _dbContext.BasketItems
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

            if (basketItem is null)
            {
                return NotFound("Позиция корзины не найдена.");
            }

            var delta = request.Count - basketItem.Count;

            if (delta > 0)
            {
                if (basketItem.Product.Count < delta)
                {
                    return BadRequest("Товара недостаточно на складе.");
                }

                basketItem.Product.Count -= delta;
            }
            else if (delta < 0)
            {
                basketItem.Product.Count += -delta;
            }

            basketItem.Count = request.Count;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var response = await BuildBasketItemsQuery(userId)
                .FirstAsync(x => x.Id == basketItem.Id, cancellationToken);

            return Ok(response);
        }

        [HttpDelete("items/{id:int}")]
        public async Task<IActionResult> RemoveItem(int id, CancellationToken cancellationToken = default)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var basketItem = await _dbContext.BasketItems
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

            if (basketItem is null)
            {
                return NotFound("Позиция корзины не найдена.");
            }

            basketItem.Product.Count += basketItem.Count;
            _dbContext.BasketItems.Remove(basketItem);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return NoContent();
        }

        private IQueryable<BasketItemDTO> BuildBasketItemsQuery(int userId)
        {
            return _dbContext.BasketItems
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.Id)
                .Select(x => new BasketItemDTO
                {
                    Id = x.Id,
                    Count = x.Count,
                    ProductId = x.ProductId,
                    UserId = x.UserId,
                    ProductAvailableCount = x.Product.Count,
                    ProductName = x.Product.Name,
                    ProductPrice = x.Product.Price,
                    ProductImage = x.Product.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .FirstOrDefault() ?? FallbackImageUrl
                });
        }

        private bool TryGetCurrentUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out userId) && userId > 0;
        }
    }
}
