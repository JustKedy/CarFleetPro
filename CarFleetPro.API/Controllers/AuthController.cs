using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using CarFleetPro.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<AppUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

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
                Role = "Agent"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok("Kullanıcı başarıyla oluşturuldu.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Email veya şifre hatalı.");

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Role, user.Role),
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
                user = new { user.FullName, user.Email, user.Role }
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(userEmail)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            return Ok(new 
            {
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                role = user.Role,
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
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
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
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value;
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
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            
            
            if (user == null)
                return Ok(new { message = "Eğer bu e-posta adresi kayıtlıysa, şifre sıfırlama kodu gönderildi." });

            
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            
            var resetCode = new Random().Next(100000, 999999).ToString();

            
            
            user.SecurityStamp = token; 
            await _userManager.UpdateAsync(user);

            
            var emailBody = $@"
                <h2>CarFleetPro — Şifre Sıfırlama</h2>
                <p>Merhaba {user.FullName},</p>
                <p>Şifre sıfırlama talebiniz alındı. Aşağıdaki kodu uygulamaya girip yeni şifrenizi belirleyebilirsiniz:</p>
                <h1 style='color: #3B82F6; text-align: center; font-size: 36px;'>{resetCode}</h1>
                <p>Bu kod 15 dakika geçerlidir.</p>
                <p>Eğer bu talebi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
                <hr>
                <p style='color: #9CA3AF; font-size: 12px;'>CarFleetPro Filo Yönetim Sistemi</p>";

            try
            {
                await _emailService.SendEmailAsync(dto.Email, "CarFleetPro — Şifre Sıfırlama Kodu", emailBody);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EMAIL] Gönderim hatası: {ex.Message}");
                
            }

            return Ok(new { message = "Eğer bu e-posta adresi kayıtlıysa, şifre sıfırlama kodu gönderildi.", token = token });
        }

        
        
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Geçersiz işlem.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }

            return Ok(new { message = "Şifreniz başarıyla sıfırlandı! Yeni şifrenizle giriş yapabilirsiniz." });
        }

        // ─────────────────────────────────────────────────────
        // Madde 2 — Admin: Kullanıcı / Personel Yönetimi
        // ─────────────────────────────────────────────────────

        // GET /api/auth/users — Tüm kullanıcıları listele (sadece Admin)
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var result = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.IsActive,
                u.MaintenanceAlerts,
                u.RentalExpiryAlerts,
                u.InstantAvailabilityAlerts
            });
            return Ok(result);
        }

        // PUT /api/auth/users/{id}/role — Kullanıcı rolünü değiştir (Admin only)
        [HttpPut("users/{id}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.Role = dto.Role;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = $"Kullanıcı rolü '{dto.Role}' olarak güncellendi." });
        }

        // PUT /api/auth/users/{id}/status — Kullanıcı aktif/pasif (Admin only)
        [HttpPut("users/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            user.IsActive = dto.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            var statusText = dto.IsActive ? "aktif" : "pasif";
            return Ok(new { message = $"Kullanıcı hesabı {statusText} yapıldı." });
        }
    }
}