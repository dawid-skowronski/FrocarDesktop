using Avalonia.Controls;

namespace AdminPanel.Views
{
    public partial class RegisterPage : UserControl
    {
        public RegisterPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.RegisterPageViewModel();
        }
    }
}
