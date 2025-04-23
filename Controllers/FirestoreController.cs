using AdminLTEKutuphane.Services;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using System.Threading.Tasks;

namespace AdminLTEKutuphane.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirestoreController : ControllerBase
    {
        private readonly FirestoreService _firestoreService;

        // FirestoreService'ı DI ile alıyoruz
        public FirestoreController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // Firestore'a veri eklemek için bir POST metodu
        [HttpPost("add")]
        public async Task<IActionResult> AddDocument([FromBody] Dictionary<string, object> data)
        {
            // ID'nin Firestore'da benzersiz olması için dinamik şekilde id oluşturabiliriz, burada "user1" örnek olarak kullanıldı.
            var documentId = "user1";
            
            // Firestore'a veri ekliyoruz
            await _firestoreService.AddDocumentAsync("users", documentId, data);
            
            return Ok("Document added successfully!");
        }

        // Firestore'dan veri almak için bir GET metodu
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetDocument(string id)
        {
            // Firestore'dan belgeyi çekiyoruz
            var doc = await _firestoreService.GetDocumentAsync("users", id);

            // Belge mevcutsa, veriyi döndürüyoruz
            if (doc.Exists)
            {
                return Ok(doc.ToDictionary());
            }

            // Belge bulunamazsa hata döndürüyoruz
            return NotFound("Document not found.");
        }
    }
}
// Compare this snippet from Services/FirestoreService.cs:
// using FirebaseAdmin;
// using Google.Apis.Auth.OAuth2;
// using Google.Cloud.Firestore;