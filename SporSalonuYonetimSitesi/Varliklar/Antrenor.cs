using SporSalonuYonetimSitesi.Varliklar;

namespace SporSalonuYonetimSitesi.Varliklar
{
    public class Antrenor
    {
        public int AntrenorId { get; set; }
        public string AdSoyad { get; set; }
        public string UzmanlikAlani { get; set; } // Fitness, Pilates
        public string? ResimYolu { get; set; }
        public string? CalismaSaatleri { get; set; } // "09:00 - 18:00"

        public ICollection<Hizmet> Hizmetler { get; set; } = new List<Hizmet>();
        // Nullable (?) yaptık ki hata vermesin
        public ICollection<Randevu>? Randevular { get; set; }
    }
}