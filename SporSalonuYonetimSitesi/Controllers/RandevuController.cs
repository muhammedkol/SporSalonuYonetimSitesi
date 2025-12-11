using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Giriş kontrolü için şart
using Microsoft.AspNetCore.Mvc.Rendering; // Dropdown listesi için
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSitesi.Varliklar;
using SporSalonuYonetimSitesi.Veri;

namespace SporSalonuYonetimSitesi.Controllers
{
    [Authorize] // DİKKAT: Sadece giriş yapanlar buraya girebilir!
    public class RandevuController : Controller
    {
        private readonly UygulamaDbContext _context;
        private readonly UserManager<Kullanici> _userManager;

        public RandevuController(UygulamaDbContext context, UserManager<Kullanici> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. RANDEVULARIM SAYFASI
        public async Task<IActionResult> Index()
        {
            var kullanici = await _userManager.GetUserAsync(User);

            // Sadece BENİM randevularımı getir + Antrenör ve Hizmet isimlerini de getir
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.KullaniciId == kullanici.Id)
                .ToList();

            return View(randevular);
        }

        // 2. RANDEVU AL (GET) - Formu Hazırla
        [HttpGet]
        public IActionResult Al()
        {
            // Dropdown kutularını dolduruyoruz (Hata çıkmasın diye burası önemli)
            ViewBag.Antrenorler = new SelectList(_context.Antrenorler.ToList(), "AntrenorId", "AdSoyad");
            ViewBag.Hizmetler = new SelectList(_context.Hizmetler.ToList(), "HizmetId", "HizmetAdi");

            return View();
        }

        // 3. RANDEVU AL (POST) - Kaydet
        [HttpPost]
        public async Task<IActionResult> Al(Randevu randevu)
        {
            var kullanici = await _userManager.GetUserAsync(User);

            // Eğer oturum süresi dolmuşsa girişe at
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            // Biz burada manuel dolduruyoruz
            randevu.KullaniciId = kullanici.Id;
            randevu.Durum = "Bekliyor";

            ModelState.Remove("KullaniciId");

            // Diğer ilişkileri de görmezden gel
            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            if (ModelState.IsValid)
            {
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Hata varsa kutuları tekrar doldur
            ViewBag.Antrenorler = new SelectList(_context.Antrenorler.ToList(), "AntrenorId", "AdSoyad");
            ViewBag.Hizmetler = new SelectList(_context.Hizmetler.ToList(), "HizmetId", "HizmetAdi");

            return View(randevu);
        }
    }
}