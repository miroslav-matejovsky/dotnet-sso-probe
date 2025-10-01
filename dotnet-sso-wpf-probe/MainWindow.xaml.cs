using System;
using System.Windows;
using System.Windows.Controls;

namespace dotnet_sso_wpf_probe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppendLog("UI initialized");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Replace with real login flow as needed
            StatusTextBlock.Text = "Logged in";
            AppendLog("Login clicked");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Replace with real logout flow as needed
            StatusTextBlock.Text = "Logged out";
            AppendLog("Logout clicked");
        }

        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\r\n");
                LogTextBox.ScrollToEnd();
            });
        }
    }
}