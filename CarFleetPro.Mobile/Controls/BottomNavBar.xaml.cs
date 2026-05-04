using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Views;

namespace CarFleetPro.Mobile.Controls;

public partial class BottomNavBar : ContentView
{
    
    private static int _lastTabIndex = 0;

    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(BottomNavBar), "Home", propertyChanged: OnSelectedTabChanged);

    public static readonly BindableProperty IsAdminProperty =
        BindableProperty.Create(nameof(IsAdmin), typeof(bool), typeof(BottomNavBar), false, propertyChanged: OnRoleChanged);

    public bool IsAdmin
    {
        get => (bool)GetValue(IsAdminProperty);
        set => SetValue(IsAdminProperty, value);
    }

    public string SelectedTab
    {
        get => (string)GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    private int _currentTabIndex;

    public BottomNavBar()
    {
        InitializeComponent();
    }

    private static void OnSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar control)
        {
            control.UpdateUI((string)newValue);
        }
    }

    private static void OnRoleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar control)
        {
            control.ApplyRoleVisibility();
        }
    }

    private void ApplyRoleVisibility()
    {
        // Ayarlar sekmesi sadece Adminlere özeldir
        SettingsBorder.IsVisible = IsAdmin;
        
        // Eğer Settings gizlenirse grid oranları değişmeli
        if (!IsAdmin)
        {
            NavGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0, GridUnitType.Absolute) } // Sonuncu grid kapanır
            };
        }
        else
        {
            NavGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            };
        }
    }

    // Navigasyon bittiğinde veya boyut değiştiğinde tetiklenir
    private void OnNavGridSizeChanged(object? sender, EventArgs e)
    {
        if (NavGrid.Width > 0)
        {
            UpdateUI(SelectedTab);
        }
    }

    private void UpdateUI(string activeTab)
    {
        if (NavGrid.Width <= 0) return;

        HomeImg.Source = "homenegatif.svg";
        GarageImg.Source = "garagenegatif.svg";
        ListImg.Source = "listnegatif.svg";
        SettingsImg.Source = "settingsnegatif.svg";

        switch (activeTab)
        {
            case "Home":
                HomeImg.Source = "homepozitif.svg";
                _currentTabIndex = 0;
                break;
            case "Garage":
                GarageImg.Source = "garagepozitif.svg";
                _currentTabIndex = 1;
                break;
            case "List":
                ListImg.Source = "listpozitif.svg";
                _currentTabIndex = 2;
                break;
            case "Settings":
                SettingsImg.Source = "settingspozitif.svg";
                _currentTabIndex = 3;
                break;
        }

        SlideIndicator();
    }

    
    private async void SlideIndicator()
    {
        int totalVisibleTabs = IsAdmin ? 4 : 3;
        if (totalVisibleTabs == 0) return;
        
        double tabWidth = NavGrid.Width / totalVisibleTabs;

        // 1. Önce çerçeveyi hafızadaki 'eski' yerine ışınla (kullanıcı görmez)
        double startX = GetTabXPosition(_lastTabIndex, tabWidth);
        SlidingIndicator.TranslationX = startX;

        // 2. Şimdi yeni hedefe doğru yağ gibi sürükle
        double targetX = GetTabXPosition(_currentTabIndex, tabWidth);

        
        if (Math.Abs(startX - targetX) < 1) return;

        
        await SlidingIndicator.TranslateToAsync(targetX, 0, 500, Easing.CubicInOut);

        
        _lastTabIndex = _currentTabIndex;
    }

    private double GetTabXPosition(int index, double tabWidth)
    {
        // Eğer personel ise ve settings sekmesi (index 3) yoksa tabWidth farklı hesaplanmış olacak,
        // geriye kalanlar normal sırasında devam ediyor. 
        return index * tabWidth;
    }

    private static async Task AnimateIcon(Border border)
    {
        await border.ScaleToAsync(0.8, 100, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 200, Easing.SpringOut);
    }

    private async void OnHomeTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "Home") return;
        _ = AnimateIcon(HomeBorder);
        await Task.Delay(50);
        var page = IPlatformApplication.Current!.Services.GetRequiredService<HomePage>();
        await Navigation.PushAsync(page, false);
    }

    private async void OnGarageTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "Garage") return;
        _ = AnimateIcon(GarageBorder);
        await Task.Delay(50);
        var page = IPlatformApplication.Current!.Services.GetRequiredService<GaragePage>();
        await Navigation.PushAsync(page, false);
    }

    private async void OnListTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "List") return;
        _ = AnimateIcon(ListBorder);
        await Task.Delay(50);
        var page = IPlatformApplication.Current!.Services.GetRequiredService<FleetManagementPage>();
        await Navigation.PushAsync(page, false);
    }

    private async void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        if (SelectedTab == "Settings") return;
        _ = AnimateIcon(SettingsBorder);
        await Task.Delay(50);
        await Navigation.PushAsync(new SettingsPage(), false);
    }
}