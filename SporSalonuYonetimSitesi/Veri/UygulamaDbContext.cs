using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSitesi.Varliklar;

namespace SporSalonuYonetimSitesi.Veri
{
    public class UygulamaDbContext : IdentityDbContext<Kullanici>
    {
        public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options) : base(options) { }

        // Tablo İsimleri
        public DbSet<Antrenor> Antrenorler { get; set; } // SQL'de 'Antrenorler' olacak
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
