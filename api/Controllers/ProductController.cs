using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/product")]
    public class ProductController: Controller
    {

        private readonly ProductRepository _productRepository;

         public ProductController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("get")]
        [Authorize(Policy ="RequireAdminRole")]        

        public async Task<List<Product>> Get(){
            return await _productRepository.GetAsync();
            
        }

        [Authorize(Policy = "RequireVendorRole")]
        [HttpPost("create")]
        [ApiExplorerSettings(IgnoreApi = true)]  // This will hide it from Swagger
        public async Task<IActionResult> Post([FromForm] Product product, [FromForm] IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Save the image to a storage (local or cloud, e.g., AWS S3, Azure Blob, etc.)
                var imageUrl = await SaveImageAsync(imageFile); // Custom method to handle image upload
                product.ImageUrl = imageUrl;
            }

            await _productRepository.CreateAsync(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var filePath = Path.Combine("wwwroot/images", imageFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{imageFile.FileName}"; // This returns the relative path of the image
        }

        
    }
}