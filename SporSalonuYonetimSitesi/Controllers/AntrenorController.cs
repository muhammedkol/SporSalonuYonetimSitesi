using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetimSitesi.Varliklar;
using SporSalonuYonetimSitesi.Veri;
using System.Linq;

namespace SporSalonuYonetimSitesi.Controllers
{
    public class AntrenorController : Controller
    {
        private readonly UygulamaDbContext _context;

        public AntrenorController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME
        public IActionResult Index()
        {
            var antrenorler = _context.Antrenorler.ToList();
            return View(antrenorler);
        }

        // 2. EKLEME (GET)
        [HttpGet]
        public IActionResult Ekle()
        {
            return View();
        }

        // 3. EKLEME (POST)
        [HttpPost]
        public IActionResult Ekle(Antrenor yeniAntrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Antrenorler.Add(yeniAntrenor);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(yeniAntrenor);
        }

        // 4. DÜZENLEME (GET)
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            if (antrenor == null) return NotFound();
            return View(antrenor);
        }

        // 5. DÜZENLEME (POST)
        [HttpPost]
        public IActionResult Duzenle(Antrenor guncelAntrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Antrenorler.Update(guncelAntrenor);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(guncelAntrenor);
        }

        // 6. SİLME
        public IActionResult Sil(int id)
        {
            var antrenor = _context.Antrenorler.Find(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}