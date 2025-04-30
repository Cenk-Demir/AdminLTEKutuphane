using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class BorrowedBook
    {
        [FirestoreProperty("borrowedId")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kitap ID zorunludur")]
        [FirestoreProperty("bookId")]
        public string BookId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        [FirestoreProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ödünç alma tarihi zorunludur")]
        [FirestoreProperty("borrowDate")]
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "İade tarihi zorunludur")]
        [FirestoreProperty("dueDate")]
        public DateTime DueDate { get; set; }

        [FirestoreProperty("returnDate")]
        public DateTime? ReturnDate { get; set; }

        [FirestoreProperty("isReturned")]
        public bool IsReturned { get; set; } = false;

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir")]
        [FirestoreProperty("notes")]
        public string Notes { get; set; } = string.Empty;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [FirestoreProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - not stored in Firestore
        public Book? Book { get; set; }
        public User? User { get; set; }
    }
}
