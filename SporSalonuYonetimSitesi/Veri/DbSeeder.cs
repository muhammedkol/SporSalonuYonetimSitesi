using Microsoft.AspNetCore.Identity;
using SporSalonuYonetimSitesi.Varliklar;

namespace SporSalonuYonetimSitesi.Veri
{
    public static class DbSeeder
    {
        // Bu metod program başlarken çalışacak
        public static async Task VerileriYukle(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Kullanici>>();

            // 1. ROLLERİ OLUŞTUR (Eğer yoksa)
            string[] roller = { "Admin", "Uye" };
            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. ADMİN KULLANCISINI OLUŞTUR
            // PDF'teki bilgi: ogrencinumarasi@sakarya.edu.tr / Şifre: sau

            // BURAYI KENDİ NUMARANLA DEĞİŞTİR! 👇
            string adminEmail = "G221210012@sakarya.edu.tr";
            string adminSifre = "sau";

            var adminKullanici = await userManager.FindByEmailAsync(adminEmail);
            if (adminKullanici == null)
            {
                var yeniAdmin = new Kullanici
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Ad = "Sistem",
                    Soyad = "Yöneticisi",
                    EmailConfirmed = true
                };

                // Admini kaydet
                var sonuc = await userManager.CreateAsync(yeniAdmin, adminSifre);

                // Eğer başarılıysa "Admin" rolünü ver
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(yeniAdmin, "Admin");
                }
            }
        }
    }
}