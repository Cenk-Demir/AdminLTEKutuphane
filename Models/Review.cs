using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class Review
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kitap ID zorunludur")]
        [FirestoreProperty("bookid")]
        public string BookId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        [FirestoreProperty("userid")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Değerlendirme puanı zorunludur")]
        [Range(1, 5, ErrorMessage = "Değerlendirme puanı 1-5 arasında olmalıdır")]
        [FirestoreProperty("rating")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Değerlendirme metni zorunludur")]
        [StringLength(500, ErrorMessage = "Değerlendirme metni en fazla 500 karakter olabilir")]
        [FirestoreProperty("reviewtext")]
        public string ReviewText { get; set; } = string.Empty;

        [FirestoreProperty("reviewdate")]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Navigation properties - not stored in Firestore
        public Book? Book { get; set; }
        public User? User { get; set; }
    }
}
