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
/// \if KO
/// <para>Dreamine 스레드 모니터 뷰의 ViewModel을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides the view model for the Dreamine thread-monitor view.</para>
/// \endif
/// </summary>
/// <remarks>
/// \if KO
/// <para>스레드 관리자를 주기적으로 폴링하고 기존 행 갱신·신규 행 추가·삭제 행 제거 방식으로 <see cref="Threads"/>에 차이를 적용합니다. 전체 컬렉션 교체로 인한 깜박임과 선택 손실을 피하고, <see cref="BatchedDispatcher{T}"/>로 UI 갱신을 병합합니다.</para>
/// \endif
/// \if EN
/// <para>Periodically polls the manager and applies diffs to <see cref="Threads"/> by updating existing rows, adding new rows, and removing missing rows. This avoids flicker and selection loss from full replacement, while <see cref="BatchedDispatcher{T}"/> coalesces UI updates.</para>
/// \endif
/// </remarks>
public sealed class DreamineThreadMonitorViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// \if KO
    /// <para>폴링 타이머의 기본 새로 고침 간격입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The default refresh interval for the polling timer.</para>
    /// \endif
    /// </summary>
    public static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// \if KO
    /// <para>thread Manager 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the thread manager value.</para>
    /// \endif
    /// </summary>
    private readonly IDreamineThreadManager _threadManager;
    /// <summary>
    /// \if KO
    /// <para>dispatcher 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the dispatcher value.</para>
    /// \endif
    /// </summary>
    private readonly IThreadUiDispatcher _dispatcher;
    /// <summary>
    /// \if KO
    /// <para>refresh Timer 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the refresh timer value.</para>
    /// \endif
    /// </summary>
    private readonly Timer _refreshTimer;
    /// <summary>
    /// \if KO
    /// <para>ui Batch 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the ui batch value.</para>
    /// \endif
    /// </summary>
    private readonly BatchedDispatcher<IReadOnlyList<DreamineThreadInfo>> _uiBatch;
    /// <summary>
    /// \if KO
    /// <para>rows By Name 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the rows by name value.</para>
    /// \endif
    /// </summary>
    private readonly Dictionary<string, ThreadInfoRow> _rowsByName = new(StringComparer.Ordinal);

    /// <summary>
    /// \if KO
    /// <para>selected Thread 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the selected thread value.</para>
    /// \endif
    /// </summary>
    private ThreadInfoRow? _selectedThread;
    /// <summary>
    /// \if KO
    /// <para>disposed 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the disposed value.</para>
    /// \endif
    /// </summary>
    private int _disposed;

    /// <summary>
    /// \if KO
    /// <para>start Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the start command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _startCommand;
    /// <summary>
    /// \if KO
    /// <para>stop Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the stop command value.</para>
    /// \endif
    /// </summary>
    private readonly AsyncRelayCommand _stopCommand;
    /// <summary>
    /// \if KO
    /// <para>pause Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the pause command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _pauseCommand;
    /// <summary>
    /// \if KO
    /// <para>resume Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the resume command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _resumeCommand;
    /// <summary>
    /// \if KO
    /// <para>refresh Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the refresh command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _refreshCommand;
    /// <summary>
    /// \if KO
    /// <para>start All Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the start all command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _startAllCommand;
    /// <summary>
    /// \if KO
    /// <para>stop All Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the stop all command value.</para>
    /// \endif
    /// </summary>
    private readonly AsyncRelayCommand _stopAllCommand;
    /// <summary>
    /// \if KO
    /// <para>pause All Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the pause all command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _pauseAllCommand;
    /// <summary>
    /// \if KO
    /// <para>resume All Command 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the resume all command value.</para>
    /// \endif
    /// </summary>
    private readonly RelayCommand _resumeAllCommand;

    /// <summary>
    /// \if KO
    /// <para>ViewModel 속성 값이 변경될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when a view-model property value changes.</para>
    /// \endif
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// \if KO
    /// <para>안정적인 행 인스턴스로 유지되는 스레드 정보 컬렉션을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the thread-information collection maintained with stable row instances.</para>
    /// \endif
    /// </summary>
    public ObservableCollection<ThreadInfoRow> Threads { get; } = new();

    /// <summary>
    /// \if KO
    /// <para>선택 스레드 시작 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that starts the selected thread.</para>
    /// \endif
    /// </summary>
    public ICommand StartCommand => _startCommand;

    /// <summary>
    /// \if KO
    /// <para>선택 스레드 중지 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that stops the selected thread.</para>
    /// \endif
    /// </summary>
    public ICommand StopCommand => _stopCommand;

    /// <summary>
    /// \if KO
    /// <para>선택 스레드 일시 정지 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that pauses the selected thread.</para>
    /// \endif
    /// </summary>
    public ICommand PauseCommand => _pauseCommand;

    /// <summary>
    /// \if KO
    /// <para>선택 스레드 재개 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that resumes the selected thread.</para>
    /// \endif
    /// </summary>
    public ICommand ResumeCommand => _resumeCommand;

    /// <summary>
    /// \if KO
    /// <para>즉시 새로 고침 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the immediate-refresh command.</para>
    /// \endif
    /// </summary>
    public ICommand RefreshCommand => _refreshCommand;

    /// <summary>
    /// \if KO
    /// <para>모든 스레드 시작 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that starts all threads.</para>
    /// \endif
    /// </summary>
    public ICommand StartAllCommand => _startAllCommand;

    /// <summary>
    /// \if KO
    /// <para>모든 스레드 중지 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that stops all threads.</para>
    /// \endif
    /// </summary>
    public ICommand StopAllCommand => _stopAllCommand;

    /// <summary>
    /// \if KO
    /// <para>모든 스레드 일시 정지 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that pauses all threads.</para>
    /// \endif
    /// </summary>
    public ICommand PauseAllCommand => _pauseAllCommand;

    /// <summary>
    /// \if KO
    /// <para>모든 스레드 재개 명령을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the command that resumes all threads.</para>
    /// \endif
    /// </summary>
    public ICommand ResumeAllCommand => _resumeAllCommand;

    /// <summary>
    /// \if KO
    /// <para>선택된 스레드 행을 가져오거나 설정하고 상세 텍스트와 명령 상태를 갱신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the selected thread row and updates detail text and command state.</para>
    /// \endif
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
    /// \if KO
    /// <para>선택 스레드의 상태·코어·통계·오류를 여러 줄 텍스트로 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets status, core, statistics, and error details for the selected thread as multiline text.</para>
    /// \endif
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
    /// \if KO
    /// <para>기본 새로 고침 간격으로 <see cref="T:Dreamine.Threading.Wpf.ViewModels.DreamineThreadMonitorViewModel" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.ViewModels.DreamineThreadMonitorViewModel" /> with the default refresh interval.</para>
    /// \endif
    /// </summary>
    /// <param name="threadManager">
    /// \if KO
    /// <para>스냅샷과 제어 작업을 제공할 스레드 관리자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The thread manager providing snapshots and control operations.</para>
    /// \endif
    /// </param>
    /// <param name="dispatcher">
    /// \if KO
    /// <para>일괄 갱신을 실행할 UI Dispatcher입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The UI dispatcher on which batched updates execute.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>인수 중 하나가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when either argument is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public DreamineThreadMonitorViewModel(
        IDreamineThreadManager threadManager,
        IThreadUiDispatcher dispatcher)
        : this(threadManager, dispatcher, DefaultRefreshInterval)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 새로 고침 간격으로 <see cref="T:Dreamine.Threading.Wpf.ViewModels.DreamineThreadMonitorViewModel" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.ViewModels.DreamineThreadMonitorViewModel" /> with the specified refresh interval.</para>
    /// \endif
    /// </summary>
    /// <param name="threadManager">
    /// \if KO
    /// <para>스냅샷과 제어 작업을 제공할 스레드 관리자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The thread manager providing snapshots and control operations.</para>
    /// \endif
    /// </param>
    /// <param name="dispatcher">
    /// \if KO
    /// <para>일괄 갱신을 실행할 UI Dispatcher입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The UI dispatcher on which batched updates execute.</para>
    /// \endif
    /// </param>
    /// <param name="refreshInterval">
    /// \if KO
    /// <para>양수 폴링 간격이며 0 이하는 기본값으로 보정됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>The positive polling interval; non-positive values are normalized to the default.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="threadManager"/> 또는 <paramref name="dispatcher"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="threadManager"/> or <paramref name="dispatcher"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>관리자에서 즉시 스냅샷을 가져와 UI 일괄 갱신 큐에 넣습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Immediately obtains a manager snapshot and enqueues it for batched UI updating.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>ViewModel이 정리된 후에는 아무 작업도 하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>No action is taken after the view model has been disposed.</para>
    /// \endif
    /// </remarks>
    public void Refresh()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        var infos = _threadManager.GetThreadInfos();
        _uiBatch.Enqueue(infos);
    }

    /// <summary>
    /// \if KO
    /// <para>스레드 풀 타이머에서 관리자 스냅샷을 가져와 UI 큐에 넣습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Obtains a manager snapshot from the thread-pool timer and enqueues it for UI delivery.</para>
    /// \endif
    /// </summary>
    /// <param name="state">
    /// \if KO
    /// <para>타이머 상태 값이며 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The timer state value, which is ignored.</para>
    /// \endif
    /// </param>
    /// <remarks>
    /// \if KO
    /// <para>폴링 오류는 UI 프로세스가 종료되지 않도록 억제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Polling errors are suppressed so they cannot terminate the UI process.</para>
    /// \endif
    /// </remarks>
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

    /// <summary>
    /// \if KO
    /// <para>일괄 처리된 스냅샷 중 최신 항목을 UI 행 컬렉션에 차이 병합합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Diff-merges the newest snapshot in a batch into the UI row collection.</para>
    /// \endif
    /// </summary>
    /// <param name="batch">
    /// \if KO
    /// <para>오래된 것부터 최신 순서로 병합된 스레드 정보 스냅샷 목록입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The coalesced thread-information snapshots ordered from older to newest.</para>
    /// \endif
    /// </param>
    /// <remarks>
    /// \if KO
    /// <para>UI 스레드에서 호출되어야 하며 기존 행을 유지하고 추가·갱신·삭제만 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Must run on the UI thread and preserves existing rows while applying only additions, updates, and removals.</para>
    /// \endif
    /// </remarks>
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

    /// <summary>
    /// \if KO
    /// <para>폴링 타이머를 정리하고 진행 중 콜백 종료를 최대 1초 동안 기다립니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disposes the polling timer and waits up to one second for an in-flight callback to finish.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>타이머 정리 및 대기 오류는 UI 종료를 방해하지 않도록 억제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Timer disposal and wait errors are suppressed so they cannot block UI shutdown.</para>
    /// \endif
    /// </remarks>
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

    /// <summary>
    /// \if KO
    /// <para>현재 선택된 스레드 행이 있는지 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Determines whether a thread row is currently selected.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>선택 항목이 있으면 <see langword="true"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> when a row is selected.</para>
    /// \endif
    /// </returns>
    private bool HasSelectedThread() => SelectedThread is not null;

    /// <summary>
    /// \if KO
    /// <para>선택된 스레드를 시작하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Starts the selected thread and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void StartSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Start(name);
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>선택된 스레드를 비동기 중지하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously stops the selected thread and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>선택 스레드 중지 및 새로 고침 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing selected-thread shutdown and refresh.</para>
    /// \endif
    /// </returns>
    private async Task StopSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        await _threadManager.StopAsync(name).ConfigureAwait(true);
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>선택된 스레드를 일시 정지하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Pauses the selected thread and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void PauseSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Pause(name);
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>선택된 스레드를 재개하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resumes the selected thread and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void ResumeSelectedThread()
    {
        var name = SelectedThread?.Name;
        if (name is null) return;

        _threadManager.Resume(name);
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>모든 스레드를 시작하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Starts all threads and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void StartAllThreads()
    {
        _threadManager.StartAll();
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>모든 스레드를 비동기 중지하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously stops all threads and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>전체 스레드 중지 및 새로 고침 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing all-thread shutdown and refresh.</para>
    /// \endif
    /// </returns>
    private async Task StopAllThreads()
    {
        await _threadManager.StopAllAsync().ConfigureAwait(true);
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>모든 스레드를 일시 정지하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Pauses all threads and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void PauseAllThreads()
    {
        _threadManager.PauseAll();
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>모든 스레드를 재개하고 모니터 데이터를 새로 고칩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resumes all threads and refreshes monitor data.</para>
    /// \endif
    /// </summary>
    private void ResumeAllThreads()
    {
        _threadManager.ResumeAll();
        Refresh();
    }

    /// <summary>
    /// \if KO
    /// <para>선택 스레드 제어 명령의 실행 가능 상태 변경을 알립니다.</para>
    /// \endif
    /// \if EN
    /// <para>Notifies selected-thread control commands that their can-execute state changed.</para>
    /// \endif
    /// </summary>
    private void RaiseCommandStates()
    {
        _startCommand.RaiseCanExecuteChanged();
        _stopCommand.RaiseCanExecuteChanged();
        _pauseCommand.RaiseCanExecuteChanged();
        _resumeCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 속성 이름으로 <see cref="PropertyChanged"/> 이벤트를 발생시킵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Raises <see cref="PropertyChanged"/> for the specified property name.</para>
    /// \endif
    /// </summary>
    /// <param name="propertyName">
    /// \if KO
    /// <para>변경된 속성 이름입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The changed property name.</para>
    /// \endif
    /// </param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
