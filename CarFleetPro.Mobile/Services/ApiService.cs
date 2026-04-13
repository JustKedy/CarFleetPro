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
        private static readonly string BaseUrl = Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android
            ? "http://10.0.2.2:5161/api/"
            : "http://localhost:5161/api/";

        private static HttpMessageHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            };
            return handler;
        }

        public ApiService()
        {
            _httpClient = new HttpClient(GetInsecureHandler());
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(50);
        }

        private async Task SetAuthorizationHeader()
        {
            var token = await SecureStorage.Default.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<string>>("CarBrands") ?? new List<string>(); }
            catch { return new List<string>(); }
        }

        public async Task<List<string>> GetModelsAsync(string brand)
        {
            try { return await _httpClient.GetFromJsonAsync<List<string>>($"CarModels/{brand}") ?? new List<string>(); }
            catch { return new List<string>(); }
        }

        public async Task<List<string>> GetColorsAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<string>>("CarColors") ?? new List<string>(); }
            catch { return new List<string>(); }
        }

        public async Task<List<string>> GetStatusesAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<string>>("VehicleStatuses") ?? new List<string>(); }
            catch { return new List<string> { "MÜSAİT", "DOLU", "BAKIMDA" }; }
        }

        public async Task<(bool Success, string Message)> CreateVehicleAsync(CreateVehicleRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Vehicle", request);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] POST Vehicle → {(int)response.StatusCode}: {content}");

                if (response.IsSuccessStatusCode)
                {
                    _cachedVehicles = null;
                    return (true, "Araç başarıyla eklendi!");
                }

                var statusCode = (int)response.StatusCode;
                var body = content.Trim().Trim('"');

                return statusCode switch
                {
                    401 => (false, "Oturum açmanız gerekiyor. Lütfen tekrar giriş yapın."),
                    403 => (false, "Bu işlem için yetkiniz yok."),
                    400 => (false, string.IsNullOrEmpty(body) ? "Geçersiz veri gönderildi." : body),
                    409 => (false, "Bu plakaya sahip araç zaten kayıtlı."),
                    _ => (false, string.IsNullOrEmpty(body) ? $"Sunucu hatası ({statusCode})." : $"({statusCode}) {body}")
                };
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        // =========================================================================
        // [YENİ EKLENDİ] --- PUT /api/Vehicle/{id} (Araç Güncelleme) ---
        // =========================================================================
        public async Task<(bool Success, string Message)> UpdateVehicleAsync(int id, CreateVehicleRequest request)
        {
            try
            {
                await SetAuthorizationHeader();

                var response = await _httpClient.PutAsJsonAsync($"Vehicle/{id}", request);
                var content = await response.Content.ReadAsStringAsync();

                System.Diagnostics.Debug.WriteLine($"[API] PUT Vehicle → {(int)response.StatusCode}: {content}");

                if (response.IsSuccessStatusCode)
                {
                    _cachedVehicles = null; // Liste değişti, cache'i temizle
                    return (true, "Araç başarıyla güncellendi!");
                }

                var statusCode = (int)response.StatusCode;
                var body = content.Trim().Trim('"');

                return statusCode switch
                {
                    401 => (false, "Oturum süresi dolmuş."),
                    403 => (false, "Bu işlem için yetkiniz yok."),
                    400 => (false, string.IsNullOrEmpty(body) ? "Geçersiz veri gönderildi." : body),
                    404 => (false, "Güncellenecek araç bulunamadı."),
                    _ => (false, string.IsNullOrEmpty(body) ? $"Sunucu hatası ({statusCode})." : $"({statusCode}) {body}")
                };
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        // =========================================================================
        // [YENİ EKLENDİ] --- DELETE /api/Vehicle/{id} (Araç Silme) ---
        // =========================================================================
        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"Vehicle/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _cachedVehicles = null; // Liste değişti, cache'i temizle
                    return true;
                }

                System.Diagnostics.Debug.WriteLine($"[API] DELETE Hata: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] DELETE Exception: {ex.Message}");
                return false;
            }
        }

        // --- POST /api/Auth/login ---
        public async Task<(bool Success, string Message)> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var response = await _httpClient.PostAsJsonAsync("Auth/login", loginData);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(
                        content,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Token != null)
                    {
                        await SecureStorage.Default.SetAsync("jwt_token", result.Token);
                        return (true, "Giriş başarılı!");
                    }
                    return (false, $"Sunucudan geçersiz yanıt. İçerik: {content}");
                }

                var body = content.Trim().Trim('"');
                return (int)response.StatusCode switch
                {
                    401 => (false, string.IsNullOrEmpty(body) ? "Email veya şifre hatalı." : body),
                    _ => (false, string.IsNullOrEmpty(body) ? $"Hata ({(int)response.StatusCode})." : body)
                };
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        private static List<Vehicle>? _cachedVehicles = null;
        private static DateTime _lastDbUpdateTime = DateTime.MinValue;

        // --- ALPER'İN GET /api/Vehicle/cards UCU ---
        public async Task<List<Vehicle>> GetVehiclesAsync(bool forceRefresh = false)
        {
            await SetAuthorizationHeader();
            DateTime currentDbUpdateTime = DateTime.MinValue;
            if (forceRefresh) _cachedVehicles = null;

            try
            {
                var lastUpdatedResponse = await _httpClient.GetAsync("Vehicle/last-updated");
                if (lastUpdatedResponse.IsSuccessStatusCode)
                {
                    var dbLastUpdatedText = await lastUpdatedResponse.Content.ReadAsStringAsync();
                    if (DateTime.TryParse(dbLastUpdatedText.Trim('"'), out DateTime dbLastUpdated))
                    {
                        currentDbUpdateTime = dbLastUpdated;
                        if (_cachedVehicles != null && _lastDbUpdateTime >= dbLastUpdated)
                        {
                            return new List<Vehicle>(_cachedVehicles);
                        }
                    }
                }
            }
            catch { }

            var response = await _httpClient.GetAsync("Vehicle/cards");

            if (response.IsSuccessStatusCode)
            {
                var vehicles = await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                if (vehicles != null)
                {
                    _cachedVehicles = vehicles;
                    _lastDbUpdateTime = currentDbUpdateTime != DateTime.MinValue ? currentDbUpdateTime : DateTime.UtcNow;
                    return new List<Vehicle>(_cachedVehicles);
                }
            }
            return new List<Vehicle>();
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
    }
}