using Avalonia.Controls;
using AdminPanel.ViewModels;

namespace AdminPanel.Views
{
    public partial class RentalsList : UserControl
    {
        public RentalsList()
        {
            InitializeComponent();
            DataContext = new RentalsListViewModel();
        }
    }
}