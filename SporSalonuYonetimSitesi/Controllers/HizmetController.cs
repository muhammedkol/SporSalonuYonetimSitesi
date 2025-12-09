using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetimSitesi.Veri;      // Veritabanı context'i burada
using SporSalonuYonetimSitesi.Varliklar; // Tablolar burada
using System.Linq;

namespace SporSalonuYonetimSitesi.Controllers
{
    public class HizmetController : Controller
    {
        private readonly UygulamaDbContext _context;

        public HizmetController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public IActionResult Index()
        {
            var hizmetler = _context.Hizmetler.ToList();
            return View(hizmetler);
        }

        // 2. EKLEME (GET) - Boş form göster
        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        // 3. EKLEME (POST) - Kaydet
        [HttpPost]
        public IActionResult Ekle(Hizmet yeniHizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Hizmetler.Add(yeniHizmet);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(yeniHizmet);
        }

        // 4. DÜZENLEME (GET) - Veriyi bul getir
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet == null) return NotFound();
            return View(hizmet);
        }

        // 5. DÜZENLEME (POST) - Güncelle
        [HttpPost]
        public IActionResult Duzenle(Hizmet guncelHizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Hizmetler.Update(guncelHizmet);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(guncelHizmet);
        }

        // 6. SİLME
        public IActionResult Sil(int id)
        {
            var hizmet = _context.Hizmetler.Find(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}