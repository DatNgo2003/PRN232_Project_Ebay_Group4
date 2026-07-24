namespace Backend.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(string name);
        Task<CategoryDto?> UpdateCategoryAsync(int id, string name);
        Task<bool> DeleteCategoryAsync(int id);
        Task<int> GetProductCountByCategoryAsync(int categoryId);
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ProductCount { get; set; }
    }
}
