using ApiDemoShop.Data;
using ApiDemoShop.Model;
using LibDemoShop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiDemoShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryMethodsController : ControllerBase
    {
        private readonly DemoShopDbContext _context; 
        public DeliveryMethodsController(DemoShopDbContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet("all")]
        [Authorize (Roles ="admin")]
        public async Task<ActionResult<List<DeliveryMethodDTO>>> GetAll()
        {
            var result = await _context.DeliveryMethods
                .OrderBy(x=>x.Price)
                .Select(x=> new DeliveryMethodDTO
                {
                    Id=x.Id,
                    Name=x.Name,
                    Price=x.Price,
                    IsActive=x.IsActive
                })
                .ToListAsync();
            
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<DeliveryMethodDTO>>> GetActive()
        {
            var result = await _context.DeliveryMethods
                .OrderBy(x => x.Price)
                .Where(x=>x.IsActive)
                .Select(x => new DeliveryMethodDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateDeliveryMethod(int id, DeliveryMethodDTO dto)
        {
            if(dto.Id!=id) return NotFound("Некорректные данные");

            var found = await _context.DeliveryMethods
                .FirstOrDefaultAsync(x => x.Id == id);

            if (found==null) return NotFound("Запись не найдена");

            found.Name = dto.Name;
            found.Price = dto.Price;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateDeliveryMethod(CreateDeliveryMethodDTO dto)
        {

            var found = await _context.DeliveryMethods
                .FirstOrDefaultAsync(x => x.Name==dto.Name);

            if (found != null) return NotFound("Запись уже существует");

            var result = new DeliveryMethod()
            {
                Name=dto.Name,
                Price=dto.Price,
                IsActive=true
                
            };

            await _context.DeliveryMethods.AddAsync(result);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

        }

        [HttpPut("disable/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Disable(int id)
        {
            var found = await _context.DeliveryMethods
                .FirstOrDefaultAsync(x => x.Id == id);

            if (found == null) return NotFound("Запись не найдена");

            found.IsActive = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPut("enable/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Enable(int id)
        {
            var found = await _context.DeliveryMethods
                .FirstOrDefaultAsync(x => x.Id == id);

            if (found == null) return NotFound("Запись не найдена");

            found.IsActive = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
