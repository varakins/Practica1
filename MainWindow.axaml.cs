using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Npgsql;
using System;

namespace Practica1;

public partial class MainWindow : Window
{
    private string connectionString = "Host=localhost;Username=postgres;Password=1;Database=Practica1";
    
    private TextBox _loginTextBox;
    private TextBox _passwordTextBox;

    public MainWindow()
    {
        InitializeComponent();
        
        _loginTextBox = this.FindControl<TextBox>("LoginTextBox");
        _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
    }
    
    private async void OnLoginButtonClick(object sender, RoutedEventArgs e)
    {
        string login = _loginTextBox.Text;
        string password = _passwordTextBox.Text;
        
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            var messageBox = new Window
            {
                Title = "Ошибка",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new TextBlock
                {
                    Text = "Введите логин и пароль",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
            await messageBox.ShowDialog(this);
            return;
        }
        
        try
        {
            int roleId = await CheckUserCredentials(login, password);
            
            if (roleId > 0)
            {
                switch (roleId)
                {
                    case 1:
                        var adminWindow = new AdministratorMenu();
                        adminWindow.Show();
                        this.Close();
                        break;
                        
                    case 2:
                        var managerWindow = new ManagerMenu();
                        managerWindow.Show();
                        this.Close();
                        break;
                        
                    case 3:
                        var clientWindow = new AuthorizedClientMenu();
                        clientWindow.Show();
                        this.Close();
                        break;
                        
                    default:
                        await ShowError("Неизвестная роль пользователя");
                        break;
                }
            }
            else
            {
                await ShowError("Неверный логин или пароль");
            }
        }
        catch (Exception ex)
        {
            await ShowError($"Ошибка подключения к БД: {ex.Message}");
        }
    }
    
    private async System.Threading.Tasks.Task<int> CheckUserCredentials(string login, string password)
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            await conn.OpenAsync();
            
            string query = @"
                SELECT role_users_id 
                FROM users 
                WHERE login = @login AND password = @password";
            
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@login", login);
                cmd.Parameters.AddWithValue("@password", password);
                
                var result = await cmd.ExecuteScalarAsync();
                
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
                
                return -1;
            }
        }
    }
    
    private async System.Threading.Tasks.Task ShowError(string message)
    {
        var errorBox = new Window
        {
            Title = "Ошибка",
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new TextBlock
            {
                Text = message,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(10)
            }
        };
        await errorBox.ShowDialog(this);
    }
    
    private void OnGuestButtonClick(object sender, RoutedEventArgs e)
    {
        var guestWindow = new GoestWindow();
        guestWindow.Show();
        this.Close();
    }
}