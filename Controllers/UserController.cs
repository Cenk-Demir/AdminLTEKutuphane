using Microsoft.AspNetCore.Mvc;
using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly FirestoreService _firestoreService;
        private readonly ILogger<UserController> _logger;

        public UserController(IConfiguration configuration, FirestoreService firestoreService, ILogger<UserController> logger)
        {
            _configuration = configuration;
            _firestoreService = firestoreService ?? throw new ArgumentNullException(nameof(firestoreService));
            _logger = logger;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _firestoreService.GetAllAsync<Models.User>("users");
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcılar listelenirken hata oluştu.");
                TempData["Error"] = "Kullanıcılar listelenirken bir hata oluştu.";
                return View(new List<Models.User>());
            }
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _firestoreService.GetByEmailAsync<Models.User>("users", user.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                        return View(user);
                    }

                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    user.CreatedAt = DateTime.UtcNow;
                    user.Role = "User";
                    user.MembershipStatus = "Active";
                    user.MembershipStartDate = DateTime.UtcNow;

                    await _firestoreService.AddAsync("users", user);

                    TempData["Success"] = "Kullanıcı başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı eklenirken hata oluştu.");
                ModelState.AddModelError("", "Kullanıcı eklenirken bir hata oluştu.");
                return View(user);
            }
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var user = await _firestoreService.GetByIdAsync<Models.User>("users", id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı detayları alınırken hata oluştu.");
                TempData["Error"] = "Kullanıcı detayları alınırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var user = await _firestoreService.GetByIdAsync<Models.User>("users", id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken hata oluştu.");
                TempData["Error"] = "Kullanıcı bilgileri alınırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Models.User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var existingUser = await _firestoreService.GetByIdAsync<Models.User>("users", id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Job = user.Job;

                    if (!string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                    }

                    existingUser.UpdatedAt = DateTime.UtcNow;
                    await _firestoreService.UpdateAsync("users", id, existingUser);
                    
                    TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı güncellenirken hata oluştu.");
                ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu.");
            }

            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var user = await _firestoreService.GetByIdAsync<Models.User>("users", id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken hata oluştu.");
                TempData["Error"] = "Kullanıcı bilgileri alınırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                await _firestoreService.DeleteAsync("users", id);

                TempData["Success"] = "Kullanıcı başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silinirken hata oluştu.");
                TempData["Error"] = "Kullanıcı silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _firestoreService.GetUserByEmailAsync(model.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
                    return View(model);
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _firestoreService.UpdateUserAsync(user.Id, user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login işlemi sırasında hata oluştu");
                ModelState.AddModelError("", "Giriş yapılırken bir hata oluştu.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _firestoreService.UserExistsByEmailAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Phone = model.Phone,
                    Address = model.Address,
                    Job = model.Job,
                    Role = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    MembershipStartDate = DateTime.UtcNow,
                    MembershipStatus = "Active"
                };

                await _firestoreService.AddUserAsync(user);

                TempData["SuccessMessage"] = "Kayıt başarıyla tamamlandı. Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register işlemi sırasında hata oluştu");
                ModelState.AddModelError("", "Kayıt olurken bir hata oluştu.");
                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var user = await _firestoreService.GetUserAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile sayfası yüklenirken hata oluştu");
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var existingUser = await _firestoreService.GetUserAsync(userId);
                if (existingUser == null)
                {
                    return RedirectToAction("Login");
                }

                existingUser.FirstName = model.FirstName;
                existingUser.LastName = model.LastName;
                existingUser.Phone = model.Phone;
                existingUser.Address = model.Address;
                existingUser.Job = model.Job;
                existingUser.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                await _firestoreService.UpdateUserAsync(userId, existingUser);

                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncellenirken hata oluştu");
                ModelState.AddModelError("", "Profil güncellenirken bir hata oluştu.");
                return View("Profile", model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var user = await _firestoreService.GetUserAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifre yanlış.");
                    return View(model);
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _firestoreService.UpdateUserAsync(userId, user);

                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirilirken hata oluştu");
                ModelState.AddModelError("", "Şifre değiştirilirken bir hata oluştu.");
                return View(model);
            }
        }

        [Authorize]
        public async Task<IActionResult> BorrowedBooks()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var borrowedBooks = await _firestoreService.GetActiveBorrowedBooksByUserAsync(userId);
                return View(borrowedBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ödünç alınan kitaplar alınırken hata oluştu.");
                TempData["ErrorMessage"] = "Ödünç alınan kitaplar alınırken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public async Task<IActionResult> History()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var borrowedBooks = await _firestoreService.GetBorrowedBooksAsync();
                var userHistory = borrowedBooks.Where(b => b.UserId == userId).ToList();
                return View(userHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçmiş kayıtları alınırken hata oluştu.");
                TempData["ErrorMessage"] = "Geçmiş kayıtları alınırken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
