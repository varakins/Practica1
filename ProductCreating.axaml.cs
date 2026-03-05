using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Npgsql;
using Practica1.Models;

namespace Practica1;

public partial class ProductCreating : Window
{
    private string connectionString = "Host=localhost;Username=postgres;Password=1;Database=Practica1";
    
    private Border _imageBorder;
    private Image _productImage;
    private TextBlock _imageFileNameText;
    
    private TextBox _articlTextBox;
    private ComboBox _titleComboBox;
    private ComboBox _categoryComboBox;
    private TextBox _descriptionTextBox;
    private ComboBox _manufacturerComboBox;
    private ComboBox _supplierComboBox;
    private TextBox _priceTextBox;
    private TextBox _unitTextBox;
    private TextBox _quantityTextBox;
    private TextBox _discountTextBox;
    
    private string _selectedImagePath;
    private string _selectedImageFileName;
    
    private List<Category> _categories;
    private List<Title> _titles;
    private List<Manufacturer> _manufacturers;
    private List<Supplier> _suppliers;

    public ProductCreating()
    {
        InitializeComponent();
        
        _imageBorder = this.FindControl<Border>("ImageBorder");
        _productImage = this.FindControl<Image>("ProductImage");
        _imageFileNameText = this.FindControl<TextBlock>("ImageFileNameText");
        
        _articlTextBox = this.FindControl<TextBox>("ArticlTextBox");
        _titleComboBox = this.FindControl<ComboBox>("TitleComboBox");
        _categoryComboBox = this.FindControl<ComboBox>("CategoryComboBox");
        _descriptionTextBox = this.FindControl<TextBox>("DescriptionTextBox");
        _manufacturerComboBox = this.FindControl<ComboBox>("ManufacturerComboBox");
        _supplierComboBox = this.FindControl<ComboBox>("SupplierComboBox");
        _priceTextBox = this.FindControl<TextBox>("PriceTextBox");
        _unitTextBox = this.FindControl<TextBox>("UnitTextBox");
        _quantityTextBox = this.FindControl<TextBox>("QuantityTextBox");
        _discountTextBox = this.FindControl<TextBox>("DiscountTextBox");
        
        LoadReferenceData();
    }

    private void LoadReferenceData()
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            
            _categories = new List<Category>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM categories_products ORDER BY name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _categories.Add(new Category
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            
            _titles = new List<Title>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM title ORDER BY name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _titles.Add(new Title
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            
            _manufacturers = new List<Manufacturer>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM manufactures ORDER BY name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _manufacturers.Add(new Manufacturer
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
            
            _suppliers = new List<Supplier>();
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM suplitiers ORDER BY name", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    _suppliers.Add(new Supplier
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }
            }
        }
        
        foreach (var category in _categories)
        {
            var item = new ComboBoxItem();
            item.Content = category.Name;
            item.Tag = category.Id;
            _categoryComboBox.Items.Add(item);
        }
        
        foreach (var title in _titles)
        {
            var item = new ComboBoxItem();
            item.Content = title.Name;
            item.Tag = title.Id;
            _titleComboBox.Items.Add(item);
        }
        
        foreach (var manufacturer in _manufacturers)
        {
            var item = new ComboBoxItem();
            item.Content = manufacturer.Name;
            item.Tag = manufacturer.Id;
            _manufacturerComboBox.Items.Add(item);
        }
        
        foreach (var supplier in _suppliers)
        {
            var item = new ComboBoxItem();
            item.Content = supplier.Name;
            item.Tag = supplier.Id;
            _supplierComboBox.Items.Add(item);
        }
    }

    private async void OnSelectImageClick(object sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображение",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" }
                }
            }
        });

        if (files.Count >= 1)
        {
            var file = files[0];
            _selectedImagePath = file.Path.LocalPath;
            _selectedImageFileName = System.IO.Path.GetFileName(_selectedImagePath);
            
            try
            {
                var bitmap = new Bitmap(_selectedImagePath);
                _productImage.Source = bitmap;
                _imageFileNameText.Text = _selectedImageFileName;
                _imageFileNameText.Foreground = Brush.Parse("#00FA9A");
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось загрузить изображение: {ex.Message}");
            }
        }
    }

    private void OnPriceTextChanged(object sender, TextChangedEventArgs e)
    {
        if (decimal.TryParse(_priceTextBox.Text, out decimal price) && price < 0)
        {
            _priceTextBox.Text = "0";
            _priceTextBox.SelectionStart = _priceTextBox.Text.Length;
        }
    }

    private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(_quantityTextBox.Text, out int qty) && qty < 0)
        {
            _quantityTextBox.Text = "0";
            _quantityTextBox.SelectionStart = _quantityTextBox.Text.Length;
        }
    }

    private async void OnSaveButtonClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_articlTextBox.Text))
        {
            await ShowMessage("Ошибка", "Введите артикул");
            return;
        }
        
        if (_titleComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите наименование товара");
            return;
        }
        
        if (_categoryComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите категорию товара");
            return;
        }
        
        if (_manufacturerComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите производителя");
            return;
        }
        
        if (_supplierComboBox.SelectedItem == null)
        {
            await ShowMessage("Ошибка", "Выберите поставщика");
            return;
        }
        
        if (string.IsNullOrWhiteSpace(_unitTextBox.Text))
        {
            await ShowMessage("Ошибка", "Введите единицу измерения");
            return;
        }
        
        if (!decimal.TryParse(_priceTextBox.Text, out decimal price) || price < 0)
        {
            await ShowMessage("Ошибка", "Цена должна быть положительным числом");
            return;
        }
        
        if (!int.TryParse(_quantityTextBox.Text, out int qty) || qty < 0)
        {
            await ShowMessage("Ошибка", "Количество должно быть целым неотрицательным числом");
            return;
        }
        
        if (!int.TryParse(_discountTextBox.Text, out int discount) || discount < 0 || discount > 100)
        {
            await ShowMessage("Ошибка", "Скидка должна быть числом от 0 до 100");
            return;
        }
        
        string imageFileName = null;
        if (!string.IsNullOrEmpty(_selectedImagePath))
        {
            try
            {
                string extension = System.IO.Path.GetExtension(_selectedImagePath);
                imageFileName = $"{Guid.NewGuid()}{extension}";
                
                string assetsPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "..", "..", "..", "Assets", imageFileName);
                
                string assetsDir = System.IO.Path.GetDirectoryName(assetsPath);
                if (!Directory.Exists(assetsDir))
                {
                    Directory.CreateDirectory(assetsDir);
                }
                
                File.Copy(_selectedImagePath, assetsPath, true);
            }
            catch (Exception ex)
            {
                await ShowMessage("Ошибка", $"Не удалось сохранить изображение: {ex.Message}");
                return;
            }
        }
        
        try
        {
            int titleId = (int)((ComboBoxItem)_titleComboBox.SelectedItem).Tag;
            int categoryId = (int)((ComboBoxItem)_categoryComboBox.SelectedItem).Tag;
            int manufacturerId = (int)((ComboBoxItem)_manufacturerComboBox.SelectedItem).Tag;
            int supplierId = (int)((ComboBoxItem)_supplierComboBox.SelectedItem).Tag;
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                
                string query = @"
                    INSERT INTO products 
                    (articl, title_id, unit_of_measurement, price, suplitiers_id, 
                     manufactures_id, categories_products, discount, qty, description, image)
                    VALUES 
                    (@articl, @title_id, @unit, @price, @splitters_id, 
                     @manufactures_id, @categories_products, @discount, @qty, @description, @image)";
                
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@articl", _articlTextBox.Text);
                    cmd.Parameters.AddWithValue("@title_id", titleId);
                    cmd.Parameters.AddWithValue("@unit", _unitTextBox.Text);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@splitters_id", supplierId);
                    cmd.Parameters.AddWithValue("@manufactures_id", manufacturerId);
                    cmd.Parameters.AddWithValue("@categories_products", categoryId);
                    cmd.Parameters.AddWithValue("@discount", discount);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@description", _descriptionTextBox.Text ?? "");
                    cmd.Parameters.AddWithValue("@image", imageFileName ?? (object)DBNull.Value);
                    
                    cmd.ExecuteNonQuery();
                }
            }
            
            await ShowMessage("Успех", "Товар успешно создан");
            
            var productManagement = new ProductManagement();
            productManagement.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            await ShowMessage("Ошибка", $"Не удалось сохранить товар: {ex.Message}");
        }
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var productManagement = new ProductManagement();
        productManagement.Show();
        this.Close();
    }

    private async System.Threading.Tasks.Task ShowMessage(string title, string message)
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
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            }
        };
        await messageBox.ShowDialog(this);
    }
}


    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class Title
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }