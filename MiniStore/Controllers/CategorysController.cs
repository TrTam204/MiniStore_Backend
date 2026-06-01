using Microsoft.AspNetCore.Mvc;
using MiniStore.DTOs.Category;
using MiniStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace MiniStore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);
            return Ok(category);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();

            return Ok(categories);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {return NotFound();}
            return Ok(category);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
        {
            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null)
            {return NotFound();}
            return Ok(category);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {return NotFound();}
            return NoContent();
        }
    }
}