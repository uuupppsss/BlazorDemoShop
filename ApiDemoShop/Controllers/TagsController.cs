using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace ApiDemoShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class TagsController : ControllerBase
    {
        private readonly DemoShopDbContext _context;
        public TagsController(DemoShopDbContext context)
        {
            _context=context;
        }

        [AllowAnonymous]
        [HttpGet("types")]
        public async Task<IActionResult> GetAllTypes()
        {
            var types = await _context.ProductTypes
                .Select(x => new ProductTypeDTO
                {
                    Id = x.Id,
                    Title = x.Title
                })
                .ToListAsync();

            return Ok(types);
        }


        [HttpPost ("types")]
        public async Task<IActionResult> CreateType(CreateProductTypeDTO dto)
        {
            var type = new ProductType
            {
                Title = dto.Title
            };

            _context.ProductTypes.Add(type);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("types/{id}")]
        public async Task<IActionResult> Update(int id, CreateProductTypeDTO dto)
        {
            var type = await _context.ProductTypes.FindAsync(id);

            if (type == null)
                return NotFound();

            type.Title = dto.Title;

            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpDelete("types/{id}")]
        //public async Task<IActionResult> DeleteTypes(int id)
        //{
        //    var type = await _context.ProductTypes
        //        .Include(x => x.Tags)
        //        .FirstOrDefaultAsync(x => x.Id == id);

        //    if (type == null)
        //        return NotFound();

        //    if (type.Tags.Any())
        //        return BadRequest("Нельзя удалить тип, содержащий теги");

        //    _context.ProductTypes.Remove(type);

        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        [AllowAnonymous]
        [HttpGet("tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _context.Tags
                .Include(x => x.Type)
                .Select(x => new TagDTO
                {
                    Id = x.Id,
                    Title = x.Title,
                    TypeId = x.TypeId,
                    TypeTitle = x.Type.Title
                })
                .ToListAsync();

            return Ok(tags);
        }

        [HttpPost("tags")]
        public async Task<IActionResult> Create(CreateTagDTO dto)
        {
            var tag = new Tag
            {
                Title = dto.Title,
                TypeId = dto.TypeId
            };

            _context.Tags.Add(tag);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("tags/{id}")]
        public async Task<IActionResult> Update(int id, UpdateTagDTO dto)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
                return NotFound();

            tag.Title = dto.Title;
            tag.TypeId = dto.TypeId;

            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpDelete("tags/{id}")]
        //public async Task<IActionResult> DeleteTags(int id)
        //{
        //    var tag = await _context.Tags
        //        .Include(x => x.ProductTags)
        //        .FirstOrDefaultAsync(x => x.Id == id);

        //    if (tag == null)
        //        return NotFound();

        //    if (tag.ProductTags.Any())
        //        return BadRequest("Тег используется товарами");

        //    _context.Tags.Remove(tag);

        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}
    }
}
