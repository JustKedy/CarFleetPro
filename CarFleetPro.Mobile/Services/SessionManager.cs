using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace CarFleetPro.Mobile.Services
{
    public static class SessionManager
    {
        public static async Task SaveSessionAsync(string token, string role, string fullName)
        {
            await SecureStorage.Default.SetAsync("jwt_token", token ?? string.Empty);
            await SecureStorage.Default.SetAsync("user_role", role ?? string.Empty);
            await SecureStorage.Default.SetAsync("user_fullname", fullName ?? string.Empty);
        }

        public static async Task<bool> IsAdminAsync()
        {
            var role = await SecureStorage.Default.GetAsync("user_role");
            return role == "Admin";
        }

        public static async Task<string> GetFullNameAsync()
        {
            var name = await SecureStorage.Default.GetAsync("user_fullname");
            return name ?? "Kullanıcı";
        }

        public static async Task<string> GetRoleAsync()
        {
            var role = await SecureStorage.Default.GetAsync("user_role");
            return role ?? "Personel";
        }

        public static void ClearSession()
        {
            SecureStorage.Default.RemoveAll();
        }
    }
}
