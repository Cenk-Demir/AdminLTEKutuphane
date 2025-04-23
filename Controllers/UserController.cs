using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AdminLTEKutuphane.Controllers
{
    public class UserController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public UserController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // Kullanıcıları listele
        public async Task<IActionResult> Index()
        {
            var users = await _firestoreService.GetUsersAsync();
            return View(users);
        }

        // Kullanıcı ekle
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(User user)
        {
            if (ModelState.IsValid)
            {
                await _firestoreService.AddUserAsync(user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // Kullanıcı detayları
        public async Task<IActionResult> Details(string id)
        {
            var user = await _firestoreService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Kullanıcı düzenle
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _firestoreService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _firestoreService.UpdateUserAsync(id, user);
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // Kullanıcı sil
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _firestoreService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _firestoreService.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }
    }
}
