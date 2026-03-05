using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Practica1;

public partial class AdministratorMenu : Window
{
    public AdministratorMenu()
    {
        InitializeComponent();
    }
    
    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

    private void OnProductManagementClick(object sender, RoutedEventArgs e)
    {
        var onProductManagementClick = new ProductManagement();
        onProductManagementClick.Show();
        this.Close();        
    }

    private void OnOrderManagementClick(object sender, RoutedEventArgs e)
    { 
        var orderManagement = new OrderManagement();
        orderManagement.Show();
        this.Close();
    }
}