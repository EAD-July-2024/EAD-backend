using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace api.Controllers
{
    [Controller]
    [Route("api/product")]
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepository;

        public ProductController(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet("get")]
        //[Authorize(Policy = "RequireAdminRole")]
        public async Task<List<Product>> Get()
        {
            return await _productRepository.GetAsync();
        }

        [HttpPost("create")]
        //[Authorize(Policy = "RequireAdminRole")]
        public async Task<IActionResult> Create([FromForm] Product product, List<IFormFile> images)
        {
            // Generate the Custom ID
            var random = new Random();
            product.ProductId = await GenerateUniqueCustomIdAsync();

            if (images.Count > 5)
            {
                return BadRequest("You can upload up to 5 images.");
            }

            var imageStreams = new List<Stream>();
            foreach (var image in images)
            {
                var stream = image.OpenReadStream();
                imageStreams.Add(stream);
            }

            await _productRepository.CreateAsync(product, imageStreams);
            return Ok(product);
        }

        // Method to generate a unique Product ID
        private async Task<string> GenerateUniqueCustomIdAsync()
        {
            var random = new Random();
            string customId;
            bool exists;

            do
            {
                
                customId = "P" + random.Next(0, 99999).ToString("D5");

                
                exists = await _productRepository.getExistingIds(customId);
            } 
            while (exists); 

            return customId;
        }

        //
        //Get product details by product custom id
        [HttpGet("getByCustomId/{customId}")]
        public async Task<IActionResult> GetByCustomId(string customId)
        {
            var product = await _productRepository.GetByCustomIdAsync(customId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(product);
        }

    }
}
