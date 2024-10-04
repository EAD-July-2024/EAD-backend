using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Controller]
    [Route("api/product")]
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        public ProductController(ProductRepository productRepository, OrderRepository orderRepository)
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        [HttpGet]
        //[Authorize(Policy = "RequireAdminRole")]
        public async Task<List<Product>> Get()
        {
            return await _productRepository.GetAsync();
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
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetByCustomId(string productId)
        {
            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            return Ok(product);
        }


        // Endpoint to delete a product
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeactivateProduct(string productId)
        {

            var product = await _productRepository.GetByCustomIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with Custom ID {productId} not found");
            }

            bool isProductInOrders = await _orderRepository.CheckProductInOrdersAsync(productId);
            if (isProductInOrders)
            {
                return BadRequest("Product cannot be deleted because it is part of existing orders.");
            }

            product.IsDeleted = false;
            await _productRepository.DeactivateProductAsync(product);

            return Ok($"Product with Custom ID {productId} has been deactivated.");
        }

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






    }
}
