using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;  // Required for BasicAWSCredentials
using api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Services
{
    public class ProductRepository
    {
        private readonly IMongoCollection<Product> _products;
        private readonly IMongoCollection<OrderItem> _orderItems;
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "eadbucket";

        public ProductRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>(mongoDBSettings.Value.CollectionName);
            _orderItems = database.GetCollection<OrderItem>("OrderItems");


            // // // Retrieve AWS credentials from environment variables
            var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");




            if (string.IsNullOrEmpty(awsAccessKeyId) || string.IsNullOrEmpty(awsSecretAccessKey))
            {
                throw new InvalidOperationException("AWS credentials not found in environment variables.");
            }

            var credentials = new BasicAWSCredentials(awsAccessKeyId, awsSecretAccessKey);

            _s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.APNortheast1); // Replace with your desired region
        }

        public async Task<string> UploadImageAsync(string fileName, Stream fileStream)
        {
            var uploadRequest = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = $"ProductImages/{fileName}",  // Include folder path in key
                InputStream = fileStream,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };
            await _s3Client.PutObjectAsync(uploadRequest);
            return $"https://{BucketName}.s3.amazonaws.com/ProductImages/{fileName}";  // URL with folder path
        }

        public async Task CreateAsync(Product product, List<Stream> imageStreams)
        {
            for (int i = 0; i < imageStreams.Count; i++)
            {
                var fileName = $"{product.ProductId}_image_{i}.jpg";
                var imageUrl = await UploadImageAsync(fileName, imageStreams[i]);
                product.ImageUrls.Add(imageUrl);
            }

            await _products.InsertOneAsync(product);
        }

        public async Task<bool> getExistingIds(String pId)
        {
            return await _products.Find(p => p.ProductId == pId).AnyAsync();
        }

        public async Task<List<Product>> GetAsync()
        {
            var products = await _products.Find(new BsonDocument()).ToListAsync();

            //foreach (var product in products)
            //{
            //    for (int i = 0; i < product.ImageUrls.Count; i++)
            //    {
            //        product.ImageUrls[i] = GeneratePresignedURL(product.ImageUrls[i]);
            //    }
            //}
            return products;
        }


        private string GeneratePresignedURL(string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = BucketName,
                Key = objectKey,  // Pass full key with folder path when generating presigned URL
                Expires = DateTime.Now.AddMinutes(10)
            };
            return _s3Client.GetPreSignedURL(request);
        }


        //Get product details by product custom id
        public async Task<Product> GetByCustomIdAsync(string customId)
        {
            return await _products.Find(p => p.ProductId == customId).FirstOrDefaultAsync();
        }

        // Deactivate product (soft delete)
        public async Task DeactivateProductAsync(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            var update = Builders<Product>.Update.Set(p => p.IsDeleted, false);
            await _products.UpdateOneAsync(filter, update);
        }

        // Update the product quantity
        public async Task UpdateQuantityAsync(string productId, int newQuantity)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.ProductId, productId);
            var update = Builders<Product>.Update.Set(p => p.Quantity, newQuantity);
            await _products.UpdateOneAsync(filter, update);
        }

        // Get products that are low in stock
        public async Task<List<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _products.Find(p => p.Quantity <= threshold).ToListAsync();
        }



        // Update product details
        public async Task<bool> UpdateProductAsync(string productId, Product updatedProduct, List<Stream> newImageStreams = null)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.ProductId, productId);
        
            var product = await _products.Find(filter).FirstOrDefaultAsync();
            if (product == null)
            {
                return false; // Product not found
            }
        
            // Update fields
            var updateDef = Builders<Product>.Update
                .Set(p => p.Name, updatedProduct.Name)
                .Set(p => p.Description, updatedProduct.Description)
                .Set(p => p.Price, updatedProduct.Price)
                .Set(p => p.CategoryId, updatedProduct.CategoryId);
        
            // Handle image update logic
            if (newImageStreams != null && newImageStreams.Count > 0)
            {
                if (product.ImageUrls.Count + newImageStreams.Count > 5)
                {
                    throw new InvalidOperationException("You cannot have more than 5 images for a product.");
                }
        
                // Remove excess images if necessary to keep the total count at 5
                int removeCount = (product.ImageUrls.Count + newImageStreams.Count) - 5;
                for (int i = 0; i < removeCount; i++)
                {
                    product.ImageUrls.RemoveAt(0);  // Remove from the front or oldest image
                }
        
                // Upload new images and add to ImageUrls
                foreach (var stream in newImageStreams)
                {
                    var fileName = $"{product.ProductId}_image_{Guid.NewGuid()}.jpg";
                    var imageUrl = await UploadImageAsync(fileName, stream);
                    product.ImageUrls.Add(imageUrl);
                }
            }
        
            updateDef = updateDef.Set(p => p.ImageUrls, product.ImageUrls);
            var result = await _products.UpdateOneAsync(filter, updateDef);
            return result.ModifiedCount > 0;
        }






        // Method to check if product is part of any order

        public async Task<bool> IsProductInAnyOrderAsync(string productId)
        {
            var filter = Builders<OrderItem>.Filter.Eq(oi => oi.ProductId, productId);
            var orderItem = await _orderItems.Find(filter).FirstOrDefaultAsync();
            return orderItem != null; // Returns true if product exists in any order
        }

        // Update the IsDeleted 
        public async Task<bool> UpdateIsDeletedAsync(string productId, string vendorId, bool isDeleted)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.ProductId, productId) &
                         Builders<Product>.Filter.Eq(p => p.VendorId, vendorId);
            var update = Builders<Product>.Update.Set(p => p.IsDeleted, isDeleted);

            var result = await _products.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }


    }
}
