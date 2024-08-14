using LibraryProject.Data;
using LibraryProject.Models;
using LibraryProject.Repositories.Interface;

namespace LibraryProject.Repositories.Implementation
{
    public class CatetoryRepository : Repository<Category>, ICatetoryRepository
    {
        public CatetoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
