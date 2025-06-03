using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AdminPanel.ViewModels;

namespace AdminPanel.Views
{
    public partial class StatisticsView : UserControl
    {
        public StatisticsView()
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}