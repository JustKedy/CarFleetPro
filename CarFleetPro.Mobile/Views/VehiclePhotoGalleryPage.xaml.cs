using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarFleetPro.Mobile.Views;

public partial class VehiclePhotoGalleryPage : ContentPage
{
    private readonly Vehicle _vehicle;
    private readonly bool _isAdmin;
    private readonly ApiService _apiService;

    private List<VehicleImageInfo> _images;
    private VehicleImageInfo? _selectedImage;

    public VehiclePhotoGalleryPage(Vehicle vehicle, List<VehicleImageInfo> images, bool isAdmin, ApiService apiService)
    {
        InitializeComponent();
        _vehicle = vehicle;
        _isAdmin = isAdmin;
        _apiService = apiService;
        _images = new List<VehicleImageInfo>(images);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        TitleLabel.Text = $"{_vehicle.Marka} {_vehicle.Model}";
        AddPhotoBtn.IsVisible = _isAdmin && _images.Count < 10;

        RefreshGrid();
    }

    private void RefreshGrid()
    {
        CountLabel.Text = $"{_images.Count} fotoğraf";
        EmptyState.IsVisible = _images.Count == 0;
        PhotoGrid.IsVisible = _images.Count > 0;
        PhotoGrid.ItemsSource = null;
        PhotoGrid.ItemsSource = _images;

        // Seçim sıfırla
        _selectedImage = null;
        ActionBar.IsVisible = false;
        AddPhotoBtn.IsVisible = _isAdmin && _images.Count < 10;
    }

    // ─── Fotoğraf Seçildi ─────────────────────────────────────────────────
    private void OnPhotoSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedImage = e.CurrentSelection.FirstOrDefault() as VehicleImageInfo;

        if (_selectedImage == null || !_isAdmin)
        {
            ActionBar.IsVisible = false;
            return;
        }

        // Admin ise aksiyon barını göster
        ActionBar.IsVisible = true;
    }

    // ─── Kapak Yap ────────────────────────────────────────────────────────
    private async void OnSetPrimaryClicked(object? sender, EventArgs e)
    {
        if (_selectedImage == null) return;
        if (_selectedImage.IsPrimary)
        {
            await DisplayAlert("Bilgi", "Bu fotoğraf zaten kapak fotoğrafı.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.SetPrimaryImageAsync(_selectedImage.VehicleImageId);

        if (success)
        {
            // Tüm fotoğrafların IsPrimary'sini güncelle
            foreach (var img in _images) img.IsPrimary = false;
            _selectedImage.IsPrimary = true;
            RefreshGrid();
            await DisplayAlert("✅", "Kapak fotoğrafı güncellendi.", "Tamam");
        }
        else
        {
            await DisplayAlert("Hata ❌", message, "Tamam");
        }
    }

    // ─── Sil ──────────────────────────────────────────────────────────────
    private async void OnDeletePhotoClicked(object? sender, EventArgs e)
    {
        if (_selectedImage == null) return;

        var confirm = await DisplayAlert(
            "Fotoğrafı Sil",
            "Bu fotoğrafı kalıcı olarak silmek istediğinize emin misiniz?",
            "Evet, Sil", "İptal");

        if (!confirm) return;

        var (success, message) = await _apiService.DeleteVehicleImageAsync(_selectedImage.VehicleImageId);

        if (success)
        {
            _images.Remove(_selectedImage);
            RefreshGrid();
        }
        else
        {
            await DisplayAlert("Hata ❌", message, "Tamam");
        }
    }

    // ─── Seçimi İptal Et ──────────────────────────────────────────────────
    private void OnCancelSelectionClicked(object? sender, EventArgs e)
    {
        PhotoGrid.SelectedItem = null;
        _selectedImage = null;
        ActionBar.IsVisible = false;
    }

    // ─── Fotoğraf Ekle ────────────────────────────────────────────────────
    private async void OnAddPhotoClicked(object? sender, EventArgs e)
    {
        if (!_isAdmin || _images.Count >= 10) return;

        var action = await DisplayActionSheet(
            "Fotoğraf Ekle",
            "İptal",
            null,
            "Galeriden Seç",
            "Kamerayı Aç");

        FileResult? photo = null;

        try
        {
            if (action == "Galeriden Seç")
                photo = await MediaPicker.Default.PickPhotoAsync();
            else if (action == "Kamerayı Aç" && MediaPicker.Default.IsCaptureSupported)
                photo = await MediaPicker.Default.CapturePhotoAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", $"Fotoğraf seçilemedi: {ex.Message}", "Tamam");
            return;
        }

        if (photo == null) return;

        AddPhotoBtn.IsEnabled = false;
        AddPhotoBtn.Text = "⬆️ Yükleniyor...";

        var (success, message, newImage) = await _apiService.UploadVehicleImageAsync(
            _vehicle.Id,
            photo.FullPath,
            photo.FileName,
            photo.ContentType ?? "image/jpeg");

        AddPhotoBtn.IsEnabled = true;
        AddPhotoBtn.Text = "+ Ekle";

        if (success && newImage != null)
        {
            _images.Add(newImage);
            RefreshGrid();
        }
        else
        {
            await DisplayAlert("Hata ❌", message, "Tamam");
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null) await Navigation.PopAsync();
    }
}
