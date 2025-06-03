using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace AdminPanel.Views
{
    public partial class ConfirmMessageBoxView : Window
    {
        public static readonly StyledProperty<string> MessageProperty =
            AvaloniaProperty.Register<ConfirmMessageBoxView, string>(nameof(Message));

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public TaskCompletionSource<bool> Result { get; } = new TaskCompletionSource<bool>();

        public ConfirmMessageBoxView()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ConfirmMessageBoxView(string title) : this()
        {
            Title = title;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            var yesButton = this.FindControl<Button>("YesButton");
            var noButton = this.FindControl<Button>("NoButton");

            if (yesButton != null)
                yesButton.Click += YesButton_Click;
            if (noButton != null)
                noButton.Click += NoButton_Click;
        }

        private void YesButton_Click(object? sender, RoutedEventArgs e)
        {
            Result.SetResult(true);
            Close();
        }

        private void NoButton_Click(object? sender, RoutedEventArgs e)
        {
            Result.SetResult(false);
            Close();
        }
    }
}