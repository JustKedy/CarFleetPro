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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
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
    }
}