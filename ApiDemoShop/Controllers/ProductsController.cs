using ApiDemoShop.Data;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ProductsController : ControllerBase
    {
        private const string FallbackImageUrl = "https://placehold.co/640x420/efe4d4/5f4638?text=No+Image";
        private readonly DemoShopDbContext _dbContext;

        public ProductsController(DemoShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDTO<ProductCardDTO>>> GetProducts(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 12,
            CancellationToken cancellationToken = default)
        {
            var safeSkip = Math.Max(0, skip);
            var safeTake = Math.Clamp(take, 1, 40);

            var totalCount = await _dbContext.Products
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var items = await _dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(safeSkip)
                .Take(safeTake)
                .Select(x => new ProductCardDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    MainImage = x.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .FirstOrDefault() ?? FallbackImageUrl
                })
                .ToListAsync(cancellationToken);

            return Ok(new PagedResultDTO<ProductCardDTO>
            {
                Items = items,
                TotalCount = totalCount
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id, CancellationToken cancellationToken = default)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductDTO
                {
                    Id = x.Id,
                    Count = x.Count,
                    Description = x.Description,
                    Name = x.Name,
                    Price = x.Price,
                    TimeBought = x.TimeBought,
                    Images = x.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .ToList(),
                    MainImage = x.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .FirstOrDefault() ?? FallbackImageUrl
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }
    }
}
