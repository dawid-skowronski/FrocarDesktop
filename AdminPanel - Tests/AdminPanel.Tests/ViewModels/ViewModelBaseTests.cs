using AdminPanel.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdminPanel.Tests.ViewModels
{
    public class ViewModelBaseTests
    {
        [Fact]
        public void Constructor_CreatesInstance_WithoutThrowingException()
        {
            // Arrange & Act
            var viewModel = new ViewModelBase();

            // Assert
            Assert.NotNull(viewModel);
        }

        [Fact]
        public void Constructor_InstantiatesCorrectType_IsViewModelBase()
        {
            // Arrange & Act
            var viewModel = new ViewModelBase();

            // Assert
            Assert.IsType<ViewModelBase>(viewModel);
        }

        [Fact]
        public void Constructor_InheritsFromReactiveObject_IsAssignableToReactiveObject()
        {
            // Arrange & Act
            var viewModel = new ViewModelBase();

            // Assert
            Assert.IsAssignableFrom<ReactiveObject>(viewModel);
        }
    }
}