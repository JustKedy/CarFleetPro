using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using CarFleetPro.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CarFleetPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailService emailService,
            IMemoryCache cache,
            ILogger<AuthController> logger)
        {
            _userManager   = userManager;
            _roleManager   = roleManager;
            _configuration = configuration;
            _emailService  = emailService;
            _cache         = cache;
            _logger        = logger;
        }

        /// <summary>
        /// Herkese açık kayıt — sadece "Çalışan" rolüyle hesap oluşturur.
        /// Yönetici hesabı için /admin/create-staff kullanın.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("Bu email adresi zaten kullanımda.");

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = "Çalışan",
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            // Identity role sistemine de ekle
            await _userManager.AddToRoleAsync(user, "Çalışan");

            return Ok(new { message = "Hesap başarıyla oluşturuldu." });
        }

        /// <summary>
        /// Sadece Yönetici tarafından çağrılabilir. Yeni çalışan veya yönetici hesabı oluşturur.
        /// </summary>
        [HttpPost("admin/create-staff")]
        [Authorize(Roles = "Yönetici")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffDto dto)
        {
            // Rol kontrolü
            var allowedRoles = new[] { "Yönetici", "Çalışan" };
            if (!allowedRoles.Contains(dto.Role))
                return BadRequest("Geçersiz rol. 'Yönetici' veya 'Çalışan' olmalı.");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Bu email adresi zaten kullanımda.");

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Department = dto.Department,
                Role = dto.Role,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new { message = $"{dto.Role} hesabı başarıyla oluşturuldu.", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Email veya şifre hatalı.");

            if (!user.IsActive)
                return Unauthorized("Hesabınız devre dışı bırakılmıştır. Yöneticinizle iletişime geçin.");

            // Identity'den gerçek rolleri al
            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? user.Role;

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Role, primaryRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = new
                {
                    user.FullName,
                    user.Email,
                    role = primaryRole,
                    user.Department,
                    user.IsActive
                }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(new
            {
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                role = user.Role,
                department = user.Department,
                maintenanceAlerts = user.MaintenanceAlerts,
                rentalExpiryAlerts = user.RentalExpiryAlerts,
                instantAvailabilityAlerts = user.InstantAvailabilityAlerts
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExists != null) return BadRequest("Bu e-posta adresi zaten kullanılıyor.");

                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpper();
                user.NormalizedUserName = dto.Email.ToUpper();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = "Profil başarıyla güncellendi!" });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(errorMsg);
            }

            return Ok(new { message = "Şifreniz başarıyla güncellendi!" });
        }

        [HttpPut("notifications")]
        [Authorize]
        public async Task<IActionResult> UpdateNotificationSettings([FromBody] UpdateNotificationsDto dto)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.MaintenanceAlerts = dto.MaintenanceAlerts;
            user.RentalExpiryAlerts = dto.RentalExpiryAlerts;
            user.InstantAvailabilityAlerts = dto.InstantAvailabilityAlerts;

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Bildirim tercihleri kaydedildi." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // Kullanıcı yoksa bile aynı mesajı dön (enum güvenliği)
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(new { message = "Eğer bu e-posta kayıtlıysa, doğrulama kodu gönderildi." });

            // 6 haneli OTP
            var otp   = new Random().Next(100000, 999999).ToString();
            // Identity password-reset token (reset-password adımında kullanılacak)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // OTP ve token'ı 15 dakika cache'e al
            var cacheKey = $"otp:{dto.Email.ToLowerInvariant()}";
            _cache.Set(cacheKey, (Otp: otp, Token: token),
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15) });

            var emailBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif; background:#f3f4f6; margin:0; padding:20px;'>
  <div style='max-width:480px; margin:0 auto; background:#ffffff; border-radius:16px; padding:32px; box-shadow:0 2px 12px rgba(0,0,0,.08);'>
    <h2 style='color:#0A1128; margin-bottom:4px;'>CarFleet<span style='color:#3B82F6;'>Pro</span></h2>
    <p style='color:#6B7280; font-size:13px; margin-top:0;'>Araç Kiralama & Filo Yönetimi</p>
    <hr style='border:none; border-top:1px solid #E5E7EB; margin:20px 0;'/>
    <p style='color:#1F2937;'>Merhaba <strong>{user.FullName}</strong>,</p>
    <p style='color:#4B5563;'>Şifre sıfırlama talebiniz alındı. Aşağıdaki 6 haneli kodu uygulamaya girerek şifrenizi sıfırlayabilirsiniz.</p>
    <div style='background:#EFF6FF; border:2px solid #3B82F6; border-radius:12px; padding:24px; text-align:center; margin:24px 0;'>
      <span style='font-size:42px; font-weight:900; letter-spacing:10px; color:#1D4ED8;'>{otp}</span>
    </div>
    <p style='color:#9CA3AF; font-size:13px;'>⏱️ Bu kod <strong>15 dakika</strong> geçerlidir.</p>
    <p style='color:#9CA3AF; font-size:13px;'>Eğer bu isteği siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
    <hr style='border:none; border-top:1px solid #E5E7EB; margin:20px 0;'/>
    <p style='color:#D1D5DB; font-size:11px; text-align:center;'>CarFleetPro Filo Yönetim Sistemi</p>
  </div>
</body>
</html>";

            try
            {
                await _emailService.SendEmailAsync(dto.Email, "CarFleetPro — Şifre Sıfırlama Kodu", emailBody);
                _logger.LogInformation("[ForgotPassword] OTP gönderildi: {Email}", dto.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ForgotPassword] E-posta gönderilemedi: {Email}", dto.Email);
                return StatusCode(500, "E-posta gönderilemedi. Lütfen daha sonra tekrar deneyin.");
            }

            return Ok(new { message = "Doğrulama kodu e-posta adresinize gönderildi." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var cacheKey = $"otp:{dto.Email.ToLowerInvariant()}";

            if (!_cache.TryGetValue(cacheKey, out (string Otp, string Token) cached))
                return BadRequest("Doğrulama kodunun süresi dolmuş veya geçersiz. Lütfen yeniden kod isteyin.");

            if (cached.Otp != dto.Token)
                return BadRequest("Girdiğiniz kod yanlış. Lütfen e-postanızı kontrol edin.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Geçersiz işlem.");

            var result = await _userManager.ResetPasswordAsync(user, cached.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            // Kullanılan OTP'yi temizle
            _cache.Remove(cacheKey);

            return Ok(new { message = "Şifreniz başarıyla sıfırlandı! Yeni şifrenizle giriş yapabilirsiniz." });
        }
    }
}