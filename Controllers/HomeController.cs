using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AdminLTEKutuphane.Services;  // FirestoreService'i içeren namespace

namespace AdminLTEKutuphane.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirestoreService _firestoreService;

        public HomeController(FirestoreService firestoreService, IConfiguration configuration)
        {
            _firestoreService = firestoreService;
            // IConfiguration'ı FirestoreService constructor'ına geçiriyoruz
            _firestoreService = new FirestoreService(configuration);
        }

        public IActionResult Index()
        {
            // Burada FirestoreService kullanabilirsiniz
            return View();
        }
    }
}
