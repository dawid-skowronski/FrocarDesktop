using AdminPanel.ViewModels;
using Avalonia.Controls;

namespace AdminPanel.Views
{
    public partial class CarsList : UserControl
    {
        public CarsList()
        {
            InitializeComponent();
            DataContext = new CarsListViewModel();
        }
    }
}
