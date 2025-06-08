using System.Windows;

namespace Salarm.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }
}