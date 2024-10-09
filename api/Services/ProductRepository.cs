/*
 * File: ProductRepository.cs
 * Author: [​Gunasekara S.N.W. ]

 * Description: 
 *     This file contains the ProductRepository class, which handles operations related to 
 *     product management in the E-commerce system. It includes methods for creating, retrieving, 
 *     updating, and deleting products, as well as managing product images stored in Amazon S3.
 * 
 * Dependencies:
 *     - MongoDB.Driver: Used to interact with the MongoDB database for product and order item storage.
 *     - Amazon.S3: Used for uploading and managing product images in an Amazon S3 bucket.
 *     - Product: Represents the product model, including details such as ProductId, Name, 
 *       Description, Price, Quantity, and ImageUrls.
 *     - OrderItem: Represents order items associated with products.
 * 
 * Methods:
 *     - UploadImageAsync: Uploads an image to the specified S3 bucket and returns the image URL.
 *     - CreateAsync: Creates a new product and uploads associated images to S3.
 *     - getExistingIds: Checks if a product with the specified ID already exists.
 *     - GetAsync: Retrieves all products from the database.
 *     - GeneratePresignedURL: Generates a pre-signed URL for accessing a product image in S3.
 *     - GetByCustomIdAsync: Retrieves a product by its custom ID.
 *     - GetByVendorIdAsync: Retrieves all products associated with a specified vendor.
 *     - DeactivateProductAsync: Soft deletes a product by updating its IsDeleted status.
 *     - UpdateQuantityAsync: Updates the quantity of a specified product.
 *     - GetLowStockProductsAsync: Retrieves products that are low in stock based on a specified threshold.
 *     - UpdateProductAsync: Updates the details of a product and handles image updates.
 *     - IsProductInAnyOrderAsync: Checks if a product is part of any order.
 *     - UpdateIsDeletedAsync: Updates the IsDeleted status of a product based on product ID and vendor ID.
 * 

 */

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

            // // Retrieve AWS credentials from environment variables
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

        // Create a new product
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

        // Get existing product ids
        public async Task<bool> getExistingIds(String pId)
        {
            return await _products.Find(p => p.ProductId == pId).AnyAsync();
        }

        // Get all products
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

        // Get product details by vendor id
        public async Task<List<Product>> GetByVendorIdAsync(string vendorId)
        {
            return await _products.Find(p => p.VendorId == vendorId).ToListAsync();
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

            var updateDef = Builders<Product>.Update.Combine();

            // Update fields only if provided in the updatedProduct object
            if (!string.IsNullOrEmpty(updatedProduct.Name))
            {
                updateDef = updateDef.Set(p => p.Name, updatedProduct.Name);
            }

            if (!string.IsNullOrEmpty(updatedProduct.Description))
            {
                updateDef = updateDef.Set(p => p.Description, updatedProduct.Description);
            }

            if (updatedProduct.Price != 0)
            {
                updateDef = updateDef.Set(p => p.Price, updatedProduct.Price);
            }

            if (!string.IsNullOrEmpty(updatedProduct.CategoryId))
            {
                updateDef = updateDef.Set(p => p.CategoryId, updatedProduct.CategoryId);
            }

            // Handle image update logic if new images are provided
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

                // Add updated image URLs to the update definition
                updateDef = updateDef.Set(p => p.ImageUrls, product.ImageUrls);
            }

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
