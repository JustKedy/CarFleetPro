using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using CarFleetPro.Mobile.Models;

namespace CarFleetPro.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        // KRİTİK BİLGİ: Android emülatöründe kendi bilgisayarının localhost'una 
        // bağlanmak için "localhost" yerine "10.0.2.2" yazmalısın!
        // Alper'in API portu kaçsa (örn: 5001) onu buraya yazın.
        private const string BaseUrl = "http://10.0.2.2:5161/api/";

        // Sertifika hatalarını yok sayan köprü (Sadece geliştirme aşaması için!)
        private static HttpMessageHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            // SSL sertifikası ne olursa olsun (geçersiz/sahte) TRUE dönüp kabul et
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            };
            return handler;
        }

        public ApiService()
        {
            _httpClient = new HttpClient(GetInsecureHandler()); // Sertifikayı sorgulama!
            _httpClient.BaseAddress = new Uri(BaseUrl);

            // 50 saniye içinde cevap vermezse bekleme, direkt patlat!
            _httpClient.Timeout = TimeSpan.FromSeconds(50);
        }

        // --- GÜVENLİK GÖREVLİSİ (Token Yönetimi) ---
        private async Task SetAuthorizationHeader()
        {
            var token = await SecureStorage.Default.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // --- ALPER'İN POST /api/Auth/login UCU ---
        public async Task<bool> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("Auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                // Alper'den gelen Token'ı alıp SecureStorage'a güvenle kaydediyoruz
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                await SecureStorage.Default.SetAsync("jwt_token", result.Token);
                return true;
            }
            return false;
        }

        // --- ALPER'İN GET /api/Vehicle/cards UCU (Ana Sayfayı Dolduracak Olan) ---
        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            // İsteği atmadan önce Güvenlik Görevlisine Token'ı gösteriyoruz
            await SetAuthorizationHeader();

            var response = await _httpClient.GetAsync("Vehicle/cards");

            if (response.IsSuccessStatusCode)
            {
                // Alper'den gelen JSON listesini bizim Vehicle modelimize çeviriyoruz
                var vehicles = await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                return vehicles ?? new List<Vehicle>();
            }

            // Hata varsa veya token geçersizse boş liste dön
            return new List<Vehicle>();
        }
    }

    // Token'ı okuyabilmek için ufak bir yardımcı sınıf
    public class AuthResponse
    {
        public string Token { get; set; }
    }
}