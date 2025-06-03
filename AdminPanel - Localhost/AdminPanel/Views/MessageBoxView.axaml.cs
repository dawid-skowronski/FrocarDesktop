using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace AdminPanel.Views
{
    public partial class MessageBoxView : Window
    {
        public static readonly StyledProperty<string> MessageProperty =
            AvaloniaProperty.Register<MessageBoxView, string>(nameof(Message));

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public MessageBoxView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public MessageBoxView(string title) : this()
        {
            Title = title;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var okButton = this.FindControl<Button>("OkButton");
            if (okButton != null)
                okButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}