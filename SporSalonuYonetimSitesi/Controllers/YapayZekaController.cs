using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SporSalonuYonetimSitesi.Controllers
{
    public class YapayZekaController : Controller
    {
        private const string ApiKey = "AIzaSyDNBOAmDiDd04LUMSjFCPpRJ29LhphE6-g";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OneriAl(int boy, int kilo, string cinsiyet, string hedef)
        {
            string prompt = $"Ben {boy} cm boyunda, {kilo} kg ağırlığında bir {cinsiyet} bireyim. " +
                            $"Hedefim: {hedef}. " +
                            $"Bana maddeler halinde 1 günlük örnek diyet listesi ve 3 tane en önemli egzersiz önerisi yazar mısın? " +
                            $"Cevabı HTML formatında (ul, li, strong etiketleri kullanarak) ver. Sadece içeriği ver.";

            string yapayZekaCevabi = "";

            try
            {
                using (var client = new HttpClient())
                {
                    // ✅ EN GÜNCEL VE HIZLI MODEL: 'gemini-1.5-flash'
                    // Google şu an bunu destekliyor, 'gemini-pro' eski kaldığı için hata vermişti.
                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={ApiKey}";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new { parts = new[] { new { text = prompt } } }
                        }
                    };

                    var jsonContent = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonDocument doc = JsonDocument.Parse(responseString))
                        {
                            yapayZekaCevabi = doc.RootElement
                                .GetProperty("candidates")[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString();
                        }
                    }
                    else
                    {
                        // Hata olursa yine ekranda görelim
                        throw new Exception($"API Hatası: {response.StatusCode} <br> Detay: {responseString}");
                    }
                }
            }
            catch (Exception ex)
            {
                // HATA ALIRSAK KIRMIZI KUTUDA GÖSTER
                yapayZekaCevabi = $"<div class='alert alert-danger'><strong>🛑 HATA OLUŞTU:</strong><br>{ex.Message}</div>";
            }

            return Json(new { success = true, cevap = yapayZekaCevabi });
        }
    }
}