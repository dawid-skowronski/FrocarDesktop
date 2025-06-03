using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdminPanel.Views
{
    public partial class EditCarDialog : Window
    {
        public EditCarDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}