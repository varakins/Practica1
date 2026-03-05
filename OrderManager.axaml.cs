using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System.Collections.Generic;
using Practica1.Models;

namespace Practica1;

public partial class OrderManager : Window
{
    private ListBox _ordersListBox;
    private List<Order> _orders;

    public OrderManager()
    {
        InitializeComponent();
        
        _ordersListBox = this.FindControl<ListBox>("OrdersListBox");
        
        LoadOrders();
    }

    private void LoadOrders()
    {
        _ordersListBox.ItemTemplate = new FuncDataTemplate<Order>((order, _) =>
        {
            return CreateOrderTemplate(order);
        });
        
        var db = new DatabaseHelper();
        _orders = db.GetOrders();
        
        foreach (var order in _orders)
        {
            order.Items = db.GetOrderItems(order.Id);
        }
        
        _ordersListBox.ItemsSource = _orders;
    }

    private Control CreateOrderTemplate(Order order)
    {
        var border = new Border
        {
            Background = Brush.Parse("#FFFFFF"),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(15),
            BorderBrush = Brush.Parse("#CCCCCC"),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        
        var infoPanel = new StackPanel
        {
            Spacing = 8,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        var codePanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        codePanel.Children.Add(new TextBlock 
        { 
            Text = "Артикул заказа: ", 
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#000000")
        });
        codePanel.Children.Add(new TextBlock 
        { 
            Text = order.articl,
            Foreground = Brush.Parse("#000000"),
            FontFamily = new FontFamily("Courier New")
        });
        infoPanel.Children.Add(codePanel);
        
        var statusPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        statusPanel.Children.Add(new TextBlock 
        { 
            Text = "Статус заказа: ", 
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#000000")
        });
        
        var statusBorder = new Border
        {
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(5, 2)
        };
        
        statusBorder.Child = new TextBlock
        {
            Text = order.StatusName,
            FontSize = 12,
            Foreground = Brush.Parse("#000000")
        };
        statusPanel.Children.Add(statusBorder);
        infoPanel.Children.Add(statusPanel);
        
        var addressPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        addressPanel.Children.Add(new TextBlock 
        { 
            Text = "Адрес пункта выдачи: ", 
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#000000")
        });
        addressPanel.Children.Add(new TextBlock 
        { 
            Text = order.PickUpPointAddress,
            TextWrapping = TextWrapping.Wrap,
            Foreground = Brush.Parse("#000000")
        });
        infoPanel.Children.Add(addressPanel);
        
        var datePanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        datePanel.Children.Add(new TextBlock 
        { 
            Text = "Дата заказа: ", 
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#000000")
        });
        datePanel.Children.Add(new TextBlock 
        { 
            Text = order.OrdersDate.ToString("dd.MM.yyyy"),
            Foreground = Brush.Parse("#000000")
        });
        infoPanel.Children.Add(datePanel);
        
        Grid.SetColumn(infoPanel, 0);
        grid.Children.Add(infoPanel);
        
        var deliveryBorder = new Border
        {
            Background = Brush.Parse("#ffffff"),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(10, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Thickness(20, 15)
        };
        
        var deliveryPanel = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        
        deliveryPanel.Children.Add(new TextBlock
        {
            Text = "Дата доставки",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#000000"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });
        
        deliveryPanel.Children.Add(new TextBlock
        {
            Text = order.DeliveryDate.ToString("dd.MM.yyyy"),
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#000000"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        });
        
        deliveryBorder.Child = deliveryPanel;
        Grid.SetColumn(deliveryBorder, 1);
        grid.Children.Add(deliveryBorder);
        
        border.Child = grid;
        return border;
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var managerMenu = new ManagerMenu();
        managerMenu.Show();
        this.Close();
    }
}