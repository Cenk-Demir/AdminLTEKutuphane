using AdminLTEKutuphane.Models;
using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Firestore;

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
                // Sadece müsait olan kitapları getir
                var allBooks = await _firestoreService.GetBooksAsync();
                var availableBooks = allBooks.Where(b => b.IsAvailable).ToList();
                ViewBag.Books = availableBooks;

                // Tüm aktif kullanıcıları getir
                var users = await _firestoreService.GetAllUsersAsync();
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
                    // Kitabın müsait olup olmadığını kontrol et
                    var book = await _firestoreService.GetBookByIdAsync(borrowedBook.BookId);
                    if (book == null)
                    {
                        ModelState.AddModelError("", "Kitap bulunamadı.");
                        goto PrepareViewBags;
                    }

                    if (!book.IsAvailable)
                    {
                        ModelState.AddModelError("", "Bu kitap şu anda müsait değil.");
                        goto PrepareViewBags;
                    }

                    // Kullanıcının var olup olmadığını kontrol et
                    var user = await _firestoreService.GetUserAsync(borrowedBook.UserId);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                        goto PrepareViewBags;
                    }

                    // Tarihleri UTC'ye çevir
                    borrowedBook.CreatedAt = DateTime.UtcNow;
                    borrowedBook.UpdatedAt = DateTime.UtcNow;
                    borrowedBook.BorrowDate = DateTime.SpecifyKind(borrowedBook.BorrowDate, DateTimeKind.Utc);
                    borrowedBook.DueDate = DateTime.SpecifyKind(borrowedBook.DueDate, DateTimeKind.Utc);
                    borrowedBook.IsReturned = false;
                    
                    await _firestoreService.AddBorrowedBookAsync(borrowedBook);

                    // Kitabın durumunu güncelle
                    book.IsAvailable = false;
                    await _firestoreService.UpdateBookAsync(book.Id, book);

                    TempData["Success"] = "Kitap başarıyla ödünç alındı.";
                    return RedirectToAction(nameof(Index));
                }

            PrepareViewBags:
                // ViewBag'leri hazırla
                var allBooks = await _firestoreService.GetBooksAsync();
                var availableBooks = allBooks.Where(b => b.IsAvailable).ToList();
                ViewBag.Books = availableBooks;

                var users = await _firestoreService.GetAllUsersAsync();
                ViewBag.Users = users;

                return View(borrowedBook);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kitap ödünç alma işlemi sırasında bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
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