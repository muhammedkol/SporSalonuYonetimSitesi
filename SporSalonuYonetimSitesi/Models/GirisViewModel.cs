using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetimSitesi.Models
{
    public class GirisViewModel
    {
        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; }

        public bool BeniHatirla { get; set; }
    }
}