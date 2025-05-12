using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminLTEKutuphane.Models
{
    public class User
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Rol")]
        public string Role { get; set; } = "User";

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Display(Name = "Meslek")]
        public string? Job { get; set; }

        [Display(Name = "Üyelik Numarası")]
        public string? MembershipNumber { get; set; }

        [Display(Name = "Üyelik Durumu")]
        public string? MembershipStatus { get; set; }

        [Display(Name = "Üyelik Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime MembershipStartDate { get; set; }

        [Display(Name = "Üyelik Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? MembershipEndDate { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Son Giriş Tarihi")]
        public DateTime? LastLoginAt { get; set; }

        [NotMapped]
        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";
    }
} 