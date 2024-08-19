using LibraryProject.Data;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;

namespace LibraryProject.Repositories.Implementation
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
