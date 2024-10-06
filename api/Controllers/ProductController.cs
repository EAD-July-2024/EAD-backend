using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using api.DTOs;

namespace api.Controllers
{
    [Controller]
    [Route("api/product")]
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderItemRepository _orderItemRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly UserRepository _userRepository;

        public ProductController(ProductRepository productRepository, OrderItemRepository orderItemRepository, CategoryRepository categoryRepository, UserRepository userRepository)
        {
            _productRepository = productRepository;
            _orderItemRepository = orderItemRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        // Get all products
        // [HttpGet]
        // //[Authorize(Policy = "RequireAdminRole")]
        // public async Task<List<Product>> Get()
        // {
        //     return await _productRepository.GetAsync();
        // }

        // Get all products with CategoryName and VendorName
        [HttpGet]
        public async Task<IActionResult> GetProductsWithDetails()
        {
            // Fetch all products
            var products = await _productRepository.GetAsync();

            // List to hold the result
            var productsWithDetails = new List<ProductWithDetailsDTO>();


            // For each product, fetch the associated Category and Vendor details
            foreach (var product in products)
            {

                if (product.IsDeleted == true)
                {
                    continue;
                }

                // Fetch category details
                var category = await _categoryRepository.GetByCustomIdAsync(product.CategoryId);
                if (category == null)
                {
                    return NotFound($"Category with ID {product.CategoryId} not found.");
                }

                // Fetch vendor details
                var vendor = await _userRepository.GetUserByIdAsync(product.VendorId);
                if (vendor == null)
                {
                    return NotFound($"Vendor with ID {product.VendorId} not found.");
                }

                // Create a new ProductWithDetailsDTO
                var productWithDetails = new ProductWithDetailsDTO
                {
                    Id = product.Id.ToString(),
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    CategoryId = product.CategoryId,
                    CategoryName = category.Name,
                    VendorId = product.VendorId,
                    VendorName = vendor.FullName,
                    ImageUrls = product.ImageUrls,
                    IsDeleted = product.IsDeleted
                };

                productsWithDetails.Add(productWithDetails);
            }

            // Return the list of products with their details
            return Ok(productsWithDetails);
        }

        // Get product details by productId with CategoryName and VendorName
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByCustomId(string productId)
        {
            // Fetch the product by productId
            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found.");
            }

            // Fetch category details
            var category = await _categoryRepository.GetByCustomIdAsync(product.CategoryId);
            if (category == null)
            {
                return NotFound($"Category with ID {product.CategoryId} not found.");
            }

            // Fetch vendor details
            var vendor = await _userRepository.GetUserByIdAsync(product.VendorId);
            if (vendor == null)
            {
                return NotFound($"Vendor with ID {product.VendorId} not found.");
            }

            // Create a new ProductWithDetailsDTO
            var productWithDetails = new ProductWithDetailsDTO
            {
                Id = product.Id.ToString(),
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = category.Name,
                VendorId = product.VendorId,
                VendorName = vendor.FullName,
                ImageUrls = product.ImageUrls,
                IsDeleted = product.IsDeleted
            };

            // Return the product with details
            return Ok(productWithDetails);
        }

        [HttpPost]
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
        /*[HttpGet("{productId}")]
        public async Task<IActionResult> GetByCustomId(string productId)
        {
            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(product);
        }*/





        // Update the stock level of a product
        [HttpPatch("quantity/{productId}")]
        public async Task<IActionResult> UpdateProductStock(string productId, [FromBody] int newQuantity)
        {
            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            await _productRepository.UpdateQuantityAsync(productId, newQuantity);
            return Ok($"Stock level updated for product {product.Name}. New quantity: {newQuantity}");
        }

        // Get the current stock level of a product
        [HttpGet("quantity/{productId}")]
        public async Task<IActionResult> GetProductStock(string productId)
        {
            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(new { product.ProductId, product.Name, product.Quantity });
        }

        // Endpoint to update IsDeleted status
        [HttpDelete("productDelete")]
        public async Task<IActionResult> UpdateIsDeleted([FromBody] UpdateIsDeletedRequest request)
        {


            bool isProductInOrder = await _productRepository.IsProductInAnyOrderAsync(request.ProductId);

            if (isProductInOrder)
            {
                return BadRequest(new { message = "Cannot delete this product as it is part of an existing order." });
            }


            var success = await _productRepository.UpdateIsDeletedAsync(request.ProductId, request.VendorId, request.IsDeleted);

            if (success)
            {
                return Ok(new { message = "Product deletion status updated successfully" });
            }
            else
            {
                return NotFound(new { message = "Product not found or you're not the owner" });
            }
        }

        // Endpoint to update product details
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProduct([FromForm] Product updatedProduct, string productId, string vendorId, List<IFormFile> newImages)
        {
            if (newImages != null && newImages.Count > 5)
            {
                return BadRequest("You cannot upload more than 5 images.");
            }

            // Prepare the image streams for upload
            var newImageStreams = new List<Stream>();
            foreach (var image in newImages)
            {
                var stream = image.OpenReadStream();
                newImageStreams.Add(stream);
            }

            // Perform the update
            var success = await _productRepository.UpdateProductAsync(productId, vendorId, updatedProduct, newImageStreams);

            if (success)
            {
                return Ok(new { message = "Product updated successfully" });
            }
            else
            {
                return NotFound(new { message = "Product not found or you are not the owner" });
            }
        }

    }

    public class UpdateIsDeletedRequest
    {
        public string ProductId { get; set; } = null!;
        public string VendorId { get; set; } = null!;
        public bool IsDeleted { get; set; }
    }
}
