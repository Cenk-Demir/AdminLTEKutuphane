using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AdminLTEKutuphane.Controllers
{
    public class UserController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public UserController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _firestoreService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcılar listelenirken bir hata oluştu: " + ex.Message;
                return View(new List<User>());
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
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _firestoreService.AddUserAsync(user);
                    TempData["Success"] = "Kullanıcı başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                }

                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kullanıcı eklenirken bir hata oluştu: " + ex.Message);
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

                var user = await _firestoreService.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcı detayları alınırken bir hata oluştu: " + ex.Message;
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

                var user = await _firestoreService.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcı bilgileri alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, User user)
        {
            try
            {
                if (id != user.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _firestoreService.UpdateUserAsync(user);
                    TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kullanıcı güncellenirken bir hata oluştu: " + ex.Message);
                return View(user);
            }
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

                var user = await _firestoreService.GetUserAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcı bilgileri alınırken bir hata oluştu: " + ex.Message;
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

                await _firestoreService.DeleteUserAsync(id);
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kullanıcı silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
