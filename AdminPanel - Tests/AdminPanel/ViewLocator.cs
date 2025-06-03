using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using System;

namespace AdminPanel
{
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var viewModelName = param.GetType().FullName;
            var viewName = viewModelName!.Replace("AdminPanel.ViewModels", "AdminPanel.Views")
                                        .Replace("ViewModel", "View", StringComparison.Ordinal);

            var type = typeof(HomePageView).Assembly.GetType(viewName);
            if (type is null)
                return new TextBlock { Text = "Not Found: " + viewName };

            try
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            catch (Exception ex)
            {
                return new TextBlock { Text = "Error: " + ex.Message };
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}