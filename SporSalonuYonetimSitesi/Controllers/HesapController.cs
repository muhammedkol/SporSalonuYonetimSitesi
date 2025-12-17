using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetimSitesi.Models;
using SporSalonuYonetimSitesi.Varliklar; // Kullanici sınıfı burada

namespace SporSalonuYonetimSitesi.Controllers
{
    public class HesapController : Controller
    {
        private readonly UserManager<Kullanici> _userManager;
        private readonly SignInManager<Kullanici> _signInManager;

        public HesapController(UserManager<Kullanici> userManager, SignInManager<Kullanici> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // 1. KAYIT OL (GET)
        [HttpGet]
        public IActionResult Kayit()
        {
            return View();
        }

        // 2. KAYIT OL (POST)
        [HttpPost]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = new Kullanici
                {
                    UserName = model.Email, // Kullanıcı adı email olsun
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad
                };

                var sonuc = await _userManager.CreateAsync(kullanici, model.Sifre);

                if (sonuc.Succeeded)
                {
                    await _userManager.AddToRoleAsync(kullanici, "Uye");
                    await _signInManager.SignInAsync(kullanici, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in sonuc.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        // 3. GİRİŞ YAP (GET)
        [HttpGet]
        public IActionResult Giris(string tip = "Uye") // Varsayılan tip 'Uye' olsun
        {
            // Giriş sayfasına bu tipi gönderiyoruz
            ViewBag.Tip = tip;
            return View();
        }

        // 4. GİRİŞ YAP (POST) - GÜVENLİK KONTROLLÜ HALİ
        [HttpPost]
        public async Task<IActionResult> Giris(GirisViewModel model, string tip) // 'tip' parametresini ekledik
        {
            if (ModelState.IsValid)
            {
                // 1. Önce kullanıcıyı bul (Şifreyi kontrol etmeden önce)
                var kullanici = await _userManager.FindByEmailAsync(model.Email);

                if (kullanici == null)
                {
                    ModelState.AddModelError("", "Böyle bir kullanıcı bulunamadı.");
                    // Tasarım bozulmasın diye tipi geri gönderiyoruz
                    ViewBag.Tip = tip;
                    return View(model);
                }

                // 2. KAPI KONTROLÜ (En Kritik Yer)
                if (tip == "Admin")
                {
                    // Eğer Yönetici Kapısından girmeye çalışıyorsa ama "Admin" değilse -> REDDET
                    if (!await _userManager.IsInRoleAsync(kullanici, "Admin"))
                    {
                        ModelState.AddModelError("", "Hata: Bu panelden sadece Yöneticiler giriş yapabilir!");
                        ViewBag.Tip = tip;
                        return View(model);
                    }
                }
                else // tip == "Uye"
                {
                    // Eğer Üye Kapısından girmeye çalışıyorsa ama aslında bir "Admin" ise -> REDDET
                    // (Yöneticilerin kafası karışmasın, kendi panelinden girsinler)
                    if (await _userManager.IsInRoleAsync(kullanici, "Admin"))
                    {
                        ModelState.AddModelError("", "Yöneticiler buradan giriş yapamaz. Lütfen Yönetici Girişini kullanın.");
                        ViewBag.Tip = tip;
                        return View(model);
                    }
                }

                // 3. Şifre Kontrolü ve Giriş
                var sonuc = await _signInManager.PasswordSignInAsync(model.Email, model.Sifre, model.BeniHatirla, false);

                if (sonuc.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Şifre hatalı.");
            }

            // Hata varsa tasarım bozulmasın diye tipi tekrar gönderiyoruz
            ViewBag.Tip = tip;
            return View(model);
        }

        // 5. ÇIKIŞ YAP
        public async Task<IActionResult> Cikis()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // 6. ERİŞİM ENGELLENDİ SAYFASI
        public IActionResult ErisimEngellendi()
        {
            return View();
        }
    }
}