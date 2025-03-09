using Avalonia.Controls;

namespace AdminPanel.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.WindowState = WindowState.Maximized;
    }
}