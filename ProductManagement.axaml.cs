using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Templates;
using System.Collections.Generic;
using System.Linq;
using Practica1.Models;
using Avalonia.Media;
using System;
using Avalonia.Input;

namespace Practica1;

public partial class ProductManagement : Window
{
    private ListBox _productsListBox;
    private List<Product> _allProducts;
    private List<Product> _filteredProducts;
    
    private TextBox _searchTextBox;
    private ComboBox _sortComboBox;
    private ComboBox _filterComboBox;    

    public ProductManagement()
    {
        InitializeComponent();

        _productsListBox = this.FindControl<ListBox>("ProductsListBox");
        _searchTextBox = this.FindControl<TextBox>("SearchTextBox");
        _sortComboBox = this.FindControl<ComboBox>("SortComboBox");
        _filterComboBox = this.FindControl<ComboBox>("FilterComboBox");
        
        LoadProducts();
        LoadSuppliers();
        
        // Добавляем обработчик двойного клика
        _productsListBox.DoubleTapped += OnProductDoubleTapped;
    }

    private void LoadProducts()
    {
        _productsListBox.ItemTemplate = new FuncDataTemplate<Product>((product, _) =>
        {
            return CreateProductTemplate(product);
        });
        
        var db = new DatabaseHelper();
        _allProducts = db.GetProducts();
        _filteredProducts = new List<Product>(_allProducts);
        
        _productsListBox.ItemsSource = _filteredProducts;
    }

    private void LoadSuppliers()
    {
        var suppliers = _allProducts
            .Select(p => p.SupplierName)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();
        
        _filterComboBox.Items.Clear();
        
        var allItem = new ComboBoxItem();
        allItem.Content = "Все поставщики";
        _filterComboBox.Items.Add(allItem);
        
        foreach (var supplier in suppliers)
        {
            var item = new ComboBoxItem();
            item.Content = supplier;
            _filterComboBox.Items.Add(item);
        }
        
        _filterComboBox.SelectedIndex = 0;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }
    
    private void OnSortSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }
    
    private void OnFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyFilters();
    }
    
    private void ApplyFilters()
    {
        if (_allProducts == null) return;
        
        IEnumerable<Product> query = _allProducts;
        
        string selectedSupplier = GetSelectedComboBoxItemContent(_filterComboBox);
        if (!string.IsNullOrEmpty(selectedSupplier) && selectedSupplier != "Все поставщики")
        {
            query = query.Where(p => p.SupplierName == selectedSupplier);
        }
        
        string searchText = _searchTextBox?.Text?.ToLower() ?? "";
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(p => 
                (p.TitleName?.ToLower().Contains(searchText) ?? false) ||
                (p.CategoryName?.ToLower().Contains(searchText) ?? false) ||
                (p.Description?.ToLower().Contains(searchText) ?? false) ||
                (p.ManufactureName?.ToLower().Contains(searchText) ?? false) ||
                (p.SupplierName?.ToLower().Contains(searchText) ?? false) ||
                (p.Artcl?.ToLower().Contains(searchText) ?? false)
            );
        }
        
        string sort = GetSelectedComboBoxItemContent(_sortComboBox);
        if (!string.IsNullOrEmpty(sort) && sort != "Без сортировки")
        {
            switch (sort)
            {
                case "По возрастанию":
                    query = query.OrderBy(p => p.Qty);
                    break;
                case "По убыванию":
                    query = query.OrderByDescending(p => p.Qty);
                    break;
            }
        }
        
        _filteredProducts = query.ToList();
        _productsListBox.ItemsSource = null;
        _productsListBox.ItemsSource = _filteredProducts;
    }

    private string GetSelectedComboBoxItemContent(ComboBox comboBox)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            return selectedItem.Content?.ToString() ?? "";
        }
        return "";
    }

    private Control CreateProductTemplate(Product product)
    {
        string backgroundColor = "#FFFFFF";
        
        if (product.Qty == 0)
        {
            backgroundColor = "#ADD8E6";
        }
        else if (product.Discount > 15)
        {
            backgroundColor = "#2E8B57";
        }
        
        var border = new Border
        {
            Background = Brush.Parse(backgroundColor),
            CornerRadius = new Avalonia.CornerRadius(10),
            Padding = new Avalonia.Thickness(15),
            BorderBrush = Brush.Parse("#CCCCCC"),
            BorderThickness = new Avalonia.Thickness(1),
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
            Cursor = new Cursor(StandardCursorType.Hand) // Меняем курсор при наведении
        };
        
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
        
        var photoBorder = new Border
        {
            Background = Brush.Parse("#ffffff"),
            CornerRadius = new Avalonia.CornerRadius(5),
            Width = 100,
            Height = 100,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        string imagePath = string.IsNullOrEmpty(product.Image) 
        ? "/Assets/picture.png" 
        : $"C:/Users/aleks/Desktop/Practica1/Assets/{product.Image}";
        
        try
        {
            var image = new Image
            {
                Source = new Avalonia.Media.Imaging.Bitmap(imagePath),
                Stretch = Avalonia.Media.Stretch.Uniform,
                Width = 80,
                Height = 80
            };
            photoBorder.Child = image;
        }
        catch
        {
            try
            {
                var fallbackImage = new Image
                {
                    Source = new Avalonia.Media.Imaging.Bitmap("C:/Users/aleks/Desktop/Practica1/Assets/picture.png"),
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    Width = 80,
                    Height = 80
                };
                photoBorder.Child = fallbackImage;
            }
            catch
            {
                
            }
        }
        
        Grid.SetColumn(photoBorder, 0);
        grid.Children.Add(photoBorder);
        
        var infoPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        
        var titleText = new TextBlock();
        titleText.Inlines.Add(new Avalonia.Controls.Documents.Run 
        { 
            Text = product.CategoryName, 
            FontWeight = FontWeight.Normal 
        });
        titleText.Inlines.Add(new Avalonia.Controls.Documents.Run 
        { 
            Text = " | " 
        });
        titleText.Inlines.Add(new Avalonia.Controls.Documents.Run 
        { 
            Text = product.TitleName, 
            FontWeight = FontWeight.Bold,
            FontSize = 16
        });
        
        infoPanel.Children.Add(titleText);
        
        infoPanel.Children.Add(new TextBlock
        {
            Text = $"Описание товара: {product.Description}",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 5, 0, 5)
        });
        
        var manufacturerPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        manufacturerPanel.Children.Add(new TextBlock 
        { 
            Text = "Производитель: ", 
            FontWeight = FontWeight.SemiBold 
        });
        manufacturerPanel.Children.Add(new TextBlock 
        { 
            Text = product.ManufactureName
        });
        infoPanel.Children.Add(manufacturerPanel);
        
        var supplierPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        supplierPanel.Children.Add(new TextBlock 
        { 
            Text = "Поставщик: ", 
            FontWeight = FontWeight.SemiBold 
        });
        supplierPanel.Children.Add(new TextBlock 
        { 
            Text = product.SupplierName
        });
        infoPanel.Children.Add(supplierPanel);
        
        var pricePanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        pricePanel.Children.Add(new TextBlock 
        { 
            Text = "Цена: ", 
            FontWeight = FontWeight.SemiBold 
        });
        
        if (product.Discount > 0)
        {
            var oldPriceText = new TextBlock
            {
                Text = $"{product.Price:N2} ₽",
                Foreground = Brushes.Red,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            
            oldPriceText.TextDecorations = TextDecorations.Strikethrough;
            
            pricePanel.Children.Add(oldPriceText);

            decimal newPrice = product.Price * (100 - product.Discount) / 100;
            pricePanel.Children.Add(new TextBlock
            {
                Text = $"{newPrice:N2} ₽",
                Foreground = Brushes.Black
            });
        }
        else
        {
            pricePanel.Children.Add(new TextBlock
            {
                Text = $"{product.Price:N2} ₽"
            });
        }
        infoPanel.Children.Add(pricePanel);

        var unitPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        unitPanel.Children.Add(new TextBlock 
        { 
            Text = "Единица измерения: ", 
            FontWeight = FontWeight.SemiBold 
        });
        unitPanel.Children.Add(new TextBlock 
        { 
            Text = product.UnitOfMeasurement
        });
        infoPanel.Children.Add(unitPanel);

        var qtyPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };
        qtyPanel.Children.Add(new TextBlock 
        { 
            Text = "Количество на складе: ", 
            FontWeight = FontWeight.SemiBold 
        });
        
        var qtyText = new TextBlock { Text = product.Qty.ToString() };

        if (product.Qty == 0)
        {
            qtyText.Foreground = Brushes.Red;
            qtyText.FontWeight = FontWeight.Bold;
            qtyText.Text = "0";
        }
        else if (product.Qty < 5)
        {
            qtyText.Foreground = Brushes.Orange;
            qtyText.FontWeight = FontWeight.Bold;
            qtyText.Text = $"{product.Qty} (мало)";
        }
        
        qtyPanel.Children.Add(qtyText);
        infoPanel.Children.Add(qtyPanel);
        
        Grid.SetColumn(infoPanel, 1);
        grid.Children.Add(infoPanel);

        var discountColor = product.Discount > 0 ? "#ffffff" : "#ffffff";
        var discountTextColor = product.Discount > 0 ? "#000000" : "#000000";
        
        var discountBorder = new Border
        {
            Background = Brush.Parse(discountColor),
            CornerRadius = new Avalonia.CornerRadius(5),
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Width = 80,
            Height = 80
        };
        
        var discountPanel = new StackPanel
        {
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        
        discountPanel.Children.Add(new TextBlock
        {
            Text = "Скидка",
            FontSize = 12,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });
        
        discountPanel.Children.Add(new TextBlock
        {
            Text = $"{product.Discount}%",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse(discountTextColor),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });
        
        discountBorder.Child = discountPanel;
        Grid.SetColumn(discountBorder, 2);
        grid.Children.Add(discountBorder);
        
        border.Child = grid;
        return border;
    }

    // Обработчик двойного нажатия на товар
    private void OnProductDoubleTapped(object sender, TappedEventArgs e)
    {
        var selectedProduct = _productsListBox.SelectedItem as Product;
        if (selectedProduct != null)
        {
            var productEdition = new ProductEdition();
            productEdition.LoadProductData(selectedProduct.Id);
            productEdition.Show();
            this.Close();
        }
    }

    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var administratorMenu = new AdministratorMenu();
        administratorMenu.Show();
        this.Close();
    }

    private void OnCreateButtonClick(object sender, RoutedEventArgs e)
    {
        var productCreating = new ProductCreating();
        productCreating.Show();
        this.Close();
    }

    private async void OnDeleteButtonClick(object sender, RoutedEventArgs e)
    {
        // Получаем выбранный товар
        var selectedProduct = _productsListBox.SelectedItem as Product;
        
        if (selectedProduct == null)
        {
            // Показываем сообщение, что товар не выбран
            var messageBox = new Window
            {
                Title = "Внимание",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "Выберите товар для удаления",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
            await messageBox.ShowDialog(this);
            return;
        }

        // Подтверждение удаления
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
                        Text = $"Вы уверены, что хотите удалить товар:\n{selectedProduct.TitleName}?",
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
        
        var buttonPanel = ((StackPanel)confirmBox.Content).Children[1] as StackPanel;
        var yesBtn = buttonPanel.Children[0] as Button;
        var noBtn = buttonPanel.Children[1] as Button;

        yesBtn.Click += async (s, args) =>
        {
            var db = new DatabaseHelper();
            db.DeleteProduct(selectedProduct.Id);
            
            _allProducts.Remove(selectedProduct);
            _filteredProducts.Remove(selectedProduct);
            _productsListBox.ItemsSource = null;
            _productsListBox.ItemsSource = _filteredProducts;
            
            confirmBox.Close();
            
            var successBox = new Window
            {
                Title = "Успех",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "Товар успешно удален",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
            await successBox.ShowDialog(this);
        };

        noBtn.Click += (s, args) =>
        {
            confirmBox.Close();
        };

        await confirmBox.ShowDialog(this);
    }
}