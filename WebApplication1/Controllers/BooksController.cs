using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebApplication1.DTOS;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController(AppDbContext db) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<BookReadDto>> Create(BookCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");
            if (dto.Price <= 0) return BadRequest("Price must be greater than 0.");

            var cat = await db.Categories.FindAsync(dto.CategoryId);
            if (cat is null) return BadRequest("Category does not exist.");

            var book = new Book
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                CategoryId = dto.CategoryId
            };
            db.Books.Add(book);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = book.Id },
                new BookReadDto(book.Id, book.Title, book.Description, book.Price, book.CategoryId, cat.Title));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookReadDto>> GetById(int id)
        {
            var b = await db.Books.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (b is null) return NotFound();
            return new BookReadDto(b.Id, b.Title, b.Description, b.Price, b.CategoryId, b.Category?.Title ?? "");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookReadDto>>> GetAll([FromQuery] int? categoryId = null)
        {
            var q = db.Books.Include(b => b.Category).AsQueryable();
            if (categoryId is not null) q = q.Where(b => b.CategoryId == categoryId);

            var list = await q.OrderBy(b => b.Title)
                .Select(b => new BookReadDto(b.Id, b.Title, b.Description, b.Price, b.CategoryId, b.Category!.Title))
                .ToListAsync();

            return list;
        }
    }
}
