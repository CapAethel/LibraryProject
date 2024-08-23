using LibraryProject.Data;
using LibraryProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace LibraryProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users/Edit/5
        // GET: Account/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email
            };

            return View(viewModel);
        }

        // POST: Account/Edit/5
        // POST: Account/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var existingUser = await _context.Users.FindAsync(viewModel.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Check if the new name or email already exists (excluding the current user)
            var userWithSameEmail = _context.Users
                .Where(u => u.Email == viewModel.Email && u.UserId != viewModel.UserId)
                .FirstOrDefault();

            var userWithSameName = _context.Users
                .Where(u => u.Name == viewModel.Name && u.UserId != viewModel.UserId)
                .FirstOrDefault();

            if (userWithSameEmail != null || userWithSameName != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this name or email already exists.");
                return View(viewModel);
            }

            // Check if old password matches
            if (!string.IsNullOrEmpty(viewModel.OldPassword) && !string.IsNullOrEmpty(viewModel.NewPassword))
            {
                if (existingUser.Password != HashPassword(viewModel.OldPassword)) // Replace with proper password verification
                {
                    ModelState.AddModelError(string.Empty, "Old password is incorrect.");
                    return View(viewModel);
                }

                // Validate new password strength
                if (!IsValidPassword(viewModel.NewPassword))
                {
                    ModelState.AddModelError(string.Empty, "New password does not meet the requirements.");
                    return View(viewModel);
                }

                // Update password
                existingUser.Password = HashPassword(viewModel.NewPassword); // Replace with proper password hashing
            }

            existingUser.Name = viewModel.Name;
            existingUser.Email = viewModel.Email;

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(existingUser.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register([Bind("Name,Email,Password")] User user)
        {
            // Check if the name or email already exists
            var existingUser = _context.Users
                .FirstOrDefault(u => u.Email == user.Email || u.Name == user.Name);

            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this name or email already exists.");
                return View(user);
            }

            // Set RoleId to 'user' by default before validation
            user.RoleId = 1; // Assuming 1 is the 'user' role ID

            // Validate password strength
            if (!IsValidPassword(user.Password))
            {
                ModelState.AddModelError(string.Empty, "Password does not meet the requirements.");
                return View(user);
            }

            // Hash the password before saving
            user.Password = HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        // GET: User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string identifier, string password)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = HashPassword(password);

                // Search for the user by either name or email
                var user = _context.Users
                             .FirstOrDefault(u => (u.Email == identifier || u.Name == identifier) && u.Password == hashedPassword);

                if (user != null)
                {
                    // Create the claims that will be stored in the cookie
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString()), // Add user ID to claims
                new Claim(ClaimTypes.Role, user.RoleId.ToString()) // Or use a role name instead
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        // Optional: Set the expiration of the authentication session
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Books"); // Redirect to a secure area
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View();
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute the hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private bool IsValidPassword(string password)
        {
            // Define password strength criteria
            var hasMinimumLength = password.Length >= 8;
            var hasUpperCase = password.Any(c => char.IsUpper(c));
            var hasLowerCase = password.Any(c => char.IsLower(c));
            var hasDigit = password.Any(c => char.IsDigit(c));
            var hasSpecialChar = password.Any(c => "!@#$%^&*()_+[]{}|;:,.<>?/".Contains(c));

            return hasMinimumLength && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }

    }
}
