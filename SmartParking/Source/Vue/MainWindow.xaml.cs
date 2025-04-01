using SmartParking.Source.ViewModel;
using System;
using System.Windows;

namespace SmartParking
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainWindowViewModel();
            viewModel.OnLoginSuccess += NavigateToAdmin;
            this.DataContext = viewModel;

            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void NavigateToAdmin()
        {
            Admin adminPage = new Admin();
            this.Content = adminPage;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (viewModel != null)
                viewModel.Password = PasswordBox.Password;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
