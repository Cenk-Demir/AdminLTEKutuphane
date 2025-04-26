using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace AdminLTEKutuphane.Controllers
{
    public class BookController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public BookController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // GET: Book
        public async Task<IActionResult> Index()
        {
            try
            {
                var books = await _firestoreService.GetBooksAsync();
                return View(books);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitaplar listelenirken bir hata oluştu: " + ex.Message;
                return View(new List<Book>());
            }
        }

        // GET: Book/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _firestoreService.AddBookAsync(book);
                    TempData["Success"] = "Kitap başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(book);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kitap eklenirken bir hata oluştu: " + ex.Message);
                return View(book);
            }
        }

        // GET: Book/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var book = await _firestoreService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap bilgileri alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Book book)
        {
            try
            {
                if (id != book.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _firestoreService.UpdateBookAsync(id, book);
                    TempData["Success"] = "Kitap başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(book);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kitap güncellenirken bir hata oluştu: " + ex.Message);
                return View(book);
            }
        }

        // GET: Book/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var book = await _firestoreService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap bilgileri alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _firestoreService.DeleteBookAsync(id);
                TempData["Success"] = "Kitap başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap silinirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Book/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var book = await _firestoreService.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                return View(book);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap detayları alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 