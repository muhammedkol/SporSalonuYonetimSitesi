using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetimSitesi.Veri;
using SporSalonuYonetimSitesi.Varliklar;
using System.Linq;

namespace SporSalonuYonetimSitesi.Controllers
{
    // Bu etiket, bunun bir API olduğunu ve adresinin /api/AntrenorApi olduğunu belirtir
    [Route("api/[controller]")]
    [ApiController]
    public class AntrenorApiController : ControllerBase
    {
        private readonly UygulamaDbContext _context;

        public AntrenorApiController(UygulamaDbContext context)
        {
            _context = context;
        }

        // 1. TÜM ANTRENÖRLERİ GETİR (JSON)
        // Adres: GET /api/AntrenorApi
        [HttpGet]
        public IActionResult TumAntrenorler()
        {
            // LINQ Kullanımı: Sadece gerekli alanları seçiyoruz (Select)
            var antrenorler = _context.Antrenorler
                .Select(x => new
                {
                    x.AdSoyad,
                    x.UzmanlikAlani,
                    x.CalismaSaatleri
                })
                .ToList();

            return Ok(antrenorler); // Veriyi JSON formatında döndürür
        }

        // 2. UZMANLIĞA GÖRE FİLTRELE (LINQ WHERE)
        // Adres: GET /api/AntrenorApi/Uzmanlik/Fitness
        [HttpGet("Uzmanlik/{alan}")]
        public IActionResult UzmanligaGoreGetir(string alan)
        {
            // LINQ ile filtreleme (Where)
            var sonuc = _context.Antrenorler
                .Where(x => x.UzmanlikAlani.Contains(alan)) // İçinde geçen kelimeye göre ara
                .Select(x => new
                {
                    x.AdSoyad,
                    x.UzmanlikAlani
                })
                .ToList();

            if (sonuc.Count == 0)
            {
                return NotFound("Bu uzmanlık alanında antrenör bulunamadı.");
            }

            return Ok(sonuc);
        }
    }
}