/*
 * File: CategoryRepository.cs
 * Author: [Piyumantha W.U.]

 * Description:
 *     This file contains the CategoryRepository class, which provides methods for 
 *     managing categories in a MongoDB database. The repository allows for CRUD 
 *     operations, including retrieving, creating, updating, and soft-deleting 
 *     categories.
 * 
 * Dependencies:
 *     - MongoDB.Driver: For interacting with the MongoDB database.
 *     - Microsoft.Extensions.Options: For accessing MongoDB settings from the 
 *       application configuration.
 * 
 * Methods:
 *     - CategoryRepository: Constructor that initializes the MongoDB collection 
 *       for categories using the provided MongoDB settings.
 * 
 *     - GetAsync:
 *         Retrieves all categories that are not marked as deleted (isDeleted = false).
 * 
 *     - GetByCustomIdAsync:
 *         Retrieves a category by its custom ID (CategoryId). Returns null if 
 *         the category does not exist or is marked as deleted.
 * 
 *     - CreateAsync:
 *         Creates a new category by inserting it into the database.
 * 
 *     - UpdateAsync:
 *         Updates an existing category based on its custom ID. Allows updating 
 *         the name, description, status, and isDeleted fields. 
 * 
 *     - DeactivateCategoryAsync:
 *         Soft deletes a category by setting the isDeleted field to true. 
 * 
 *     - GetExistingIdsAsync:
 *         Checks if a category with the specified custom ID already exists in 
 *         the database. Returns true if it exists, false otherwise.
 * 

 */

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
        public async Task UpdateAsync(string categoryId, string? name = null, string? description = null, string? status = null, bool? isDeleted = null)
        {
            var filter = Builders<Category>.Filter.Eq(c => c.CategoryId, categoryId);

            // Initialize an empty UpdateDefinition
            var update = Builders<Category>.Update.Combine();
        
            // Apply updates only for non-null fields
            if (!string.IsNullOrEmpty(name))
            {
                update = update.Set(c => c.Name, name);
            }
        
            if (description != null)
            {
                update = update.Set(c => c.Description, description);
            }
        
            if (!string.IsNullOrEmpty(status))
            {
                update = update.Set(c => c.Status, status);
            }
        
            if (isDeleted.HasValue)
            {
                update = update.Set(c => c.isDeleted, isDeleted.Value);
            }
        
            // Perform the update if there are any changes
            if (update != Builders<Category>.Update.Combine())
            {
                await _categories.UpdateOneAsync(filter, update);
            }
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
