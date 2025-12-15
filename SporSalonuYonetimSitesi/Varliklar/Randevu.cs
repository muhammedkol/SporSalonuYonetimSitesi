using SporSalonuYonetimSitesi.Varliklar;

namespace SporSalonuYonetimSitesi.Varliklar
{
    public class Randevu
    {
        public int RandevuId { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public string Durum { get; set; } = "Bekliyor";

        // İLİŞKİLER
        public string KullaniciId { get; set; }
        public Kullanici? Kullanici { get; set; }

        // Artık Egitmen değil ANTRENOR kullanıyoruz
        public int AntrenorId { get; set; }
        public Antrenor? Antrenor { get; set; }

        public int HizmetId { get; set; }
        public Hizmet? Hizmet { get; set; }
    }
}