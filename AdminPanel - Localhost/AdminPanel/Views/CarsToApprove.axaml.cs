using Avalonia.Controls;
using AdminPanel.ViewModels;

namespace AdminPanel.Views
{
    public partial class CarsToApprove : UserControl
    {
        public CarsToApprove()
        {
            InitializeComponent();
            DataContext = new CarsToApproveViewModel();
        }
    }
}