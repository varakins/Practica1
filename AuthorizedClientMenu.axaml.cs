using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Practica1;

public partial class AuthorizedClientMenu : Window
{
    public AuthorizedClientMenu()
    {
        InitializeComponent();
    }
    
    private void OnBackButtonClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

    private void OnClickClientView(object sender, RoutedEventArgs e)
    {
        var clientView = new ClientView();
        clientView.Show();
        this.Close();
    }
}