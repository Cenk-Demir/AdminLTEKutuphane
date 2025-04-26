using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad zorunludur")]
        [FirestoreProperty("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur")]
        [FirestoreProperty("Lastname")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [FirestoreProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
        [FirestoreProperty("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [FirestoreProperty("phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres zorunludur")]
        [FirestoreProperty("address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol zorunludur")]
        [FirestoreProperty("Role")]
        public string Role { get; set; } = "User";

        [FirestoreProperty("membershipnumber")]
        public string MembershipNumber { get; set; } = string.Empty;

        [FirestoreProperty("membershipstatus")]
        public string MembershipStatus { get; set; } = "Active";

        [FirestoreProperty("membershipstartdate")]
        public DateTime MembershipStartDate { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("membershipenddate")]
        public DateTime? MembershipEndDate { get; set; }

        [FirestoreProperty("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties - not stored in Firestore
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        // Computed property - not stored in Firestore
        public string FullName => $"{FirstName} {LastName}";
    }
}

