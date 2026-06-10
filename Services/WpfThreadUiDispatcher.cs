using System;
using System.Windows;
using System.Windows.Threading;

namespace Dreamine.Threading.Wpf.Services;

/// <summary>
/// Provides UI thread dispatching for Dreamine threading WPF components.
/// </summary>
public sealed class WpfThreadUiDispatcher : IThreadUiDispatcher
{
    private readonly Dispatcher _dispatcher;

    /// <summary>
    /// Gets the underlying WPF <see cref="Dispatcher"/>.
    /// </summary>
    public Dispatcher Dispatcher => _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfThreadUiDispatcher"/> class
    /// using the current application or thread dispatcher.
    /// </summary>
    public WpfThreadUiDispatcher()
        : this(Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WpfThreadUiDispatcher"/> class.
    /// </summary>
    /// <param name="dispatcher">The WPF dispatcher.</param>
    public WpfThreadUiDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// Executes the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.Invoke(action);
    }

    /// <summary>
    /// Executes the specified action asynchronously on the UI thread at
    /// <see cref="DispatcherPriority.Background"/> priority.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public void BeginInvoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_dispatcher.CheckAccess())
        {
            action();
            return;
        }

        _dispatcher.BeginInvoke(action, DispatcherPriority.Background);
    }
}
