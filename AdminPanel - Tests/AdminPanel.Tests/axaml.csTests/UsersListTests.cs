using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class UsersListTests
    {
        [Fact]
        public void Constructor_SetsDataContextToUsersListViewModel()
        {
            // Arrange & Act
            var usersList = new UsersList();

            // Assert
            Assert.NotNull(usersList.DataContext);
            Assert.IsType<UsersListViewModel>(usersList.DataContext);
        }
    }
}