using System;
using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace AdminLTEKutuphane.Models
{
    [FirestoreData]
    public class UserModel
    {
        [FirestoreProperty]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [FirestoreProperty]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [FirestoreProperty]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [FirestoreProperty]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [FirestoreProperty]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol alanı zorunludur.")]
        [FirestoreProperty]
        public string Role { get; set; } = "user";

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [FirestoreProperty]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        [FirestoreProperty]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Meslek alanı zorunludur.")]
        [FirestoreProperty]
        public string Job { get; set; } = string.Empty;

        [FirestoreProperty]
        public string MembershipNumber { get; set; } = string.Empty;

        [FirestoreProperty]
        public string MembershipStatus { get; set; } = "Active";

        [FirestoreProperty]
        public Timestamp MembershipStartDate { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

        [FirestoreProperty]
        public Timestamp MembershipEndDate { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow.AddYears(1));

        [FirestoreProperty]
        public Timestamp CreatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

        [FirestoreProperty]
        public Timestamp UpdatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

        [FirestoreProperty]
        public Timestamp LastLoginAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

        // Navigation properties - not stored in Firestore
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        // Computed property - not stored in Firestore
        public string FullName => $"{FirstName} {LastName}";
    }
}

