using Avalonia;
using Xunit;

namespace AdminPanel.Tests
{
    public abstract class TestBase : IDisposable
    {
        static TestBase()
        {
            AvaloniaTestSetup.Initialize();
        }

        public void Dispose()
        {
        }
    }
}