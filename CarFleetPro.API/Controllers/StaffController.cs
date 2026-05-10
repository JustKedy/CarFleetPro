using CarFleetPro.API.DTOs;
using CarFleetPro.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarFleetPro.API.Controllers
{
    /// <summary>
    /// Çalışan (personel) yönetimi — yalnızca Yönetici erişebilir.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Yönetici")]
    public class StaffController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public StaffController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>GET /api/staff — Tüm personeli listele</summary>
        [HttpGet]
        public IActionResult GetAllStaff([FromQuery] bool? activeOnly)
        {
            var query = _userManager.Users.AsQueryable();

            if (activeOnly == true)
                query = query.Where(u => u.IsActive);

            var staff = query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new StaffListDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    Department = u.Department,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            return Ok(staff);
        }

        /// <summary>GET /api/staff/{id} — Personel detayı</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Personel bulunamadı.");

            return Ok(new StaffListDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                Department = user.Department,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>PUT /api/staff/{id} — Personel bilgilerini güncelle</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(string id, [FromBody] UpdateStaffDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Personel bulunamadı.");

            var allowedRoles = new[] { "Yönetici", "Çalışan" };
            if (!allowedRoles.Contains(dto.Role))
                return BadRequest("Geçersiz rol. 'Yönetici' veya 'Çalışan' olmalı.");

            // Eski rolü kaldır, yeni rolü ekle
            if (user.Role != dto.Role)
            {
                await _userManager.RemoveFromRoleAsync(user, user.Role);
                await _userManager.AddToRoleAsync(user, dto.Role);
                user.Role = dto.Role;
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.Department = dto.Department;
            user.IsActive = dto.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = "Personel bilgileri güncellendi." });
        }

        /// <summary>
        /// PUT /api/staff/{id}/toggle-active — Personeli aktif/pasif yap (soft delete)
        /// </summary>
        [HttpPut("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Personel bulunamadı.");

            // Kendini pasif yapmasını engelle
            var requesterId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (user.Id == requesterId)
                return BadRequest("Kendi hesabınızı pasif yapamazsınız.");

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            var durum = user.IsActive ? "aktif" : "pasif";
            return Ok(new { message = $"{user.FullName} hesabı {durum} yapıldı.", isActive = user.IsActive });
        }

        /// <summary>DELETE /api/staff/{id} — Personeli kalıcı sil (dikkatli kullanın)</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Personel bulunamadı.");

            // Kendini silmesini engelle
            var requesterId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (user.Id == requesterId)
                return BadRequest("Kendi hesabınızı silemezsiniz.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));

            return Ok(new { message = $"{user.FullName} hesabı silindi." });
        }
    }
}
