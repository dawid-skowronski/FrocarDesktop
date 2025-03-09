using AdminPanel.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AdminPanel.Views
{
    public partial class CreateCar : UserControl
    {
        public CreateCar()
        {
            InitializeComponent();
            DataContext = new CreateCarViewModel(); // To jest wa¿ne!
        }
    }
}
