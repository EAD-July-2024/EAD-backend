using api.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace api.Services
{
    public class CategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _categories = database.GetCollection<Category>("Categories");
        }

        // Retrieve all categories
        public async Task<List<Category>> GetAsync()
        {
            return await _categories.Find(c => !c.isDeleted).ToListAsync();
        }

        // Get a category by custom ID
        public async Task<Category> GetByCustomIdAsync(string categoryId)
        {
            return await _categories.Find(c => c.CategoryId == categoryId && !c.isDeleted).FirstOrDefaultAsync();
        }

        // Create a new category
        public async Task CreateAsync(Category category)
        {
            await _categories.InsertOneAsync(category);
        }

        // Update an existing category
        public async Task UpdateAsync(string categoryId, string? name, string? description, string? status, bool isDeleted)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId);

            // Define the fields to be updated
            var update = Builders<Category>.Update
                .Set(c => c.Name, name)
                .Set(c => c.Description, description)
                .Set(c => c.Status, status)
                .Set(c => c.isDeleted, isDeleted);

            // Update the category document, but only with the specified fields
            await _categories.UpdateOneAsync(filter, update);
        }

        // public async Task UpdateAsync(string categoryId, string? name, string? description, string? status, bool? isDeleted)
        // {
        //     var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId);

        //     var updateDefinitionBuilder = Builders<Category>.Update;
        //     var update = new List<UpdateDefinition<Category>>();

        //     // Add updates only for the fields that are not null or empty
        //     if (!string.IsNullOrEmpty(name))
        //     {
        //         update.Add(updateDefinitionBuilder.Set(c => c.Name, name));
        //     }

        //     if (!string.IsNullOrEmpty(description))
        //     {
        //         update.Add(updateDefinitionBuilder.Set(c => c.Description, description));
        //     }

        //     if (!string.IsNullOrEmpty(status))
        //     {
        //         update.Add(updateDefinitionBuilder.Set(c => c.Status, status));
        //     }

        //     if (isDeleted.HasValue)
        //     {
        //         update.Add(updateDefinitionBuilder.Set(c => c.isDeleted, isDeleted.Value));
        //     }

        //     // Combine all the updates together
        //     if (update.Count > 0)
        //     {
        //         var combinedUpdate = updateDefinitionBuilder.Combine(update);
        //         await _categories.UpdateOneAsync(filter, combinedUpdate);
        //     }
        // }



        // Soft delete a category (set isDeleted to true)
        public async Task DeactivateCategoryAsync(Category category)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.Id, category.Id);
            var update = Builders<Category>.Update.Set(c => c.isDeleted, true);
            await _categories.UpdateOneAsync(filter, update);
        }

        // Check if a category with the given custom ID already exists
        public async Task<bool> GetExistingIdsAsync(string categoryId)
        {
            return await _categories.Find(c => c.CategoryId == categoryId).AnyAsync();
        }
    }
}
