using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;
using Practica1.Models;

namespace Practica1;

public partial class SelectProductDialog : Window
{
    private ListBox _productListBox;
    private TextBox _quantityTextBox;
    
    public Product SelectedProduct { get; private set; }
    public int Quantity { get; private set; } = 1;

    public SelectProductDialog(List<Product> products)
    {
        InitializeComponent();
        
        _productListBox = this.FindControl<ListBox>("ProductListBox");
        _quantityTextBox = this.FindControl<TextBox>("QuantityTextBox");
        
        _productListBox.ItemsSource = products;
        
        if (products.Any())
            _productListBox.SelectedIndex = 0;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        SelectedProduct = _productListBox.SelectedItem as Product;
        if (SelectedProduct != null)
        {
            if (int.TryParse(_quantityTextBox.Text, out int qty) && qty > 0)
            {
                if (qty <= SelectedProduct.Qty)
                {
                    Quantity = qty;
                    Close();
                }
                else
                {
                    ShowMessage("Ошибка", $"Доступно только {SelectedProduct.Qty} шт.");
                }
            }
            else
            {
                ShowMessage("Ошибка", "Введите корректное количество");
            }
        }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ShowMessage(string title, string message)
    {
        var messageBox = new Window
        {
            Title = title,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new TextBlock
            {
                Text = message,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
        await messageBox.ShowDialog(this);
    }
}