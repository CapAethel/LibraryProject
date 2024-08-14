using LibraryProject.Models;

namespace LibraryProject.Services.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
    }
}
