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

        //[Authorize(Policy =UserRoles.CSR)]        

        [HttpGet("get")]
        public async Task<List<Product>> Get(){
            return await _productRepository.GetAsync();
            
        }
        
            
        [HttpPost("create")]
        public async Task<IActionResult> Post([FromForm] Product product, [FromForm] IFormFile image){

            if (image != null && image.Length > 0)
            {
                
                var imageUrl = await SaveImageAsync(image); 
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
        
            return $"/images/{imageFile.FileName}"; 
        }
        
    }
}