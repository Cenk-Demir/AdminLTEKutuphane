using Microsoft.AspNetCore.Mvc;
using AdminLTEKutuphane.Services;
using AdminLTEKutuphane.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AdminLTEKutuphane.Controllers
{
    public class FirestoreController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public FirestoreController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // Kitapları listele
        public async Task<IActionResult> BookList()
        {
            var books = await _firestoreService.GetBooksAsync();
            return View(books);
        }

        // Kitap ekleme sayfası
        [HttpGet]
        public IActionResult AddBook()
        {
            return View();
        }

        // Kitap ekleme işlemi
        [HttpPost]
        public async Task<IActionResult> AddBook(Book book)
        {
            if (ModelState.IsValid)
            {
                await _firestoreService.AddBookAsync(book);
                return RedirectToAction("BookList");
            }
            return View(book);
        }

        // Kullanıcıları listele
        public async Task<IActionResult> UserList()
        {
            var users = await _firestoreService.GetUsersAsync();
            return View(users);
        }

        // Ödünç kitapları listele
        public async Task<IActionResult> BorrowedBooks()
        {
            var borrowedBooks = await _firestoreService.GetBorrowedBooksAsync();
            return View(borrowedBooks);
        }
    }
}
