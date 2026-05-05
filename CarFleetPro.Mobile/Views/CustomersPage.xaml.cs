using CarFleetPro.Mobile.Models;
using CarFleetPro.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace CarFleetPro.Mobile.Views;

public partial class CustomersPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<CustomerInfo> _allCustomers = new();

    public CustomersPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        _allCustomers = await _apiService.GetCustomersAsync();
        CustomersList.ItemsSource = _allCustomers;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation != null)
            await Navigation.PopAsync();
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            CustomersList.ItemsSource = _allCustomers;
            return;
        }

        var results = await _apiService.SearchCustomersAsync(query);
        CustomersList.ItemsSource = results;
    }

    private async void OnCustomerSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CustomerInfo customer)
        {
            await DisplayAlertAsync(customer.FullName, 
                $"Telefon: {customer.PhoneNumber}\nToplam Kiralama: {customer.TotalRentals}\nDurum: {customer.RentalStatus}", 
                "Tamam");
        }
        CustomersList.SelectedItem = null;
    }
}
