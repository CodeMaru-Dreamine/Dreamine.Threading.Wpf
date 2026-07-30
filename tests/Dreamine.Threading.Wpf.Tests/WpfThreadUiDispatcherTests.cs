using System.Windows.Threading;
using Dreamine.Threading.Wpf.Services;

namespace Dreamine.Threading.Wpf.Tests;

public sealed class WpfThreadUiDispatcherTests
{
    [Fact]
    public void InvokeRunsImmediatelyWhenDispatcherAccessIsAvailable()
    {
        var service = new WpfThreadUiDispatcher(Dispatcher.CurrentDispatcher);
        var invoked = false;

        service.Invoke(() => invoked = true);

        Assert.True(invoked);
        Assert.Same(Dispatcher.CurrentDispatcher, service.Dispatcher);
    }

    [Fact]
    public void BeginInvokeRunsImmediatelyWhenDispatcherAccessIsAvailable()
    {
        var service = new WpfThreadUiDispatcher(Dispatcher.CurrentDispatcher);
        var invoked = false;

        service.BeginInvoke(() => invoked = true);

        Assert.True(invoked);
    }

    [Fact]
    public void ConstructorAndMethodsRejectNull()
    {
        Assert.Throws<ArgumentNullException>(() => new WpfThreadUiDispatcher(null!));

        var service = new WpfThreadUiDispatcher(Dispatcher.CurrentDispatcher);
        Assert.Throws<ArgumentNullException>(() => service.Invoke(null!));
        Assert.Throws<ArgumentNullException>(() => service.BeginInvoke(null!));
    }
}
