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
        PhotoCarousel.IsVisible = _images.Count > 0;
        PhotoCarousel.ItemsSource = null;
        PhotoCarousel.ItemsSource = _images;

        // Seçim sıfırla
        _selectedImage = null;
        ActionBar.IsVisible = false;
        AddPhotoBtn.IsVisible = _isAdmin && _images.Count < 10;
    }

    // ─── Fotoğraf Seçildi (Carousel) ──────────────────────────────────────
    private void OnPhotoSelected(object? sender, CurrentItemChangedEventArgs e)
    {
        _selectedImage = e.CurrentItem as VehicleImageInfo;

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
            await DisplayAlertAsync("Bilgi", "Bu fotoğraf zaten kapak fotoğrafı.", "Tamam");
            return;
        }

        var (success, message) = await _apiService.SetPrimaryImageAsync(_selectedImage.VehicleImageId);

        if (success)
        {
            // Tüm fotoğrafların IsPrimary'sini güncelle
            foreach (var img in _images) img.IsPrimary = false;
            _selectedImage.IsPrimary = true;
            RefreshGrid();
            await DisplayAlertAsync("✅", "Kapak fotoğrafı güncellendi.", "Tamam");
        }
        else
        {
            await DisplayAlertAsync("Hata ❌", message, "Tamam");
        }
    }

    // ─── Sil ──────────────────────────────────────────────────────────────
    private async void OnDeletePhotoClicked(object? sender, EventArgs e)
    {
        if (_selectedImage == null) return;

        var confirm = await DisplayAlertAsync(
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
            await DisplayAlertAsync("Hata ❌", message, "Tamam");
        }
    }

    // ─── Seçimi İptal Et ──────────────────────────────────────────────────
    private void OnCancelSelectionClicked(object? sender, EventArgs e)
    {
        ActionBar.IsVisible = false;
    }

    // ─── Fotoğraf Ekle ────────────────────────────────────────────────────
    private async void OnAddPhotoClicked(object? sender, EventArgs e)
    {
        if (!_isAdmin || _images.Count >= 10) return;

        var action = await DisplayActionSheetAsync(
            "Fotoğraf Ekle",
            "İptal",
            null,
            "Galeriden Seç",
            "Kamerayı Aç");

        List<FileResult> photosToUpload = new();

        try
        {
            if (action == "Galeriden Seç")
            {
                var results = await FilePicker.Default.PickMultipleAsync(new PickOptions
                {
                    PickerTitle = "Fotoğrafları Seçin (Maksimum 10)",
                    FileTypes = FilePickerFileType.Images
                });
                
                if (results != null && results.Any())
                {
                    int eklenebilecek = Math.Max(0, 10 - _images.Count);
                    var secilenler = results.Where(r => r != null).ToList();
                    
                    if (secilenler.Count > eklenebilecek)
                    {
                        await DisplayAlertAsync("Limit Aşıldı", $"En fazla 10 fotoğraf eklenebilir. Sadece seçtiğiniz ilk {eklenebilecek} fotoğraf yüklenecek.", "Tamam");
                        photosToUpload.AddRange(secilenler.Take(eklenebilecek).Cast<FileResult>());
                    }
                    else
                    {
                        photosToUpload.AddRange(secilenler.Cast<FileResult>());
                    }
                }
            }
            else if (action == "Kamerayı Aç" && MediaPicker.Default.IsCaptureSupported)
            {
                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    photosToUpload.Add(photo);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Hata", $"Fotoğraf seçilemedi: {ex.Message}", "Tamam");
            return;
        }

        if (photosToUpload.Count == 0) return;

        AddPhotoBtn.IsEnabled = false;

        int totalCount = photosToUpload.Count;
        int uploadedCount = 0;
        int failedCount = 0;

        foreach (var photo in photosToUpload)
        {
            uploadedCount++;
            AddPhotoBtn.Text = $"Yükleniyor ({uploadedCount}/{totalCount})...";

            try
            {
                var (success, message, newImage) = await _apiService.UploadVehicleImageAsync(
                    _vehicle.Id,
                    photo.FullPath,
                    photo.FileName,
                    photo.ContentType ?? "image/jpeg");

                if (success && newImage != null)
                {
                    _images.Add(newImage);
                }
                else
                {
                    failedCount++;
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                System.Diagnostics.Debug.WriteLine($"[Gallery] Resim yükleme hatası: {ex.Message}");
            }
        }

        AddPhotoBtn.IsEnabled = true;
        AddPhotoBtn.Text = "+ Ekle";

        RefreshGrid();

        if (failedCount > 0)
        {
            await DisplayAlertAsync("Yükleme Tamamlandı", $"{totalCount} resimden {totalCount - failedCount} tanesi başarıyla yüklendi, {failedCount} tanesi başarısız oldu.", "Tamam");
        }
        else
        {
            await DisplayAlertAsync("Başarılı ✅", $"{totalCount} resim başarıyla yüklendi.", "Tamam");
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation is not null) await Navigation.PopAsync();
    }
}
