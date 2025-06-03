using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdminPanel.Views
{
    public partial class HomePageView : UserControl
    {
        public HomePageView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}