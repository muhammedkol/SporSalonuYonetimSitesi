using Microsoft.AspNetCore.Identity;

namespace SporSalonuYonetimSitesi.Varliklar
{
    public class Kullanici : IdentityUser
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string? ResimYolu { get; set; }
        public int? Yas { get; set; }
        public double? Kilo { get; set; }
        public double? Boy { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
