using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class Book
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kitap adı zorunludur")]
        [FirestoreProperty("title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yazar adı zorunludur")]
        [FirestoreProperty("author")]
        public string Author { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN zorunludur")]
        [FirestoreProperty("isbn")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yayın yılı zorunludur")]
        [FirestoreProperty("publicationYear")]
        public int PublicationYear { get; set; }

        [Required(ErrorMessage = "Sayfa sayısı zorunludur")]
        [FirestoreProperty("pageCount")]
        public int PageCount { get; set; }

        [FirestoreProperty("description")]
        public string Description { get; set; } = string.Empty;

        [FirestoreProperty("category")]
        public string Category { get; set; } = string.Empty;

        [FirestoreProperty("language")]
        public string Language { get; set; } = string.Empty;

        [FirestoreProperty("publisher")]
        public string Publisher { get; set; } = string.Empty;

        [FirestoreProperty("coverImageUrl")]
        public string CoverImageUrl { get; set; } = string.Empty;

        [FirestoreProperty("shelfNumber")]
        public string ShelfNumber { get; set; } = string.Empty;

        [FirestoreProperty("isAvailable")]
        public bool IsAvailable { get; set; } = true;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - not stored in Firestore
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}
