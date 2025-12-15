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
            // HİZMETLERİ FORMATLI GETİR (Örn: "Pilates (60 dk)")
            var hizmetListesi = _context.Hizmetler
                .Select(h => new
                {
                    h.HizmetId,
                    // Burada metni birleştiriyoruz:
                    Gorunum = h.HizmetAdi + " (" + h.SureDakika + " dk - " + h.Ucret + " TL)"
                })
                .ToList();

            // SelectList'e diyoruz ki: "Arka planda ID'yi tut ama ekranda 'Gorunum' alanını göster"
            ViewBag.Hizmetler = new SelectList(hizmetListesi, "HizmetId", "Gorunum");

            // Antrenörleri normal getir
            ViewBag.Antrenorler = new SelectList(_context.Antrenorler.ToList(), "AntrenorId", "AdSoyad");

            return View();
        }

        // 3. RANDEVU AL (POST) - Kaydet
        [HttpPost]
        public async Task<IActionResult> Al(Randevu randevu)
        {
            ModelState.Remove("KullaniciId");
            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            // 1. Kullanıcıyı ve Hizmeti Bul
            var kullanici = await _userManager.GetUserAsync(User);
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            // Hizmetin süresini öğrenmek için veritabanından çekiyoruz
            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);
            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);

            // Verileri doldur
            randevu.KullaniciId = kullanici.Id;
            randevu.Durum = "Bekliyor";

            // Validation temizliği
            ModelState.Remove("Kullanici");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");
            ModelState.Remove("KullaniciId");

            if (secilenHizmet == null || secilenAntrenor == null)
            {
                ModelState.AddModelError("", "Hizmet veya Antrenör bulunamadı.");
            }
            else
            {
                // ============================================================
                // KURAL 1: GEÇMİŞ ZAMAN KONTROLÜ
                // ============================================================
                if (randevu.Tarih < DateTime.Today)
                {
                    ModelState.AddModelError("", "Geçmiş bir tarihe randevu alamazsınız.");
                }
                // Eğer bugünse ve saat geçmişse?
                else if (randevu.Tarih == DateTime.Today && randevu.BaslangicSaati < DateTime.Now.TimeOfDay)
                {
                    ModelState.AddModelError("", "Geçmiş bir saate randevu alamazsınız.");
                }

                // ============================================================
                // KURAL 2: MESAİ SAATİ KONTROLÜ (Basit String Parse)
                // ============================================================
                // Antrenörün saati "09:00-18:00" formatındaysa kontrol et
                if (!string.IsNullOrEmpty(secilenAntrenor.CalismaSaatleri) && secilenAntrenor.CalismaSaatleri.Contains("-"))
                {
                    try
                    {
                        var saatler = secilenAntrenor.CalismaSaatleri.Split('-'); // ["09:00", "18:00"]
                        var mesaiBaslangic = TimeSpan.Parse(saatler[0].Trim());
                        var mesaiBitis = TimeSpan.Parse(saatler[1].Trim());

                        // Randevu bitiş saati = Başlangıç + Hizmet Süresi
                        var randevuBitis = randevu.BaslangicSaati.Add(TimeSpan.FromMinutes(secilenHizmet.SureDakika));

                        if (randevu.BaslangicSaati < mesaiBaslangic || randevuBitis > mesaiBitis)
                        {
                            ModelState.AddModelError("", $"Bu antrenör sadece {secilenAntrenor.CalismaSaatleri} saatleri arasında çalışmaktadır.");
                        }
                    }
                    catch
                    {
                        // Format bozuksa (örn: "Haftaiçi her gün") bu kontrolü görmezden gel.
                    }
                }

                // ============================================================
                // KURAL 3: ÇAKIŞMA KONTROLÜ (EN ÖNEMLİSİ)
                // ============================================================
                if (ModelState.IsValid)
                {
                    // Yeni randevunun başlangıç ve bitiş anları
                    var yeniBaslangic = randevu.BaslangicSaati;
                    var yeniBitis = randevu.BaslangicSaati.Add(TimeSpan.FromMinutes(secilenHizmet.SureDakika));

                    // Veritabanında AYNI GÜN ve AYNI HOCA için çakışan randevu var mı?
                    // Mantık: (MevcutBaşla < YeniBit) VE (MevcutBit > YeniBaşla) ise çakışma vardır.
                    var cakismaVarMi = _context.Randevular
                        .Include(r => r.Hizmet) // Hizmet süresini bilmemiz lazım
                        .Where(r => r.AntrenorId == randevu.AntrenorId && r.Tarih == randevu.Tarih && r.Durum != "İptal Edildi") // İptal edilenler sorun değil
                        .ToList() // Client-side evaluation gerekebilir (TimeSpan hesaplamaları için)
                        .Any(mevcut =>
                        {
                            var mevcutBitis = mevcut.BaslangicSaati.Add(TimeSpan.FromMinutes(mevcut.Hizmet.SureDakika));
                            return (mevcut.BaslangicSaati < yeniBitis) && (mevcutBitis > yeniBaslangic);
                        });

                    if (cakismaVarMi)
                    {
                        ModelState.AddModelError("", "Seçtiğiniz saatlerde bu antrenörün başka bir randevusu var. Lütfen başka bir saat seçiniz.");
                    }
                }
            }

            // HATA YOKSA KAYDET
            if (ModelState.IsValid)
            {
                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // HATA VARSA LİSTELERİ TEKRAR DOLDUR
            var hizmetListesi = _context.Hizmetler
                .Select(h => new
                {
                    h.HizmetId,
                    Gorunum = h.HizmetAdi + " (" + h.SureDakika + " dk - " + h.Ucret + " TL)"
                })
                .ToList();

            ViewBag.Hizmetler = new SelectList(hizmetListesi, "HizmetId", "Gorunum");
            ViewBag.Antrenorler = new SelectList(_context.Antrenorler.ToList(), "AntrenorId", "AdSoyad");

            return View(randevu);
        }

        // ==========================================
        // ADMİN PANELİ İŞLEMLERİ
        // ==========================================

        // 1. TÜM RANDEVULARI LİSTELE (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult TumRandevular()
        {
            // Bekleyenler en üstte görünsün diye OrderBy kullanıyoruz
            var randevular = _context.Randevular
                .Include(r => r.Kullanici) // Randevuyu kim aldı?
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderBy(x => x.Durum == "Bekliyor" ? 0 : 1) // Bekleyenler önce
                .ThenByDescending(x => x.Tarih) // Sonra tarihe göre
                .ToList();

            return View(randevular);
        }

        // 2. RANDEVUYU ONAYLA (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = "Onaylandı";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TumRandevular");
        }

        // 3. RANDEVUYU İPTAL ET / REDDET (Sadece Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Iptal(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = "İptal Edildi";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TumRandevular");
        }
        // AJAX İÇİN API: Seçilen hizmeti veren antrenörleri getirir
        [HttpGet]
        public IActionResult GetAntrenorlerByHizmet(int hizmetId)
        {
            var antrenorler = _context.Antrenorler
                .Where(a => a.Hizmetler.Any(h => h.HizmetId == hizmetId)) // O hizmeti verenler
                .Select(a => new {
                    a.AntrenorId,
                    a.AdSoyad
                })
                .ToList();

            return Json(antrenorler); // JSON formatında gönder
        }

        // ==========================================
        // AKILLI SAAT SİSTEMİ (SALON KURALLARI DAHİL)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetMusaitSaatler(int antrenorId, int hizmetId, DateTime tarih)
        {
            var antrenor = await _context.Antrenorler.FindAsync(antrenorId);
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);

            if (antrenor == null || hizmet == null) return Json(new List<object>());

            // ----------------------------------------------------
            // KURAL 1: PAZAR GÜNÜ KONTROLÜ
            // ----------------------------------------------------
            if (tarih.DayOfWeek == DayOfWeek.Sunday)
            {
                // Boş liste döndür (Hiçbir saat yok, çünkü kapalı)
                return Json(new List<object>());
                // İstersen burada client tarafında "Bugün salon kapalı" uyarısı verdirebiliriz
                // ama şimdilik saat çıkmaması yeterli.
            }

            // ----------------------------------------------------
            // KURAL 2: SALON ÇALIŞMA SAATLERİ (Footer ile Uyumlu)
            // ----------------------------------------------------
            TimeSpan salonAcilis = TimeSpan.FromHours(8);  // 08:00
            TimeSpan salonKapanis = TimeSpan.FromHours(23); // 23:00 (Hafta içi)

            // Cumartesi ise 22:00'de kapanıyor
            if (tarih.DayOfWeek == DayOfWeek.Saturday)
            {
                salonKapanis = TimeSpan.FromHours(22);
            }

            // ----------------------------------------------------
            // KURAL 3: ANTRENÖR MESAİSİ İLE SALON SAATİNİ KESİŞTİR
            // ----------------------------------------------------
            // Varsayılan olarak antrenör tüm gün çalışıyor diyelim, 
            // ama salon saatleri bunu kısıtlayacak.
            TimeSpan hocaBaslangic = salonAcilis;
            TimeSpan hocaBitis = salonKapanis;

            // Eğer hocanın özel saati varsa (Örn: "10:00-16:00") onu al
            if (!string.IsNullOrEmpty(antrenor.CalismaSaatleri) && antrenor.CalismaSaatleri.Contains("-"))
            {
                try
                {
                    var parcalar = antrenor.CalismaSaatleri.Split('-');
                    var ozelBaslangic = TimeSpan.Parse(parcalar[0].Trim());
                    var ozelBitis = TimeSpan.Parse(parcalar[1].Trim());

                    // Kesişim Mantığı:
                    // Hoca 07:00 diyor ama Salon 08:00'de açılıyor -> 08:00 al.
                    if (ozelBaslangic > hocaBaslangic) hocaBaslangic = ozelBaslangic;
                    if (ozelBaslangic < salonAcilis) hocaBaslangic = salonAcilis;

                    // Hoca 24:00 diyor ama Salon 23:00'de kapanıyor -> 23:00 al.
                    if (ozelBitis < hocaBitis) hocaBitis = ozelBitis;
                    if (ozelBitis > salonKapanis) hocaBitis = salonKapanis;
                }
                catch { /* Format hatası varsa salon saatlerini baz al */ }
            }

            // ----------------------------------------------------
            // KURAL 4: RANDEVULARI KONTROL ET VE SLOTLARI OLUŞTUR
            // ----------------------------------------------------
            var doluRandevular = _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == antrenorId
                         && r.Tarih.Date == tarih.Date
                         && r.Durum != "İptal Edildi")
                .ToList();

            var slotlar = new List<object>();
            var suankiZaman = hocaBaslangic;

            // Döngü: Başlangıçtan Bitişe kadar slot oluştur
            while (suankiZaman + TimeSpan.FromMinutes(hizmet.SureDakika) <= hocaBitis)
            {
                var slotBitis = suankiZaman + TimeSpan.FromMinutes(hizmet.SureDakika);
                bool doluMu = false;

                // Geçmiş saat kontrolü
                if (tarih.Date == DateTime.Today && suankiZaman < DateTime.Now.TimeOfDay)
                {
                    doluMu = true;
                }
                else
                {
                    // Çakışma Kontrolü
                    foreach (var randevu in doluRandevular)
                    {
                        var randevuBitis = randevu.BaslangicSaati.Add(TimeSpan.FromMinutes(randevu.Hizmet.SureDakika));
                        if (randevu.BaslangicSaati < slotBitis && randevuBitis > suankiZaman)
                        {
                            doluMu = true;
                            break;
                        }
                    }
                }

                slotlar.Add(new
                {
                    saat = suankiZaman.ToString(@"hh\:mm"),
                    durum = doluMu ? "Dolu" : "Bos"
                });

                suankiZaman = suankiZaman.Add(TimeSpan.FromMinutes(hizmet.SureDakika));
            }

            return Json(slotlar);
        }
    }

}