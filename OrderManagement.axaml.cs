using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System.Collections.Generic;
using Practica1.Models;
using System;
using Npgsql;

namespace Practica1;

public partial class OrderManagement : Window
{
    private ListBox _ordersListBox;
    private List<Order> _orders;
    private string connectionString = "Host=localhost;Username=postgres;Password=1;Database=Practica1";

    public OrderManagement()
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
            Text = order.Code,
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

    private void OnCreateOrderClick(object sender, RoutedEventArgs e)
    {
        var createOrders = new CreateOrders();
        createOrders.Show();
        this.Close();
    }

    private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
    {
        var selectedOrder = _ordersListBox.SelectedItem as Order;
        
        if (selectedOrder == null)
        {
            var messageBox = new Window
            {
                Title = "Внимание",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "Выберите заказ для удаления",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
            await messageBox.ShowDialog(this);
            return;
        }

        var confirmBox = new Window
        {
            Title = "Подтверждение",
            Width = 350,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 20,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Вы уверены, что хотите удалить заказ:\n{selectedOrder.Code}?",
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                    },
                    new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Spacing = 10,
                        Children =
                        {
                            new Button
                            {
                                Content = "Да",
                                Width = 80,
                                Height = 35,
                                Background = Brush.Parse("#00FA9A"),
                                BorderThickness = new Thickness(0),
                                CornerRadius = new CornerRadius(5)
                            },
                            new Button
                            {
                                Content = "Нет",
                                Width = 80,
                                Height = 35,
                                Background = Brush.Parse("#F44336"),
                                BorderThickness = new Thickness(0),
                                CornerRadius = new CornerRadius(5)
                            }
                        }
                    }
                }
            }
        };
        
        // Получаем ссылки на кнопки
        var buttonPanel = ((StackPanel)confirmBox.Content).Children[1] as StackPanel;
        var yesBtn = buttonPanel.Children[0] as Button;
        var noBtn = buttonPanel.Children[1] as Button;

        yesBtn.Click += async (s, args) =>
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    
                    using (var cmd = new NpgsqlCommand("DELETE FROM orders_datalse WHERE orders_id = @orderId", conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", selectedOrder.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    using (var cmd = new NpgsqlCommand("DELETE FROM orders WHERE id = @orderId", conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", selectedOrder.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                
                _orders.Remove(selectedOrder);
                _ordersListBox.ItemsSource = null;
                _ordersListBox.ItemsSource = _orders;
                
                confirmBox.Close();
                
                var successBox = new Window
                {
                    Title = "Успех",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = "Заказ успешно удален",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }
                };
                await successBox.ShowDialog(this);
            }
            catch (Exception ex)
            {
                confirmBox.Close();
                
                var errorBox = new Window
                {
                    Title = "Ошибка",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Content = new TextBlock
                    {
                        Text = $"Не удалось удалить заказ: {ex.Message}",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    }
                };
                await errorBox.ShowDialog(this);
            }
        };

        noBtn.Click += (s, args) =>
        {
            confirmBox.Close();
        };

        await confirmBox.ShowDialog(this);
    }
}