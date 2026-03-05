using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Practica1;

public partial class ManagerMenu : Window
{
    public ManagerMenu()
    {
        InitializeComponent();
    }
    
    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }
    
    private void OnViewProductsClick(object sender, RoutedEventArgs e)
    {
        var viewingManagerWindow = new ViewingMnager();
        viewingManagerWindow.Show();
        this.Close();
    }

    private void OnOrderManagerClick(object sender, RoutedEventArgs e)
    {
        var orderManager = new OrderManager();
        orderManager.Show();
        this.Close();
    }
}