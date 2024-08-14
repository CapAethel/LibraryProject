using LibraryProject.Models;
using LibraryProject.Repositories.Interface;
using LibraryProject.Services.Interface;

namespace LibraryProject.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICatetoryRepository _categoryRepository;

        public CategoryService(ICatetoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }
    }
}
