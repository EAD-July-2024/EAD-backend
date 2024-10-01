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
        private readonly IAmazonS3 _s3Client;
        private const string BucketName = "eadbucket";

        public ProductRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _products = database.GetCollection<Product>(mongoDBSettings.Value.CollectionName);

            // Use the AWS Access Key and Secret Key directly in code
            var awsAccessKeyId = "";  // Replace with your actual AWS_ACCESS_KEY_ID
            var awsSecretAccessKey = "";  // Replace with your actual AWS_SECRET_ACCESS_KEY

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
                var fileName = $"{product.Id}_image_{i}.jpg";
                var imageUrl = await UploadImageAsync(fileName, imageStreams[i]);
                product.ImageUrls.Add(imageUrl);
            }

            await _products.InsertOneAsync(product);
        }

        public async Task<List<Product>> GetAsync()
        {
            var products = await _products.Find(new BsonDocument()).ToListAsync();
            foreach (var product in products)
            {
                for (int i = 0; i < product.ImageUrls.Count; i++)
                {
                    product.ImageUrls[i] = GeneratePresignedURL(product.ImageUrls[i]);
                }
            }
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
    }
}
