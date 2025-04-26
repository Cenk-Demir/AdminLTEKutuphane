using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AdminLTEKutuphane.Services;  // FirestoreService'i içeren namespace
using AdminLTEKutuphane.Models;

namespace AdminLTEKutuphane.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public HomeController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardViewModel = new DashboardViewModel
                {
                    BookCount = await _firestoreService.GetBookCount(),
                    UserCount = await _firestoreService.GetUserCount(),
                    BorrowedBookCount = await _firestoreService.GetBorrowedBookCount(),
                    ActiveTransactionCount = await _firestoreService.GetActiveTransactionCount()
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yapabilirsiniz
                return View(new DashboardViewModel()); // Boş model döndür
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
