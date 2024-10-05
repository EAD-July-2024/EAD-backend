using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/category")]
    public class CategoryController(CategoryRepository categoryRepository) : Controller
    {
        private readonly CategoryRepository _categoryRepository = categoryRepository;

        // Method to generate a unique Category ID
        private async Task<string> GenerateUniqueCustomIdAsync()
        {
            var random = new Random();
            string customId;
            bool exists;

            do
            {
                customId = "C" + random.Next(0, 99999).ToString("D5");
                exists = await _categoryRepository.GetExistingIdsAsync(customId);
            }
            while (exists);

            return customId;
        }

        // GET all categories
        [HttpGet]
        public async Task<List<Category>> Get()
        {
            return await _categoryRepository.GetAsync();
        }

        // GET category by custom ID
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetByCustomId(string categoryId)
        {
            var category = await _categoryRepository.GetByCustomIdAsync(categoryId);
            if (category == null)
            {
                return NotFound("Category not found");
            }
            return Ok(category);
        }

        // Create a new category
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            // Generate custom CategoryId
            category.CategoryId = await GenerateUniqueCustomIdAsync();
            await _categoryRepository.CreateAsync(category);
            return Ok(category);
        }

        // Update an existing category
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Update(string categoryId, [FromBody] Category updatedCategory)
        {
            var existingCategory = await _categoryRepository.GetByCustomIdAsync(categoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found");
            }

            // Only update the required fields: Name, Description, and isDeleted
            await _categoryRepository.UpdateAsync(categoryId, updatedCategory.Name, updatedCategory.Description, updatedCategory.Status, updatedCategory.isDeleted);

            return Ok(updatedCategory);
        }


        // Update an existing category
        // [HttpPut("{categoryId}")]
        // public async Task<IActionResult> Update(string categoryId, [FromBody] Category? updatedCategory)
        // {
        //     // Check if the updatedCategory object is null
        //     if (updatedCategory == null)
        //     {
        //         return BadRequest("Category update data is missing.");
        //     }

        //     // Retrieve the existing category from the database
        //     var existingCategory = await _categoryRepository.GetByCustomIdAsync(categoryId);
        //     if (existingCategory == null)
        //     {
        //         return NotFound("Category not found");
        //     }

        //     // Only update the fields that are provided
        //     await _categoryRepository.UpdateAsync(
        //         categoryId,
        //         updatedCategory.Name ?? existingCategory.Name,  // Use the existing name if not provided
        //         updatedCategory.Description ?? existingCategory.Description,  // Use the existing description if not provided
        //         updatedCategory.Status ?? existingCategory.Status,  // Use the existing status if not provided
        //         updatedCategory.isDeleted  // This will always have a value (false or true)
        //     );

        //     return Ok(updatedCategory);
        // }


        // Delete a category
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeactivateCategory(string categoryId)
        {
            var category = await _categoryRepository.GetByCustomIdAsync(categoryId);
            if (category == null)
            {
                return NotFound($"Category with Custom ID {categoryId} not found");
            }

            category.isDeleted = true;
            await _categoryRepository.DeactivateCategoryAsync(category);
            return Ok($"Category with Custom ID {categoryId} has been deactivated.");
        }
    }
}
