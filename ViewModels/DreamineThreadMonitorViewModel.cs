using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Dreamine.MVVM.ViewModels;
using Dreamine.Threading.Interfaces;
using Dreamine.Threading.Models;
using Dreamine.Threading.Wpf.Services;

namespace Dreamine.Threading.Wpf.ViewModels;

/// <summary>
/// Provides the ViewModel for the Dreamine thread monitor view.
/// </summary>
/// <remarks>
/// Polls the thread manager periodically and applies a diff-based update to
/// <see cref="Threads"/>: existing rows are mutated in place (only changed
/// fields raise <see cref="INotifyPropertyChanged.PropertyChanged"/>), new
/// threads are appended, and removed threads are deleted. This avoids the
/// flicker and selection loss caused by a full <c>Clear()</c> + <c>Add()</c>
/// refresh on every tick. UI updates are coalesced through
/// <see cref="BatchedDispatcher{T}"/> at <see cref="DispatcherPriority.Background"/>
/// so the dispatcher queue cannot accumulate pending operations.
/// </remarks>
public sealed class DreamineThreadMonitorViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Default refresh interval for the polling timer.
    /// </summary>
    public static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMilliseconds(500);

    private readonly IDreamineThreadManager _threadManager;
    private readonly IThreadUiDispatcher _dispatcher;
    private readonly Timer _refreshTimer;
    private readonly BatchedDispatcher<IReadOnlyList<DreamineThreadInfo>> _uiBatch;
    private readonly Dictionary<string, ThreadInfoRow> _rowsByName = new(StringComparer.Ordinal);

    private ThreadInfoRow? _selectedThread;
    private int _disposed;

    private readonly RelayCommand _startCommand;
    private readonly AsyncRelayCommand _stopCommand;
    private readonly RelayCommand _pauseCommand;
    private readonly RelayCommand _resumeCommand;
    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _startAllCommand;
    private readonly AsyncRelayCommand _stopAllCommand;
    private readonly RelayCommand _pauseAllCommand;
    private readonly RelayCommand _resumeAllCommand;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the thread information collection.
    /// </summary>
    public ObservableCollection<ThreadInfoRow> Threads { get; } = new();

    /// <summary>Gets the start selected thread command.</summary>
    public ICommand StartCommand => _startCommand;

    /// <summary>Gets the stop selected thread command.</summary>
    public ICommand StopCommand => _stopCommand;

    /// <summary>Gets the pause selected thread command.</summary>
    public ICommand PauseCommand => _pauseCommand;

    /// <summary>Gets the resume selected thread command.</summary>
    public ICommand ResumeCommand => _resumeCommand;

    /// <summary>Gets the refresh command.</summary>
    public ICommand RefreshCommand => _refreshCommand;

    /// <summary>Gets the start all threads command.</summary>
    public ICommand StartAllCommand => _startAllCommand;

    /// <summary>Gets the stop all threads command.</summary>
    public ICommand StopAllCommand => _stopAllCommand;

    /// <summary>Gets the pause all threads command.</summary>
    public ICommand PauseAllCommand => _pauseAllCommand;

    /// <summary>Gets the resume all threads command.</summary>
    public ICommand ResumeAllCommand => _resumeAllCommand;

    /// <summary>
    /// Gets or sets the selected thread row.
    /// </summary>
    public ThreadInfoRow? SelectedThread
    {
        get => _selectedThread;
        set
        {
            if (ReferenceEquals(_selectedThread, value))
            {
                return;
            }

            _selectedThread = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedDetailText));
            RaiseCommandStates();
        }
    }

    /// <summary>
    /// Gets the selected thread detail text.
    /// </summary>
    public string SelectedDetailText
    {
        get
        {
            var row = SelectedThread;
            if (row is null)
            {
                return string.Empty;
            }

            return
                $"Name: {row.Name}{Environment.NewLine}" +
                $"Status: {row.Status}{Environment.NewLine}" +
                $"Priority: {row.Priority}{Environment.NewLine}" +
                $"Interval: {row.IntervalMs} ms{Environment.NewLine}" +
                $"Core: {(row.CoreIndex?.ToString() ?? "None")}{Environment.NewLine}" +
                $"Affinity: {row.UseAffinity}{Environment.NewLine}" +
                $"Job Count: {row.JobCount}{Environment.NewLine}" +
                $"Cycle Count: {row.CycleCount}{Environment.NewLine}" +
                $"Started At: {row.StartedAt}{Environment.NewLine}" +
                $"Stopped At: {row.StoppedAt}{Environment.NewLine}" +
                $"Last Error: {row.LastErrorMessage}";
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineThreadMonitorViewModel"/> class.
    /// </summary>
    /// <param name="threadManager">The Dreamine thread manager.</param>
    /// <param name="dispatcher">The UI dispatcher.</param>
    public DreamineThreadMonitorViewModel(
        IDreamineThreadManager threadManager,
        IThreadUiDispatcher dispatcher)
        : this(threadManager, dispatcher, DefaultRefreshInterval)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DreamineThreadMonitorViewModel"/> class.
    /// </summary>
    /// <param name="threadManager">The Dreamine thread manager.</param>
    /// <param name="dispatcher">The UI dispatcher.</param>
    /// <param name="refreshInterval">Polling interval. Must be positive.</param>
    public DreamineThreadMonitorViewModel(
        IDreamineThreadManager threadManager,
        IThreadUiDispatcher dispatcher,
        TimeSpan refreshInterval)
    {
        _threadManager = threadManager ?? throw new ArgumentNullException(nameof(threadManager));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

        if (refreshInterval <= TimeSpan.Zero)
        {
            refreshInterval = DefaultRefreshInterval;
        }

        _uiBatch = new BatchedDispatcher<IReadOnlyList<DreamineThreadInfo>>(
            dispatcher.Dispatcher,
            ApplySnapshotsOnUiThread,
            DispatcherPriority.Background);

        _startCommand = new RelayCommand(StartSelectedThread, HasSelectedThread);
        _stopCommand = new AsyncRelayCommand(StopSelectedThread, HasSelectedThread);
        _pauseCommand = new RelayCommand(PauseSelectedThread, HasSelectedThread);
        _resumeCommand = new RelayCommand(ResumeSelectedThread, HasSelectedThread);
        _refreshCommand = new RelayCommand(Refresh);
        _startAllCommand = new RelayCommand(StartAllThreads);
        _stopAllCommand = new AsyncRelayCommand(StopAllThreads);
        _pauseAllCommand = new RelayCommand(PauseAllThreads);
        _resumeAllCommand = new RelayCommand(ResumeAllThreads);

        // Initial sync refresh so the grid is populated before the timer fires.
        Refresh();

        _refreshTimer = new Timer(
            OnTimerTick,
            null,
            refreshInterval,
            refreshInterval);
    }

    /// <summary>
    /// Forces an immediate refresh of the thread monitor data.
    /// </summary>
    public void Refresh()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        var infos = _threadManager.GetThreadInfos();
        _uiBatch.Enqueue(infos);
    }

    private void OnTimerTick(object? state)
    {
        // Timer callback runs on a thread-pool thread. Just snapshot and enqueue;
        // the UI-thread merge happens in ApplySnapshotsOnUiThread.
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        try
        {
            var infos = _threadManager.GetThreadInfos();
            _uiBatch.Enqueue(infos);
        }
        catch
        {
            // Polling failures must not bring down the UI.
        }
    }

    private void ApplySnapshotsOnUiThread(IReadOnlyList<IReadOnlyList<DreamineThreadInfo>> batch)
    {
        // UI thread. Use only the most recent snapshot in the batch — older
        // snapshots inside the same batch are stale by definition.
        if (Volatile.Read(ref _disposed) != 0 || batch.Count == 0)
        {
            return;
        }

        var latest = batch[batch.Count - 1];

        // Track which existing rows are still present.
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var anyStructuralChange = false;
        var selectedRowChanged = false;

        // Apply additions / updates.
        for (var i = 0; i < latest.Count; i++)
        {
            var info = latest[i];
            seen.Add(info.Name);

            if (_rowsByName.TryGetValue(info.Name, out var existing))
            {
                var changed = existing.UpdateFrom(info);
                if (changed && ReferenceEquals(existing, _selectedThread))
                {
                    selectedRowChanged = true;
                }
            }
            else
            {
                var row = new ThreadInfoRow(info);
                _rowsByName[info.Name] = row;
                Threads.Add(row);
                anyStructuralChange = true;
            }
        }

        // Apply removals.
        for (var i = Threads.Count - 1; i >= 0; i--)
        {
            var row = Threads[i];
            if (!seen.Contains(row.Name))
            {
                Threads.RemoveAt(i);
                _rowsByName.Remove(row.Name);

                if (ReferenceEquals(row, _selectedThread))
                {
                    SelectedThread = null;
                }

                anyStructuralChange = true;
            }
        }

        if (selectedRowChanged)
        {
            // Selected row's underlying fields changed — refresh the detail text.
            OnPropertyChanged(nameof(SelectedDetailText));
        }

        if (anyStructuralChange || selectedRowChanged)
        {
            RaiseCommandStates();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // Wait for an in-flight timer callback when the runtime can signal it.
        // Dispose can return false when the timer is already disposed or no
        // callback can be signaled; in that case disposal remains best-effort.
        // The wait is bounded so UI shutdown cannot hang indefinitely.
        using var disposed = new ManualResetEvent(false);
        try
        {
            if (_refreshTimer.Dispose(disposed))
            {
                disposed.WaitOne(TimeSpan.FromSeconds(1));
            }
        }
        catch
        {
            // Suppress on dispose.
        }
    }

    private bool HasSelectedThread() => SelectedThread is not null;

    private void StartSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Start(name);
        Refresh();
    }

    private async Task StopSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        await _threadManager.StopAsync(name).ConfigureAwait(true);
        Refresh();
    }

    private void PauseSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Pause(name);
        Refresh();
    }

    private void ResumeSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Resume(name);
        Refresh();
    }

    private void StartAllThreads()
    {
        _threadManager.StartAll();
        Refresh();
    }

    private async Task StopAllThreads()
    {
        await _threadManager.StopAllAsync().ConfigureAwait(true);
        Refresh();
    }

    private void PauseAllThreads()
    {
        _threadManager.PauseAll();
        Refresh();
    }

    private void ResumeAllThreads()
    {
        _threadManager.ResumeAll();
        Refresh();
    }

    private void RaiseCommandStates()
    {
        _startCommand.RaiseCanExecuteChanged();
        _stopCommand.RaiseCanExecuteChanged();
        _pauseCommand.RaiseCanExecuteChanged();
        _resumeCommand.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
