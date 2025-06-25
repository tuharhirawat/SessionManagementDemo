using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using SessionManagementDemo.Models;
using Microsoft.AspNetCore.Authorization;

namespace SessionManagementDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: /account/Register
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }



        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (ModelState.IsValid)
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Username or Email already exists.");
                    return View(model);
                }

                //Hash password
                model.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.PasswordHash));

                _context.Users.Add(model); ;
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }


        // GET: /account/login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }


        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var hashedPassword = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(model.Password));

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // ✅ Step 3: Set custom expiration properties
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // persists across browser sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20) // custom expiration (e.g., 20 mins)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return RedirectToAction("Index", "Home");
        }




        // GET: /Account/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}


