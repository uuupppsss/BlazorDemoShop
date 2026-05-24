using ApiDemoShop.Data;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace ApiDemoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly DemoShopDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

        public BannersController(
            DemoShopDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var banners = await _context.Banners
                .OrderBy(x => x.Order)
                .ToListAsync();

            return Ok(banners);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл пуст");

            var bannersCount = await _context.Banners.CountAsync();

            if (bannersCount >= 5)
                return BadRequest("Максимум 5 баннеров");

            var webRootPath = _env.WebRootPath
                ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadFolder = Path.Combine(webRootPath, "uploads", "banners");

            Directory.CreateDirectory(uploadFolder);

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

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/banners/{fileName}";
         
            var banner = new Banner
            {
                ImageUrl = publicUrl,
                Order = bannersCount + 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Banners.Add(banner);

            await _context.SaveChangesAsync();

            return Ok(banner);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
                return NotFound();

            var fullPath = Path.Combine(
                _env.WebRootPath,
                banner.ImageUrl.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.Banners.Remove(banner);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, Banner updatedBanner)
        {
            var banner = await _context.Banners.FindAsync(id);

            if (banner == null)
                return NotFound();

            banner.Order = updatedBanner.Order;
            banner.IsActive = updatedBanner.IsActive;

            await _context.SaveChangesAsync();

            return Ok(banner);
        }
    }
}
