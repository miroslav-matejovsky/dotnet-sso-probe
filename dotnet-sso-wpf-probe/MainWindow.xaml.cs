using System;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace dotnet_sso_wpf_probe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configure Serilog to write to the console
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Redirect Console output to the UI textbox so Serilog Console sink appears in the TextBox
            Console.SetOut(new TextBoxWriter(LogTextBox));
            Log.Information("Console output redirected to UI");

            Log.Information("UI initialized");

            this.Closed += MainWindow_Closed;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Logged in";
            Log.Information("Login clicked");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Logged out";
            Log.Information("Logout clicked");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Log.Information("Application closing");
            Log.CloseAndFlush();
        }
    }
}