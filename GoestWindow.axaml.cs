using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Templates;
using System.Collections.Generic;
using System.Linq;
using Practica1.Models;
using Avalonia.Media;

namespace Practica1;

public partial class GoestWindow : Window
{
    private ListBox _productsListBox;
    private List<Product> _products;

    public GoestWindow()
    {
        InitializeComponent();
        
        _productsListBox = this.FindControl<ListBox>("ProductsListBox");
        
        LoadProducts();
    }
    
    private void LoadProducts()
    {
        _productsListBox.ItemTemplate = new FuncDataTemplate<Product>((product, _) =>
        {
            return CreateProductTemplate(product);
        });
        
        var db = new DatabaseHelper();
        _products = db.GetProducts();
        
        _productsListBox.ItemsSource = _products;
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
        Margin = new Avalonia.Thickness(0, 0, 0, 10)
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
    
    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }
}