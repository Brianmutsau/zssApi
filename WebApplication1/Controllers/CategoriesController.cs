using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOS;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController(AppDbContext db) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<CategoryReadDto>> Create(CategoryCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title is required.");

            var exists = await db.Categories.AnyAsync(c => c.Title == dto.Title);
            if (exists) return Conflict("Category title already exists.");

            var cat = new Category { Title = dto.Title.Trim() };
            db.Categories.Add(cat);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cat.Id }, new CategoryReadDto(cat.Id, cat.Title));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryReadDto>> GetById(int id)
        {
            var cat = await db.Categories.FindAsync(id);
            if (cat is null) return NotFound();
            return new CategoryReadDto(cat.Id, cat.Title);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryReadDto>>> GetAll()
        {
            var list = await db.Categories
                .OrderBy(c => c.Title)
                .Select(c => new CategoryReadDto(c.Id, c.Title))
                .ToListAsync();
            return list;
        }
    }
}
