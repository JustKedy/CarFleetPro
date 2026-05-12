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

        
        
        

        public async Task<List<LookupItem>> GetBrandsAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<LookupItem>>("Lookup/brands") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<LookupItem>> GetModelsAsync(int brandId)
        {
            try { return await _httpClient.GetFromJsonAsync<List<LookupItem>>($"Lookup/models/{brandId}") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<LookupItem>> GetCarTypesAsync()
        {
            // API'deki CarTypes alias'ını güncelleyip obje dönmesini sağlamak veya LookupController'a yeni endpoint eklemek gerekiyor. 
            // Şimdilik API'deki alias'ı kullanacağız ama API'yi güncelleyeceğiz.
            try { return await _httpClient.GetFromJsonAsync<List<LookupItem>>("CarTypes") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<LookupItem>> GetColorsAsync()
        {
            try { return await _httpClient.GetFromJsonAsync<List<LookupItem>>("Lookup/colors") ?? new(); }
            catch { return new(); }
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

        public async Task<(bool Success, string Message)> DeleteVehicleAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"Vehicle/{id}");
                if (response.IsSuccessStatusCode) 
                { 
                    _cachedVehicles = null; 
                    _cachedETag = null; 
                    return (true, "Araç başarıyla silindi."); 
                }
                
                var errorMsg = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(errorMsg) ? $"Sunucu hatası: {response.StatusCode}" : errorMsg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] DELETE Exception: {ex.Message}");
                return (false, $"Bağlantı hatası: {ex.Message}");
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

        /// <summary>PATCH /api/Vehicle/{id}/status — MÜSAİT | DOLU | BAKIMDA</summary>
        public async Task<(bool Success, string Message)> UpdateVehicleStatusAsync(int id, string status)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PatchAsJsonAsync($"Vehicle/{id}/status", new { Status = status });
                var content  = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _cachedVehicles = null;
                    _cachedETag     = null;
                    return (true, content.Trim().Trim('"'));
                }
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
        /// POST /api/Auth/forgot-password — 6 haneli OTP e-postayla gönderilir
        /// </summary>
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Auth/forgot-password", new { Email = email });
                var content  = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (true, "Doğrulama kodu e-posta adresinize gönderildi.");

                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
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

        /// <summary>
        /// Anlık kiralama: Müşteri kaydı gerektirmez.
        /// Ad-soyad ve telefon ile önce müşteriyi bulur veya oluşturur, sonra kiralar.
        /// </summary>
        public async Task<(bool Success, string Message)> CreateRentalWithGuestAsync(
            string firstName, string lastName, string phone,
            int vehicleId, DateTime startDate, DateTime endDate,
            decimal depositAmount, string notes,
            string tc = "", string licenseNo = "",
            DateTime licenseExpiry = default, string address = "Belirtilmedi")
        {
            try
            {
                await SetAuthorizationHeader();

                // 1. Telefon ile müşteriyi bul veya anlık oluştur
                var guestData = new
                {
                    FirstName           = firstName.Trim(),
                    LastName            = lastName.Trim(),
                    PhoneNumber         = phone.Trim(),
                    Email               = $"misafir_{phone.Trim().Replace(" ", "")}@carfleetpro.com",
                    IdentityNumber      = string.IsNullOrWhiteSpace(tc)
                                            ? $"MISAFIR{phone.Trim().Replace(" ", "").Replace("+", "").TakeLast(10).Aggregate("", (a, c) => a + c)}"
                                            : tc.Trim(),
                    Address             = string.IsNullOrWhiteSpace(address) ? "Belirtilmedi" : address,
                    DateOfBirth         = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DriverLicenseNumber = string.IsNullOrWhiteSpace(licenseNo)
                                            ? $"GS{phone.Trim().Replace(" ", "").TakeLast(6).Aggregate("", (a, c) => a + c)}"
                                            : licenseNo.Trim(),
                    DriverLicenseExpiry = licenseExpiry == default ? DateTime.UtcNow.AddYears(5) : licenseExpiry.ToUniversalTime()
                };

                var guestResponse = await _httpClient.PostAsJsonAsync("Customer/guest", guestData);
                var guestContent  = await guestResponse.Content.ReadAsStringAsync();

                int customerId;
                if (guestResponse.IsSuccessStatusCode)
                {
                    var created = System.Text.Json.JsonSerializer.Deserialize<CustomerIdResult>(
                        guestContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    customerId = created?.CustomerId ?? 0;
                }
                else if ((int)guestResponse.StatusCode == 409)
                {
                    // Zaten kayıtlı → mevcut ID'yi döndürür
                    var existing = System.Text.Json.JsonSerializer.Deserialize<CustomerIdResult>(
                        guestContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    customerId = existing?.CustomerId ?? 0;
                }
                else
                {
                    return (false, $"Müşteri kaydı oluşturulamadı: {guestContent.Trim().Trim('"')}");
                }

                if (customerId == 0) return (false, "Müşteri ID alınamadı.");

                // 2. Kirala
                return await CreateRentalAsync(customerId, vehicleId, startDate, endDate, depositAmount, notes);
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        private class CustomerIdResult { public int CustomerId { get; set; } }

        
        
        

        
        
        
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

        // ==========================================
        //  KİRALAMA (RENTAL) EK METODLAR
        // ==========================================

        public async Task<List<RentalInfo>> GetRentalsAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<RentalInfo>>("Rental") ?? new List<RentalInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetRentalsAsync Hatası: {ex.Message}");
                return new List<RentalInfo>();
            }
        }

        public async Task<RentalInfo?> GetRentalDetailAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<RentalInfo>($"Rental/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetRentalDetailAsync Hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CompleteRentalAsync(int id, string? notes = null)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { Notes = notes };
                var response = await _httpClient.PutAsJsonAsync($"Rental/{id}/complete", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Kiralama başarıyla tamamlandı.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> CancelRentalAsync(int id, string reason)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { Notes = reason };
                var response = await _httpClient.PutAsJsonAsync($"Rental/{id}/cancel", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Kiralama iptal edildi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  BAKIM (MAINTENANCE)
        // ==========================================

        public async Task<List<MaintenanceInfo>> GetMaintenancesAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<MaintenanceInfo>>("Maintenance") ?? new List<MaintenanceInfo>();
            }
            catch { return new List<MaintenanceInfo>(); }
        }

        public async Task<(bool Success, string Message)> CreateMaintenanceAsync(CreateMaintenanceRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Maintenance", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Bakım kaydı eklendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> UpdateMaintenanceAsync(int id, UpdateMaintenanceRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"Maintenance/{id}", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Bakım kaydı güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> EndMaintenanceAsync(int vehicleId, decimal cost, string notes)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { Cost = cost, Notes = notes };
                var response = await _httpClient.PutAsJsonAsync($"Vehicle/{vehicleId}/maintenance/end", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Araç bakımdan çıkarıldı.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  HASAR KAYDI (DAMAGE RECORD)
        // ==========================================

        public async Task<List<DamageInfo>> GetDamageRecordsAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<DamageInfo>>("DamageRecord") ?? new List<DamageInfo>();
            }
            catch { return new List<DamageInfo>(); }
        }

        public async Task<(bool Success, string Message)> CreateDamageRecordAsync(CreateDamageRecordRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("DamageRecord", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Hasar kaydı eklendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> UpdateDamageRecordAsync(int id, UpdateDamageRecordRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"DamageRecord/{id}", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Hasar kaydı güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  FATURA (INVOICE)
        // ==========================================

        public async Task<List<InvoiceInfo>> GetInvoicesAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("Invoice");
                var body = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] GetInvoicesAsync → {(int)response.StatusCode}: {body[..Math.Min(200, body.Length)]}");
                if (!response.IsSuccessStatusCode) return new List<InvoiceInfo>();
                return System.Text.Json.JsonSerializer.Deserialize<List<InvoiceInfo>>(
                    body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<InvoiceInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetInvoicesAsync HATA: {ex.Message}");
                return new List<InvoiceInfo>();
            }
        }

        public async Task<(bool Success, string Message)> CreateInvoiceAsync(CreateInvoiceRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Invoice", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Fatura oluşturuldu.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"Invoice/{id}", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Fatura güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  BİLDİRİM (NOTIFICATION)
        // ==========================================

        public async Task<List<NotificationInfo>> GetNotificationsAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<NotificationInfo>>("Notification") ?? new List<NotificationInfo>();
            }
            catch { return new List<NotificationInfo>(); }
        }

        public async Task<(bool Success, string Message)> SendNotificationAsync(SendNotificationRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Notification/send", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Bildirim gönderildi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> MarkNotificationReadAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsync($"Notification/{id}/read", null);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Okundu olarak işaretlendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  PERSONEL (STAFF)
        // ==========================================

        public async Task<List<StaffInfo>> GetStaffAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.GetAsync("Staff");
                var body = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[API] GetStaffAsync → {(int)response.StatusCode}: {body[..Math.Min(300, body.Length)]}");
                if (!response.IsSuccessStatusCode) return new List<StaffInfo>();
                return System.Text.Json.JsonSerializer.Deserialize<List<StaffInfo>>(
                    body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<StaffInfo>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetStaffAsync HATA: {ex.Message}");
                return new List<StaffInfo>();
            }
        }

        public async Task<(bool Success, string Message)> UpdateStaffAsync(string id, UpdateStaffRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"Staff/{id}", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Personel bilgileri güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> ToggleStaffActiveAsync(string id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsync($"Staff/{id}/toggle-active", null);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Personel durumu güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> DeleteStaffAsync(string id)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"Staff/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Personel silindi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<(bool Success, string Message)> CreateStaffAsync(CreateStaffRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Auth/admin/create-staff", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Yeni personel hesabı oluşturuldu.");

                if (string.IsNullOrWhiteSpace(content))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        return (false, "Bu işlem için 'Yönetici' yetkiniz bulunmuyor.");
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return (false, "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.");
                    
                    return (false, $"İşlem başarısız (Hata Kodu: {(int)response.StatusCode})");
                }

                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  BİLDİRİM AYARLARI
        // ==========================================

        public async Task<(bool Success, string Message)> UpdateNotificationSettingsAsync(bool maintenance, bool rental, bool availability)
        {
            try
            {
                await SetAuthorizationHeader();
                var data = new { MaintenanceAlerts = maintenance, RentalExpiryAlerts = rental, InstantAvailabilityAlerts = availability };
                var response = await _httpClient.PutAsJsonAsync("Auth/notifications", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Bildirim ayarları güncellendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        // ==========================================
        //  MÜŞTERİ (CUSTOMER) EK METODLAR
        // ==========================================

        public async Task<(bool Success, string Message)> AddCustomerAsync(CreateCustomerRequest request)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("Customer", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode) return (true, "Müşteri eklendi.");
                return (false, content.Trim().Trim('"'));
            }
            catch (Exception ex) { return (false, $"Bağlantı hatası: {ex.Message}"); }
        }

        public async Task<CustomerDetail?> GetCustomerDetailAsync(int id)
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<CustomerDetail>($"Customer/{id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[API] GetCustomerDetailAsync Hatası: {ex.Message}");
                return null;
            }
        }

        // ==========================================
        //  FİYAT POLİTİKASI (PRICE POLICY)
        // ==========================================

        public async Task<List<PricePolicy>> GetPricePoliciesAsync()
        {
            try
            {
                await SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<PricePolicy>>("PricePolicy") ?? new List<PricePolicy>();
            }
            catch { return new List<PricePolicy>(); }
        }

        public async Task<(bool Success, string Message)> SavePricePolicyAsync(PricePolicy policy)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PostAsJsonAsync("PricePolicy", policy);
                if (response.IsSuccessStatusCode) return (true, "Politika başarıyla kaydedildi.");
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(error) ? $"Hata: {response.StatusCode}" : error);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateVehiclePricingAsync(int id, decimal basePrice, double maxDiscount)
        {
            try
            {
                await SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync($"Vehicle/{id}/pricing", new Dictionary<string, object> 
                { 
                    { "basePrice", basePrice }, 
                    { "maxDiscountPercentage", maxDiscount } 
                });
                if (response.IsSuccessStatusCode)
                {
                    // Mobil önbelleği temizle ki bir sonraki veri çekiminde taze gelsin
                    _cachedVehicles = null;
                    _cachedETag = null;
                    return (true, "Fiyatlandırma güncellendi.");
                }
                
                var error = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrEmpty(error) ? $"Hata: {response.StatusCode}" : error);
            }
            catch (Exception ex) { return (false, ex.Message); }
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

    public class PricePolicy
    {
        public int Id { get; set; }
        public string TargetType { get; set; } = "Global";
        public string? TargetValue { get; set; }
        public decimal BasePrice { get; set; }
        public double MaxDiscountPercentage { get; set; }
    }
}