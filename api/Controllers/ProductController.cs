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
        
    }
}