using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminLTEKutuphane.Controllers
{
    public class BorrowedBookController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public BorrowedBookController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // GET: BorrowedBook
        public async Task<IActionResult> Index()
        {
            try
            {
                var borrowedBooks = await _firestoreService.GetBorrowedBooksAsync();
                return View(borrowedBooks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödünç alınan kitaplar listelenirken bir hata oluştu: " + ex.Message;
                return View(new List<BorrowedBook>());
            }
        }

        // GET: BorrowedBook/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var books = await _firestoreService.GetBooksAsync();
                var users = await _firestoreService.GetAllUsersAsync();
                ViewBag.Books = books.Where(b => b.IsAvailable).ToList();
                ViewBag.Users = users;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap ve kullanıcı bilgileri alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowedBook/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowedBook borrowedBook)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var book = await _firestoreService.GetBookByIdAsync(borrowedBook.BookId);
                    if (book == null)
                    {
                        ModelState.AddModelError("", "Kitap bulunamadı.");
                        return View(borrowedBook);
                    }

                    if (!book.IsAvailable)
                    {
                        ModelState.AddModelError("", "Kitap şu anda müsait değil.");
                        return View(borrowedBook);
                    }

                    book.IsAvailable = false;
                    await _firestoreService.UpdateBookAsync(book.Id, book);
                    await _firestoreService.AddBorrowedBookAsync(borrowedBook);
                    TempData["Success"] = "Kitap başarıyla ödünç verildi.";
                    return RedirectToAction(nameof(Index));
                }
                return View(borrowedBook);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Kitap ödünç verilirken bir hata oluştu: " + ex.Message);
                return View(borrowedBook);
            }
        }

        // GET: BorrowedBook/Return/5
        public async Task<IActionResult> Return(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var borrowedBook = await _firestoreService.GetBorrowedBookByIdAsync(id);
                if (borrowedBook == null)
                {
                    return NotFound();
                }

                if (borrowedBook.IsReturned)
                {
                    TempData["Error"] = "Bu kitap zaten iade edilmiş.";
                    return RedirectToAction(nameof(Index));
                }

                return View(borrowedBook);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödünç kitap bilgileri alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BorrowedBook/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(string id)
        {
            try
            {
                var borrowedBook = await _firestoreService.GetBorrowedBookByIdAsync(id);
                if (borrowedBook == null)
                {
                    return NotFound();
                }

                borrowedBook.IsReturned = true;
                borrowedBook.ReturnDate = DateTime.UtcNow;
                await _firestoreService.UpdateBorrowedBookAsync(id, borrowedBook);

                var book = await _firestoreService.GetBookByIdAsync(borrowedBook.BookId);
                if (book != null)
                {
                    book.IsAvailable = true;
                    await _firestoreService.UpdateBookAsync(book.Id, book);
                }

                TempData["Success"] = "Kitap başarıyla iade edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap iade edilirken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: BorrowedBook/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var borrowedBook = await _firestoreService.GetBorrowedBookByIdAsync(id);
                if (borrowedBook == null)
                {
                    return NotFound();
                }
                return View(borrowedBook);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ödünç kitap detayları alınırken bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 