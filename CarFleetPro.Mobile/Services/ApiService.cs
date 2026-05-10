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

        
        private static readonly string BaseUrl = "https://carfleetpro-hcf2f6hua6f2h5f0.westeurope-01.azurewebsites.net/api/";

        private static HttpMessageHandler GetInsecureHandler()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            };
            
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate;
            return handler;
        }

        public ApiService()
        {
            _httpClient = new HttpClient(GetInsecureHandler());
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
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
                    _cachedETag = null;
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
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

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
                    _cachedVehicles = null;
                    _cachedETag = null;
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
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"Vehicle/{id}");
                if (response.IsSuccessStatusCode) { _cachedVehicles = null; _cachedETag = null; return true; }
                System.Diagnostics.Debug.WriteLine($"[API] DELETE Hata: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] DELETE Exception: {ex.Message}");
                return false;
            }
        }

        
        
        

        private static List<Vehicle>? _cachedVehicles = null;
        private static string? _cachedETag = null;

        public async Task<List<Vehicle>> GetVehiclesAsync(bool forceRefresh = false)
        {
            await SetAuthorizationHeader();
            if (forceRefresh) { _cachedVehicles = null; _cachedETag = null; }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "Vehicle/cards");
                if (!string.IsNullOrEmpty(_cachedETag) && _cachedVehicles != null)
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(_cachedETag));

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotModified && _cachedVehicles != null)
                {
                    System.Diagnostics.Debug.WriteLine("[API] 304 Not Modified → Cache'den döndü");
                    return new List<Vehicle>(_cachedVehicles);
                }

                if (response.IsSuccessStatusCode)
                {
                    var vehicles = await response.Content.ReadFromJsonAsync<List<Vehicle>>();
                    if (vehicles != null)
                    {
                        _cachedVehicles = vehicles;
                        if (response.Headers.ETag != null) _cachedETag = response.Headers.ETag.Tag;
                        return new List<Vehicle>(_cachedVehicles);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Cards hatası: {ex.Message}");
                if (_cachedVehicles != null) return new List<Vehicle>(_cachedVehicles);
            }
            return new List<Vehicle>();
        }


        
        

        
        public async Task<VehicleDetail?> GetVehicleDetailsAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<VehicleDetail>($"Vehicle/{id}/details");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Vehicle Details Hatası: {ex.Message}");
                return null;
            }
        }

        
        
        
        public async Task<(bool Success, string Message)> SendToMaintenanceAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsync($"Vehicle/{id}/maintenance/start", null);
                var content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) { _cachedVehicles = null; _cachedETag = null; return (true, "Araç bakıma alındı!"); }
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  GİRİŞ / KAYIT / OTURUM
        // ==========================================

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
                        content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        
        
        
        public async Task<(bool Success, string Message)> RegisterUserAsync(string fullName, string email, string password, string role)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { FullName = fullName, Email = email, Password = password, Role = role };
                var response = await _httpClient.PostAsJsonAsync("Auth/register", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Yeni kullanıcı hesabı başarıyla oluşturuldu!");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        /// <summary>
        /// GET /api/Auth/me — Profil bilgilerini çek
        /// </summary>
        public async Task<UserProfile?> GetProfileAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("Auth/me");
                if (!response.IsSuccessStatusCode) return null;

                var content = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<UserProfile>(
                    content,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Profil Hatası: {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// PUT /api/Auth/profile — Profil güncelle
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateProfileAsync(string fullName, string email, string? phone)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { FullName = fullName, Email = email, PhoneNumber = phone };
                var response = await _httpClient.PutAsJsonAsync("Auth/profile", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Profil başarıyla güncellendi!");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        
        
        
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { OldPassword = oldPassword, NewPassword = newPassword };
                var response = await _httpClient.PostAsJsonAsync("Auth/change-password", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Şifreniz başarıyla güncellendi!");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        /// <summary>
        /// POST /api/Auth/forgot-password — Şifre sıfırlama kodu gönder
        /// </summary>
        public async Task<(bool Success, string Message, string? Token)> ForgotPasswordAsync(string email)
        {
            try
            {
                var data = new { Email = email };
                var response = await _httpClient.PostAsJsonAsync("Auth/forgot-password", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Token'ı al (dev modda kullanıcıya reset için lazım)
                    var result = System.Text.Json.JsonSerializer.Deserialize<ForgotPasswordResponse>(
                        content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, "Şifre sıfırlama kodu e-posta adresinize gönderildi!", result?.Token);
                }
                return (false, content.Trim().Trim('"'), null);
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}", null); }
        }

        
        
        
        public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var data = new { Email = email, Token = token, NewPassword = newPassword };
                var response = await _httpClient.PostAsJsonAsync("Auth/reset-password", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Şifreniz başarıyla sıfırlandı!");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  MÜŞTERİLER
        // ==========================================

        /// <summary>
        /// GET /api/Customer — Tüm müşterileri listele
        /// </summary>
        public async Task<List<CustomerInfo>> GetCustomersAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<CustomerInfo>>("Customer") ?? new List<CustomerInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Müşteri Listesi Hatası: {ex.Message}");
                return new List<CustomerInfo>();
            }
        }

        /// <summary>
        /// GET /api/Customer/search?q= — Müşteri ara
        /// </summary>
        public async Task<List<CustomerInfo>> SearchCustomersAsync(string query)
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<CustomerInfo>>($"Customer/search?q={Uri.EscapeDataString(query)}") ?? new List<CustomerInfo>();
            }
            catch { return new List<CustomerInfo>(); }
        }

        /// <summary>
        /// GET /api/Customer/names — Kiralama formu için müşteri listesi
        /// </summary>
        public async Task<List<CustomerName>> GetCustomerNamesAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<CustomerName>>("Customer/names") ?? new List<CustomerName>();
            }
            catch { return new List<CustomerName>(); }
        }

        // ==========================================
        //  KİRALAMA
        // ==========================================

        /// <summary>
        /// POST /api/Rental — Yeni kiralama oluştur
        /// </summary>
        public async Task<(bool Success, string Message)> CreateRentalAsync(int customerId, int vehicleId, DateTime startDate, DateTime endDate, decimal depositAmount, string notes)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new
                {
                    CustomerId = customerId,
                    VehicleId = vehicleId,
                    StartDate = startDate.ToUniversalTime(),
                    PlannedEndDate = endDate.ToUniversalTime(),
                    DepositAmount = depositAmount,
                    Notes = notes
                };
                var response = await _httpClient.PostAsJsonAsync("Rental", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) { _cachedVehicles = null; _cachedETag = null; return (true, "Kiralama işlemi başarıyla tamamlandı!"); }
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        
        
        

        
        
        
        // ==========================================
        //  DASHBOARD
        // ==========================================

        public async Task<DashboardStats?> GetDashboardStatsAsync(string? branch = null)
        {
            try
            {
                await SetAuthorizationHeader();
                var url = "Dashboard/stats";
                if (!string.IsNullOrEmpty(branch) && branch != "Tümü")
                    url += $"?branch={Uri.EscapeDataString(branch)}";

                return await _httpClient.GetFromJsonAsync<DashboardStats>(url,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Dashboard Stats Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AlertInfo>> GetAlertsAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<AlertInfo>>("Alert") ?? new List<AlertInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] Alert Hatası: {ex.Message}");
                return new List<AlertInfo>();
            }
        }

        // ==========================================
        //  ARAÇ FOTOĞRAFLARI
        // ==========================================

        /// <summary>
        /// GET /api/vehicleimage/{vehicleId} — Araçtaki tüm fotoğrafları getir
        /// </summary>
        public async Task<List<VehicleImageInfo>> GetVehicleImagesAsync(int vehicleId)
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<VehicleImageInfo>>(
                    $"VehicleImage/{vehicleId}",
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<VehicleImageInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] VehicleImage List Hatası: {ex.Message}");
                return new List<VehicleImageInfo>();
            }
        }

        /// <summary>
        /// POST /api/vehicleimage/upload/{vehicleId} — Tekil fotoğraf yükle
        /// </summary>
        public async Task<(bool Success, string Message, VehicleImageInfo? Image)> UploadVehicleImageAsync(int vehicleId, string filePath, string fileName, string contentType = "image/jpeg")
        {
            try
            {
                await SetAuthorizationHeader();

                await using var stream = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                content.Add(fileContent, "file", fileName);

                var response = await _httpClient.PostAsync($"VehicleImage/upload/{vehicleId}", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var image = System.Text.Json.JsonSerializer.Deserialize<VehicleImageInfo>(
                        responseBody,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, "Fotoğraf yüklendi!", image);
                }

                var errorMsg = responseBody.Trim().Trim('"');
                return (int)response.StatusCode switch
                {
                    400 => (false, string.IsNullOrEmpty(errorMsg) ? "Geçersiz dosya." : errorMsg, null),
                    403 => (false, "Bu işlem için yetkiniz yok.", null),
                    404 => (false, "Araç bulunamadı.", null),
                    _ => (false, $"Sunucu hatası ({(int)response.StatusCode}).", null)
                };
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}", null);
            }
        }

        /// <summary>
        /// DELETE /api/vehicleimage/{imageId} — Fotoğrafı sil
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteVehicleImageAsync(int imageId)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"VehicleImage/{imageId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Fotoğraf silindi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// PUT /api/vehicleimage/{imageId}/set-primary — Birincil fotoğrafı değiştir
        /// </summary>
        public async Task<(bool Success, string Message)> SetPrimaryImageAsync(int imageId)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsync($"VehicleImage/{imageId}/set-primary", null);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Kapak fotoğrafı güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }
    }

    
    
    

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }
    }

    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}