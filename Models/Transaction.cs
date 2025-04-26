using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class Transaction
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kitap ID zorunludur")]
        [FirestoreProperty("bookid")]
        public string BookId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı ID zorunludur")]
        [FirestoreProperty("userid")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "İşlem tarihi zorunludur")]
        [FirestoreProperty("transactiondate")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "İade tarihi zorunludur")]
        [FirestoreProperty("duedate")]
        public DateTime DueDate { get; set; }

        [FirestoreProperty("returndate")]
        public DateTime? ReturnDate { get; set; }

        [Required(ErrorMessage = "İşlem tipi zorunludur")]
        [FirestoreProperty("transactiontype")]
        public string TransactionType { get; set; } = string.Empty;

        [FirestoreProperty("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [FirestoreProperty("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - not stored in Firestore
        public Book? Book { get; set; }
        public User? User { get; set; }
    }
} 