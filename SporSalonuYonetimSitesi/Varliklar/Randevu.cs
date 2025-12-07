using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuYonetimSitesi.Varliklar
{
    public class Randevu
    {
        public int RandevuId { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan BaslangicSaati { get; set; }
        public string Durum { get; set; } = "Bekliyor";

        //Üye
        public string KullaniciId { get; set; }
        public Kullanici Kullanici { get; set; }

        //Antrenör
        public int AntrenörId { get; set; }
        public Antrenor Antrenor { get; set; }

        //Hizmet
        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }
    }
}
