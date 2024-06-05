using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Dtos;
using variate.Mapping;

namespace variate.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext dbContext, ILogger<CategoryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .Select(c => c.ToDto())
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .Select(p => p.ToDto())
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => c.ToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CreateCategoryAsync(CategoryDto categoryDto)
        {
            try
            {
                await _dbContext.Categories.AddAsync(categoryDto.ToEntity());
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category.");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(CategoryDto categoryDto)
        {
            try
            {
                _dbContext.Categories.Update(categoryDto.ToEntity());
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID {Id}.", categoryDto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _dbContext.Categories.FindAsync(id);
                if (category == null) return false;

                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID {Id}.", id);
                return false;
            }
        }
    }
}
