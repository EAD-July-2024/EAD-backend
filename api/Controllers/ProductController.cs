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
    }
}
