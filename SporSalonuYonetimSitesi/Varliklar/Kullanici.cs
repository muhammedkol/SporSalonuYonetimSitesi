using Microsoft.AspNetCore.Identity;
using SporSalonuYonetimSitesi.Varliklar;

namespace SporSalonuYonetimSitesi.Varliklar // Proje ismin farklıysa burayı düzelt
{
    public class Kullanici : IdentityUser
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string? ResimYolu { get; set; } // Zorunlu değil

        // Yapay Zeka Verileri
        public int? Yas { get; set; }
        public double? Kilo { get; set; }
        public double? Boy { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}