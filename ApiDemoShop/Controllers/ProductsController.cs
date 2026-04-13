using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private const string FallbackImageUrl = "https://placehold.co/640x420/efe4d4/5f4638?text=No+Image";
        private const int MaxImagesPerProduct = 5;
        private const long MaxImageSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

        private readonly DemoShopDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(DemoShopDbContext dbContext, IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _environment = environment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDTO<ProductCardDTO>>> GetProducts(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 12,
            [FromQuery] int[]? tagIds = null,
            CancellationToken cancellationToken = default)
        {
            var safeSkip = Math.Max(0, skip);
            var safeTake = Math.Clamp(take, 1, 40);
            var normalizedTagIds = NormalizeTagIds(tagIds);

            var productsQuery = _dbContext.Products
                .AsNoTracking()
                .AsQueryable();

            foreach (var tagId in normalizedTagIds)
            {
                var localTagId = tagId;
                productsQuery = productsQuery.Where(p => p.ProductTags.Any(pt => pt.TagId == localTagId));
            }

            var totalCount = await productsQuery.CountAsync(cancellationToken);

            var items = await productsQuery
                .OrderBy(x => x.Id)
                .Skip(safeSkip)
                .Take(safeTake)
                .Select(x => new ProductCardDTO
                {
                    Id = x.Id,
                    Count = x.Count,
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

        [HttpGet("tag-filters")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProductTypeDTO>>> GetTagFilters(CancellationToken cancellationToken = default)
        {
            var filters = await _dbContext.ProductTypes
                .AsNoTracking()
                .Where(x => x.Tags.Any())
                .OrderBy(x => x.Title)
                .Select(x => new ProductTypeDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    Tags = x.Tags
                        .OrderBy(t => t.Title)
                        .Select(t => new TagDTO
                        {
                            Id = t.Id,
                            Title = t.Title,
                            TypeId = t.TypeId,
                            TypeTitle = x.Title
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return Ok(filters);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ProductDTO>> CreateProduct(
            [FromBody] CreateProductDTO request,
            CancellationToken cancellationToken = default)
        {
            var name = request.Name?.Trim();
            var description = NormalizeDescription(request.Description);
            var normalizedTagIds = NormalizeTagIds(request.TagIds);
            var normalizedImageUrls = NormalizeImageUrls(request.ImageUrls);

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Название товара обязательно.");
            }

            if (name.Length > 50)
            {
                return BadRequest("Название товара не должно превышать 50 символов.");
            }

            if (description is not null && description.Length > 255)
            {
                return BadRequest("Описание товара не должно превышать 255 символов.");
            }

            if (request.Price <= 0)
            {
                return BadRequest("Цена товара должна быть больше нуля.");
            }

            if (normalizedImageUrls.Count > MaxImagesPerProduct)
            {
                return BadRequest($"Допускается не более {MaxImagesPerProduct} картинок на товар.");
            }

            var product = new Product
            {
                Name = name,
                Description = description,
                Price = request.Price,
                Count = Math.Max(0, request.Count),
                TimeBought = Math.Max(0, request.TimeBought)
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (normalizedTagIds.Length > 0)
            {
                var existingTagIds = await _dbContext.Tags
                    .AsNoTracking()
                    .Where(t => normalizedTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync(cancellationToken);

                if (existingTagIds.Count > 0)
                {
                    var productTags = existingTagIds.Select(tagId => new ProductTag
                    {
                        ProductId = product.Id,
                        TagId = tagId
                    });

                    _dbContext.ProductTags.AddRange(productTags);
                }
            }

            if (normalizedImageUrls.Count > 0)
            {
                var productImages = normalizedImageUrls.Select(url => new ProductImage
                {
                    ProductId = product.Id,
                    Image = url
                });

                _dbContext.ProductImages.AddRange(productImages);
            }

            if (normalizedTagIds.Length > 0 || normalizedImageUrls.Count > 0)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var createdProduct = await BuildProductDetailsQuery()
                .FirstAsync(x => x.Id == product.Id, cancellationToken);

            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPost("images/upload")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<List<string>>> UploadImages(
            [FromForm] List<IFormFile> files,
            CancellationToken cancellationToken = default)
        {
            if (files.Count == 0)
            {
                return BadRequest("Не выбраны файлы для загрузки.");
            }

            if (files.Count > MaxImagesPerProduct)
            {
                return BadRequest($"Можно загрузить не более {MaxImagesPerProduct} картинок за один раз.");
            }

            var webRootPath = _environment.WebRootPath
                ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadFolder = Path.Combine(webRootPath, "uploads", "products");

            Directory.CreateDirectory(uploadFolder);

            var uploadedUrls = new List<string>(files.Count);

            foreach (var file in files)
            {
                if (file.Length <= 0)
                {
                    return BadRequest("Один из выбранных файлов пуст.");
                }

                if (file.Length > MaxImageSizeBytes)
                {
                    return BadRequest("Размер одного файла не должен превышать 10 МБ.");
                }

                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
                {
                    return BadRequest("Поддерживаются только изображения: .jpg, .jpeg, .png, .webp, .gif.");
                }

                var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
                var savePath = Path.Combine(uploadFolder, fileName);

                await using (var fileStream = new FileStream(savePath, FileMode.CreateNew))
                {
                    await file.CopyToAsync(fileStream, cancellationToken);
                }

                var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/products/{fileName}";
                uploadedUrls.Add(publicUrl);
            }

            return Ok(uploadedUrls);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ProductDTO>> UpdateProduct(
            int id,
            [FromBody] UpdateProductDTO request,
            CancellationToken cancellationToken = default)
        {
            if (request.Id > 0 && request.Id != id)
            {
                return BadRequest("Id в теле запроса не совпадает с id в URL.");
            }

            var product = await _dbContext.Products
                .Include(x => x.ProductTags)
                .Include(x => x.ProductImages)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            if (request.Name is not null)
            {
                var normalizedName = request.Name.Trim();

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    return BadRequest("Название товара обязательно.");
                }

                if (normalizedName.Length > 50)
                {
                    return BadRequest("Название товара не должно превышать 50 символов.");
                }

                product.Name = normalizedName;
            }

            if (request.Description is not null)
            {
                var normalizedDescription = NormalizeDescription(request.Description);

                if (normalizedDescription is not null && normalizedDescription.Length > 255)
                {
                    return BadRequest("Описание товара не должно превышать 255 символов.");
                }

                product.Description = normalizedDescription;
            }

            if (request.Price.HasValue)
            {
                if (request.Price.Value <= 0)
                {
                    return BadRequest("Цена товара должна быть больше нуля.");
                }

                product.Price = request.Price.Value;
            }

            if (request.Count.HasValue)
            {
                product.Count = Math.Max(0, request.Count.Value);
            }

            if (request.TimeBought.HasValue)
            {
                product.TimeBought = Math.Max(0, request.TimeBought.Value);
            }

            if (request.TagIds is not null)
            {
                var desiredTagIds = NormalizeTagIds(request.TagIds);
                var existingTagIds = await _dbContext.Tags
                    .AsNoTracking()
                    .Where(t => desiredTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToArrayAsync(cancellationToken);

                var desiredTagIdsSet = existingTagIds.ToHashSet();
                var tagsToRemove = product.ProductTags
                    .Where(pt => !desiredTagIdsSet.Contains(pt.TagId))
                    .ToList();

                if (tagsToRemove.Count > 0)
                {
                    _dbContext.ProductTags.RemoveRange(tagsToRemove);
                }

                var currentTagIdsSet = product.ProductTags
                    .Select(pt => pt.TagId)
                    .ToHashSet();

                var tagsToAdd = desiredTagIdsSet
                    .Where(tagId => !currentTagIdsSet.Contains(tagId))
                    .Select(tagId => new ProductTag
                    {
                        ProductId = product.Id,
                        TagId = tagId
                    })
                    .ToList();

                if (tagsToAdd.Count > 0)
                {
                    _dbContext.ProductTags.AddRange(tagsToAdd);
                }
            }

            if (request.ImageUrls is not null)
            {
                var normalizedImageUrls = NormalizeImageUrls(request.ImageUrls);

                if (normalizedImageUrls.Count > MaxImagesPerProduct)
                {
                    return BadRequest($"Допускается не более {MaxImagesPerProduct} картинок на товар.");
                }

                if (product.ProductImages.Count > 0)
                {
                    _dbContext.ProductImages.RemoveRange(product.ProductImages);
                }

                if (normalizedImageUrls.Count > 0)
                {
                    var imagesToAdd = normalizedImageUrls.Select(url => new ProductImage
                    {
                        ProductId = product.Id,
                        Image = url
                    });

                    _dbContext.ProductImages.AddRange(imagesToAdd);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            var updatedProduct = await BuildProductDetailsQuery()
                .FirstAsync(x => x.Id == id, cancellationToken);

            return Ok(updatedProduct);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id, CancellationToken cancellationToken = default)
        {
            var product = await BuildProductDetailsQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        private IQueryable<ProductDTO> BuildProductDetailsQuery()
        {
            return _dbContext.Products
                .AsNoTracking()
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
                    Tags = x.ProductTags
                        .OrderBy(pt => pt.Tag.Title)
                        .Select(pt => new TagDTO
                        {
                            Id = pt.Tag.Id,
                            Title = pt.Tag.Title,
                            TypeId = pt.Tag.TypeId,
                            TypeTitle = pt.Tag.Type.Title
                        })
                        .ToList(),
                    MainImage = x.ProductImages
                        .OrderBy(i => i.Id)
                        .Select(i => i.Image)
                        .FirstOrDefault() ?? FallbackImageUrl
                });
        }

        private static int[] NormalizeTagIds(IEnumerable<int>? tagIds)
        {
            return (tagIds ?? Array.Empty<int>())
                .Where(id => id > 0)
                .Distinct()
                .ToArray();
        }

        private static List<string> NormalizeImageUrls(IEnumerable<string>? imageUrls)
        {
            return (imageUrls ?? Array.Empty<string>())
                .Select(url => url?.Trim())
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Cast<string>()
                .Distinct()
                .ToList();
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}
