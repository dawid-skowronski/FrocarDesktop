using AdminPanel.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPanel.Tests.ViewModels
{
    public class HomePageAdminViewModelTests
    {
        [Fact]
        public void Constructor_CreatesInstance_WithoutThrowingException()
        {
            // Arrange & Act
            var viewModel = new HomePageAdminViewModel();

            // Assert
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void Constructor_InstantiatesCorrectType_IsHomePageAdminViewModel()
        {
            // Arrange & Act
            var viewModel = new HomePageAdminViewModel();

            // Assert
            Assert.IsType<HomePageAdminViewModel>(viewModel);
        }

        [Fact]
        public void Constructor_InheritsFromViewModelBase_IsAssignableToViewModelBase()
        {
            // Arrange & Act
            var viewModel = new HomePageAdminViewModel();

            // Assert
            Assert.IsAssignableFrom<ViewModelBase>(viewModel);
        }
    }
}