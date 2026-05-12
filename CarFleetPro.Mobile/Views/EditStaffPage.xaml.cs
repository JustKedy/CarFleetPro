using CarFleetPro.Mobile.Services;
using CarFleetPro.Mobile.Models;
using System;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class EditStaffPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly StaffInfo _staff;

    public EditStaffPage(StaffInfo staff)
    {
        InitializeComponent();
        _apiService = new ApiService();
        _staff = staff;

        LoadStaffData();
    }

    private void LoadStaffData()
    {
        AdminNameEntry.Text = _staff.FullName;
        AdminEmailEntry.Text = _staff.Email;
        AdminPhoneEntry.Text = _staff.PhoneNumber;
        AdminRoleSwitch.IsToggled = _staff.Role == "Yönetici";
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnUpdateStaffClicked(object? sender, EventArgs e)
    {
        var name = AdminNameEntry.Text?.Trim();
        var phone = AdminPhoneEntry.Text?.Trim() ?? string.Empty;
        var department = _staff.Department ?? string.Empty; // mevcut değeri koru
        var role = AdminRoleSwitch.IsToggled ? "Yönetici" : "Çalışan";

        if (string.IsNullOrEmpty(name))
        {
            await DisplayAlertAsync("Uyarı", "Ad Soyad alanı zorunludur.", "Tamam");
            return;
        }

        var request = new UpdateStaffRequest
        {
            FullName = name,
            PhoneNumber = phone,
            Department = department,
            Role = role
        };

        var (success, message) = await _apiService.UpdateStaffAsync(_staff.Id, request);
        
        if (success)
        {
            await DisplayAlertAsync("Başarılı ✅", message, "Tamam");
            if (Navigation != null)
                await Navigation.PopAsync();
        }
        else
        {
            await DisplayAlertAsync("Hata ❌", message, "Tamam");
        }
    }
}
