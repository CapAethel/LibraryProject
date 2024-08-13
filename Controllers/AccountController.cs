using Microsoft.AspNetCore.Mvc;

namespace LibraryProject.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
