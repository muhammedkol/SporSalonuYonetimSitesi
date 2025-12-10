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
        public IActionResult Giris()
        {
            return View();
        }

        // 4. GİRİŞ YAP (POST)
        [HttpPost]
        public async Task<IActionResult> Giris(GirisViewModel model)
        {
            if (ModelState.IsValid)
            {
                var sonuc = await _signInManager.PasswordSignInAsync(model.Email, model.Sifre, model.BeniHatirla, false);

                if (sonuc.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Email veya şifre hatalı.");
            }
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