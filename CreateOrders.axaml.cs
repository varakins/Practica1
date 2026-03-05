using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Practica1.Models;

namespace Practica1;

public partial class CreateOrders : Window
{
    private string connectionString = "Host=localhost;Username=postgres;Password=1;Database=Practica1";
    
    private ComboBox _statusComboBox;
    private ComboBox _pickUpPointComboBox;
    private DatePicker _orderDatePicker;
    private DatePicker _deliveryDatePicker;
    private ListBox _productsListBox;
    
    private List<Status> _statuses;
    private List<PickUpPoint> _pickUpPoints;
    private List<ProductViewModel> _availableProducts;

    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public int MaxQuantity { get; set; }
    }

    public CreateOrders()
    {
        InitializeComponent();
        
        _statusComboBox = this.FindControl<ComboBox>("StatusComboBox");
        _pickUpPointComboBox = this.FindControl<ComboBox>("PickUpPointComboBox");
        _orderDatePicker = this.FindControl<DatePicker>("OrderDatePicker");
        _deliveryDatePicker = this.FindControl<DatePicker>("DeliveryDatePicker");
        _productsListBox = this.FindControl<ListBox>("ProductsListBox");
        
        LoadReferenceData();
        LoadProducts();
        SetDefaultDates();
    }

    private void LoadReferenceData()
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            _statuses = new List<Status>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM status ORDER BY id", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _statuses.Add(new Status
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            
            _pickUpPoints = new List<PickUpPoint>();
            using (var cmd = new NpgsqlCommand("SELECT id, mailing_address, city, street, street_number FROM pic_up_poins", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _pickUpPoints.Add(new PickUpPoint
                    {
                        Id = reader.GetInt32(0),
                        MailingAddress = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        City = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        Street = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        StreetNumber = reader.IsDBNull(4) ? "" : reader.GetString(4)
                    });
                }
            }
        }
        
        foreach (var status in _statuses)
        {
            var item = new ComboBoxItem();
            item.Content = status.Name;
            item.Tag = status.Id;
            _statusComboBox.Items.Add(item);
        }
        
        foreach (var point in _pickUpPoints)
        {
            var item = new ComboBoxItem();
            item.Content = point.FullAddress;
            item.Tag = point.Id;
            _pickUpPointComboBox.Items.Add(item);
        }
        
        if (_statusComboBox.Items.Count > 0)
            _statusComboBox.SelectedIndex = 0;
    }

    private void LoadProducts()
    {
        _availableProducts = new List<ProductViewModel>();
        
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            string query = @"
                SELECT p.id, p.articl, t.name as title_name, p.price, p.qty
                FROM products p
                LEFT JOIN title t ON p.title_id = t.id
                WHERE p.qty > 0
                ORDER BY t.name";
            
            using (var cmd = new NpgsqlCommand(query, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _availableProducts.Add(new ProductViewModel
                    {
                        Id = reader.GetInt32(0),
                        Article = reader.GetString(1),
                        Name = reader.IsDBNull(2) ? "Без названия" : reader.GetString(2),
                        Price = reader.GetDecimal(3),
                        MaxQuantity = reader.GetInt32(4),
                        Quantity = 1,
                        IsSelected = false
                    });
                }
            }
        }
        
        RefreshProductsList();
    }

    private void SetDefaultDates()
    {
        _orderDatePicker.SelectedDate = DateTimeOffset.Now;
        _deliveryDatePicker.SelectedDate = DateTimeOffset.Now.AddDays(3);
    }

    private void RefreshProductsList()
    {
        _productsListBox.ItemTemplate = new FuncDataTemplate<ProductViewModel>((item, _) =>
        {
            var border = new Border
            {
                Background = item.IsSelected ? Brush.Parse("#E8F5E9") : Brush.Parse("#F8F8F8"),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // Название
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Артикул
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) }); // Цена
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Количество
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) }); // Выбор
            
            grid.Children.Add(new TextBlock 
            { 
                Text = item.Name,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            });
            Grid.SetColumn(grid.Children[grid.Children.Count - 1], 0);
            
            grid.Children.Add(new TextBlock 
            { 
                Text = item.Article,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = Brush.Parse("#666666")
            });
            Grid.SetColumn(grid.Children[grid.Children.Count - 1], 1);
            
            grid.Children.Add(new TextBlock 
            { 
                Text = $"{item.Price:N2} ₽",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            });
            Grid.SetColumn(grid.Children[grid.Children.Count - 1], 2);
            
            var quantityTextBox = new TextBox
            {
                Text = item.Quantity.ToString(),
                Width = 80,
                Height = 30,
                Padding = new Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = Brush.Parse("#7FFF00"),
                CornerRadius = new CornerRadius(3)
            };
            
            quantityTextBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(quantityTextBox.Text, out int qty))
                {
                    if (qty < 1)
                        item.Quantity = 1;
                    else if (qty > item.MaxQuantity)
                        item.Quantity = item.MaxQuantity;
                    else
                        item.Quantity = qty;
                    
                    quantityTextBox.Text = item.Quantity.ToString();
                }
                else
                {
                    item.Quantity = 1;
                    quantityTextBox.Text = "1";
                }
            };
            
            grid.Children.Add(quantityTextBox);
            Grid.SetColumn(grid.Children[grid.Children.Count - 1], 3);
            
            // Выбор (CheckBox)
            var checkBox = new CheckBox
            {
                IsChecked = item.IsSelected,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            checkBox.Click += (s, e) =>
            {
                item.IsSelected = checkBox.IsChecked ?? false;
                RefreshProductsList();
            };
            
            grid.Children.Add(checkBox);
            Grid.SetColumn(grid.Children[grid.Children.Count - 1], 4);
            
            border.Child = grid;
            return border;
        });
        
        _productsListBox.ItemsSource = null;
        _productsListBox.ItemsSource = _availableProducts;
    }

    private int GenerateOrderCode()
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            string query = "SELECT COALESCE(MAX(CAST(code AS INTEGER)), 0) FROM orders WHERE code ~ '^[0-9]+$'";
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar()) + 1;
            }
        }
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var orderManagement = new OrderManagement();
        orderManagement.Show();
        this.Close();
    }

    private async void OnCreateButtonClick(object sender, RoutedEventArgs e)
    {
        if (_statusComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите статус заказа");
            return;
        }
        
        if (_pickUpPointComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите адрес пункта выдачи");
            return;
        }
        
        if (_orderDatePicker.SelectedDate == null)
        {
            await ShowMessage("Ошибка", "Выберите дату заказа");
            return;
        }
        
        if (_deliveryDatePicker.SelectedDate == null)
        {
            await ShowMessage("Ошибка", "Выберите дату выдачи");
            return;
        }
        
        var selectedProducts = _availableProducts.Where(p => p.IsSelected).ToList();
        if (selectedProducts.Count == 0)
        {
            await ShowMessage("Ошибка", "Выберите хотя бы один товар");
            return;
        }
        
        foreach (var product in selectedProducts)
        {
            if (product.Quantity > product.MaxQuantity)
            {
                await ShowMessage("Ошибка", $"Товара '{product.Name}' доступно только {product.MaxQuantity} шт.");
                return;
            }
        }
        
        try
        {
            int statusId = (int)((ComboBoxItem)_statusComboBox.SelectedItem).Tag;
            int pointId = (int)((ComboBoxItem)_pickUpPointComboBox.SelectedItem).Tag;
            int orderCode = GenerateOrderCode();
            
            int usersId = 1;
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                await conn.OpenAsync();
                
                string orderQuery = @"
                    INSERT INTO orders 
                    (orders_date, delivery_date, pic_up_poins_id, users_id, code, status_id)
                    VALUES 
                    (@orders_date, @delivery_date, @pic_up_poins_id, @users_id, @code, @status_id)
                    RETURNING id";
                
                int orderId;
                using (var cmd = new NpgsqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orders_date", _orderDatePicker.SelectedDate.Value.DateTime);
                    cmd.Parameters.AddWithValue("@delivery_date", _deliveryDatePicker.SelectedDate.Value.DateTime);
                    cmd.Parameters.AddWithValue("@pic_up_poins_id", pointId);
                    cmd.Parameters.AddWithValue("@users_id", usersId);
                    cmd.Parameters.AddWithValue("@code", orderCode.ToString());
                    cmd.Parameters.AddWithValue("@status_id", statusId);
                    
                    orderId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                
                foreach (var product in selectedProducts)
                {
                    string itemQuery = @"
                        INSERT INTO orders_datalse 
                        (orders_id, products_id, qty)
                        VALUES 
                        (@orders_id, @products_id, @qty)";
                    
                    using (var cmd = new NpgsqlCommand(itemQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@orders_id", orderId);
                        cmd.Parameters.AddWithValue("@products_id", product.Id);
                        cmd.Parameters.AddWithValue("@qty", product.Quantity);
                        
                        await cmd.ExecuteNonQueryAsync();
                    }
                    
                    string updateQtyQuery = "UPDATE products SET qty = qty - @qty WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(updateQtyQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@qty", product.Quantity);
                        cmd.Parameters.AddWithValue("@id", product.Id);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            
            await ShowMessage("Успех", $"Заказ №{orderCode} успешно создан");
            
            var orderManagement = new OrderManagement();
            orderManagement.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            await ShowMessage("Ошибка", $"Не удалось создать заказ: {ex.Message}");
        }
    }

    private async System.Threading.Tasks.Task ShowMessage(string title, string message)
    {
        var messageBox = new Window
        {
            Title = title,
            Width = 300,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new TextBlock
            {
                Text = message,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            }
        };
        await messageBox.ShowDialog(this);
    }
}