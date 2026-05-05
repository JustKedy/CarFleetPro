using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using CarFleetPro.Mobile.Views;

namespace CarFleetPro.Mobile.Controls;

public partial class BottomNavBar : ContentView
{
    private bool _isFirstLoad = true;

    // Global deÄŸiÅŸken sayesinde sayfalar arasÄ± geÃ§iÅŸlerde son durumu kaybetmeyiz
    private static string _globalSelectedTab = "Home";

    public static readonly BindableProperty SelectedTabProperty =
        BindableProperty.Create(nameof(SelectedTab), typeof(string), typeof(BottomNavBar), "Home", propertyChanged: OnSelectedTabChanged);

    public static readonly BindableProperty IsAdminProperty =
        BindableProperty.Create(nameof(IsAdmin), typeof(bool), typeof(BottomNavBar), true, propertyChanged: OnRoleChanged);

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

    public BottomNavBar()
    {
        InitializeComponent();
        SelectedTab = _globalSelectedTab; // Ä°lk aÃ§Ä±lÄ±ÅŸta son durumu al
    }

    private static void OnSelectedTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar control)
        {
            _globalSelectedTab = (string)newValue;
            control.UpdateUI();
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
        ListBorder.IsVisible = IsAdmin;
        
        if (!IsAdmin)
        {
            NavGrid.ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(0, GridUnitType.Absolute) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
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

        SlideIndicator(false); // Rol deÄŸiÅŸtiÄŸinde animasyonsuz anÄ±nda oturttur
    }

    private void OnNavGridSizeChanged(object? sender, EventArgs e)
    {
        if (ContainerGrid.Width > 0)
        {
            UpdateUI();
            _isFirstLoad = false;
        }
    }

    private void UpdateUI()
    {
        if (ContainerGrid.Width <= 0) return;

        HomeImg.Source = "homenegatif.svg";
        GarageImg.Source = "garagenegatif.svg";
        ListImg.Source = "listnegatif.svg";
        SettingsImg.Source = "settingsnegatif.svg";

        switch (SelectedTab)
        {
            case "Home":
                HomeImg.Source = "homepozitif.svg";
                break;
            case "Garage":
                GarageImg.Source = "garagepozitif.svg";
                break;
            case "List":
                ListImg.Source = "listpozitif.svg";
                break;
            case "Settings":
                SettingsImg.Source = "settingspozitif.svg";
                break;
        }

        SlideIndicator(!_isFirstLoad);
    }

    private int GetVisualIndex(string tabName)
    {
        if (ListBorder.IsVisible)
        {
            return tabName switch
            {
                "Home" => 0,
                "Garage" => 1,
                "List" => 2,
                "Settings" => 3,
                _ => 0
            };
        }
        else
        {
            return tabName switch
            {
                "Home" => 0,
                "Garage" => 1,
                "Settings" => 2, // List (2) olmadÄ±ÄŸÄ± iÃ§in Settings (2) olur
                _ => 0
            };
        }
    }

    private void SlideIndicator(bool animated)
    {
        if (ContainerGrid.Width <= 0) return;

        int totalVisibleTabs = ListBorder.IsVisible ? 4 : 3;
        if (totalVisibleTabs == 0) return;
        
        // Matematiksel olarak kesin konum hesaplama:
        double tabWidth = ContainerGrid.Width / totalVisibleTabs;
        int visualIndex = GetVisualIndex(SelectedTab);

        double targetCenter = (visualIndex * tabWidth) + (tabWidth / 2);
        double indicatorWidth = SlidingIndicator.WidthRequest; // 50
        double targetX = targetCenter - (indicatorWidth / 2);

        // OlasÄ± Ã§akÄ±ÅŸmalarÄ± ve hatalarÄ± Ã¶nlemek iÃ§in Ã§alÄ±ÅŸan tÃ¼m animasyonlarÄ± temizle
        SlidingIndicator.CancelAnimations();

        if (animated)
        {
            _ = SlidingIndicator.TranslateToAsync(targetX, 0, 300, Easing.CubicInOut);
        }
        else
        {
            SlidingIndicator.TranslationX = targetX;
        }
    }

    private static async Task AnimateIcon(Border border)
    {
        border.CancelAnimations();
        await border.ScaleToAsync(0.8, 100, Easing.CubicOut);
        await border.ScaleToAsync(1.0, 200, Easing.SpringOut);
    }

    private async void NavigateTo(string tabName, Func<Page> pageFactory)
    {
        if (SelectedTab == tabName) return;
        
        SelectedTab = tabName; // Bu, anÄ±nda UpdateUI ve SlideIndicator tetikler! Animasyon TIKLANDIÄI AN baÅŸlar.

        Border targetBorder = tabName switch
        {
            "Home" => HomeBorder,
            "Garage" => GarageBorder,
            "List" => ListBorder,
            "Settings" => SettingsBorder,
            _ => HomeBorder
        };

        _ = AnimateIcon(targetBorder);
        
        // KullanÄ±cÄ± animasyonu hissetsin diye yarÄ±m saniyenin onda biri kadar bekle (tepkisellik)
        await Task.Delay(50);
        
        try
        {
            Page page = pageFactory();
            await Navigation.PushAsync(page, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
        }
    }

    private void OnHomeTapped(object? sender, TappedEventArgs e)
    {
        NavigateTo("Home", () => IPlatformApplication.Current!.Services.GetRequiredService<HomePage>());
    }

    private void OnGarageTapped(object? sender, TappedEventArgs e)
    {
        NavigateTo("Garage", () => IPlatformApplication.Current!.Services.GetRequiredService<GaragePage>());
    }

    private void OnListTapped(object? sender, TappedEventArgs e)
    {
        NavigateTo("List", () => IPlatformApplication.Current!.Services.GetRequiredService<FleetManagementPage>());
    }

    private void OnSettingsTapped(object? sender, TappedEventArgs e)
    {
        NavigateTo("Settings", () => new SettingsPage());
    }
}
