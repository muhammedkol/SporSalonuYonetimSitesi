namespace SporSalonuYonetimSitesi.Varliklar
{
    public class Antrenor
    {
        public int AntrenorId { get; set; }
        public string AdSoyad { get; set; }
        public string UzmanlikAlani { get; set; }
        public string? ResimYolu { get; set; }
        public string? CalismaSaatleri { get; set; }

        public ICollection<Randevu>? Randevular { get; set; }
    }
}
