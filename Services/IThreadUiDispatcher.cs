using System;
using System.Windows.Threading;

namespace Dreamine.Threading.Wpf.Services;

/// <summary>
/// Abstracts UI thread dispatching for threading monitor view models.
/// </summary>
public interface IThreadUiDispatcher
{
    /// <summary>
    /// Gets the underlying dispatcher used by batched UI updates.
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// Executes the specified action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void Invoke(Action action);

    /// <summary>
    /// Executes the specified action asynchronously on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void BeginInvoke(Action action);
}
