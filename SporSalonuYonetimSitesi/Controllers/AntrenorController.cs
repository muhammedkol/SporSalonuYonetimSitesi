using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetimSitesi.Varliklar;
using SporSalonuYonetimSitesi.Veri;

namespace GymProje.Controllers
{
    [Authorize(Roles = "Admin")]
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
            // Hizmetleri Checkbox olarak göstermek için listeyi gönderiyoruz
            ViewBag.TumHizmetler = _context.Hizmetler.ToList();
            return View();
        }

        // 3. EKLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Ekle(Antrenor yeniAntrenor, int[] secilenHizmetIds)
        {
            // Seçilen hizmet ID'lerini bulup antrenöre ekliyoruz
            foreach (var id in secilenHizmetIds)
            {
                var hizmet = await _context.Hizmetler.FindAsync(id);
                if (hizmet != null)
                {
                    yeniAntrenor.Hizmetler.Add(hizmet);
                }
            }

            // Doğrulama kontrolünü biraz esnetiyoruz (ilişkiler yüzünden)
            ModelState.Remove("Hizmetler");

            if (ModelState.IsValid)
            {
                _context.Antrenorler.Add(yeniAntrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Hata olursa listeyi tekrar gönder
            ViewBag.TumHizmetler = _context.Hizmetler.ToList();
            return View(yeniAntrenor);
        }

        // 4. DÜZENLEME (GET) - Veriyi ve Hizmetleri Getir
        [HttpGet]
        public IActionResult Duzenle(int id)
        {
            // Hocayı bulurken HİZMETLERİNİ DE (.Include) getiriyoruz ki kutucukları işaretleyebilelim
            var antrenor = _context.Antrenorler
                .Include(x => x.Hizmetler)
                .FirstOrDefault(x => x.AntrenorId == id);

            if (antrenor == null) return NotFound();

            // Tüm hizmetlerin listesini de gönderiyoruz (Checkboxlar için)
            ViewBag.TumHizmetler = _context.Hizmetler.ToList();

            return View(antrenor);
        }

        // 5. DÜZENLEME (POST) - Güncelle
        [HttpPost]
        public async Task<IActionResult> Duzenle(Antrenor gelenAntrenor, int[] secilenHizmetIds)
        {
            // 1. Veritabanındaki gerçek kaydı (ilişkileriyle beraber) çekiyoruz
            var dbAntrenor = await _context.Antrenorler
                .Include(a => a.Hizmetler)
                .FirstOrDefaultAsync(a => a.AntrenorId == gelenAntrenor.AntrenorId);

            if (dbAntrenor == null) return NotFound();

            // 2. Normal bilgileri güncelle
            dbAntrenor.AdSoyad = gelenAntrenor.AdSoyad;
            dbAntrenor.UzmanlikAlani = gelenAntrenor.UzmanlikAlani;
            dbAntrenor.CalismaSaatleri = gelenAntrenor.CalismaSaatleri;

            // 3. İLİŞKİLERİ GÜNCELLE
            // Önce eski hizmetlerin hepsini siliyoruz
            dbAntrenor.Hizmetler.Clear();

            // Sonra yeni seçilenleri tek tek ekliyoruz
            foreach (var id in secilenHizmetIds)
            {
                var hizmet = await _context.Hizmetler.FindAsync(id);
                if (hizmet != null)
                {
                    dbAntrenor.Hizmetler.Add(hizmet);
                }
            }

            // 4. Kaydet
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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