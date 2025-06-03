using AdminPanel.ViewModels;
using Avalonia.Controls;

namespace AdminPanel.Views
{
    public partial class UsersList : UserControl
    {
        public UsersList()
        {
            InitializeComponent();
            DataContext = new UsersListViewModel();
        }
    }
}