using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.ViewModels
{
    /// <summary>
    /// \if KO
    /// <para><see cref="DreamineThreadMonitorViewModel"/>이 사용하는 <see cref="DreamineThreadInfo"/> 스냅샷의 변경 가능하고 관찰 가능한 래퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A mutable, observable wrapper around <see cref="DreamineThreadInfo"/> snapshots used by <see cref="DreamineThreadMonitorViewModel"/>.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>불변 스냅샷 자체를 DataGrid에 바인딩하면 갱신마다 행을 교체해야 합니다. 이 래퍼는 스레드별 행 인스턴스를 유지하고 실제로 변경된 필드만 알리므로 선택과 행 컨테이너가 유지됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Binding immutable snapshots directly requires replacing rows on every refresh. This wrapper preserves one row per thread and notifies only changed fields, retaining grid selection and row containers.</para>
    /// \endif
    /// </remarks>
    public sealed class ThreadInfoRow : INotifyPropertyChanged
    {
        /// <summary>
        /// \if KO
        /// <para>status 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the status value.</para>
        /// \endif
        /// </summary>
        private DreamineThreadStatus _status;
        /// <summary>
        /// \if KO
        /// <para>priority 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the priority value.</para>
        /// \endif
        /// </summary>
        private DreamineThreadPriority _priority;
        /// <summary>
        /// \if KO
        /// <para>interval Ms 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the interval ms value.</para>
        /// \endif
        /// </summary>
        private int _intervalMs;
        /// <summary>
        /// \if KO
        /// <para>core Index 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the core index value.</para>
        /// \endif
        /// </summary>
        private int? _coreIndex;
        /// <summary>
        /// \if KO
        /// <para>use Affinity 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the use affinity value.</para>
        /// \endif
        /// </summary>
        private bool _useAffinity;
        /// <summary>
        /// \if KO
        /// <para>job Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the job count value.</para>
        /// \endif
        /// </summary>
        private int _jobCount;
        /// <summary>
        /// \if KO
        /// <para>cycle Count 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the cycle count value.</para>
        /// \endif
        /// </summary>
        private long _cycleCount;
        /// <summary>
        /// \if KO
        /// <para>started At 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the started at value.</para>
        /// \endif
        /// </summary>
        private DateTimeOffset? _startedAt;
        /// <summary>
        /// \if KO
        /// <para>stopped At 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the stopped at value.</para>
        /// \endif
        /// </summary>
        private DateTimeOffset? _stoppedAt;
        /// <summary>
        /// \if KO
        /// <para>last Error Message 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the last error message value.</para>
        /// \endif
        /// </summary>
        private string? _lastErrorMessage;

        /// <summary>
        /// \if KO
        /// <para>행 속성 값이 변경될 때 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when a row property value changes.</para>
        /// \endif
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// \if KO
        /// <para>생성 후 변경되지 않는 작업자 스레드 이름을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the worker-thread name, a stable identifier that never changes after construction.</para>
        /// \endif
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// \if KO
        /// <para>작업자 스레드 상태를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the worker-thread status.</para>
        /// \endif
        /// </summary>
        public DreamineThreadStatus Status
        {
            get => _status;
            private set => SetField(ref _status, value);
        }

        /// <summary>
        /// \if KO
        /// <para>작업자 스레드 우선순위를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the worker-thread priority.</para>
        /// \endif
        /// </summary>
        public DreamineThreadPriority Priority
        {
            get => _priority;
            private set => SetField(ref _priority, value);
        }

        /// <summary>
        /// \if KO
        /// <para>밀리초 단위 작업자 실행 간격을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the worker-thread interval in milliseconds.</para>
        /// \endif
        /// </summary>
        public int IntervalMs
        {
            get => _intervalMs;
            private set => SetField(ref _intervalMs, value);
        }

        /// <summary>
        /// \if KO
        /// <para>할당된 CPU 코어 인덱스를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the assigned CPU core index.</para>
        /// \endif
        /// </summary>
        public int? CoreIndex
        {
            get => _coreIndex;
            private set => SetField(ref _coreIndex, value);
        }

        /// <summary>
        /// \if KO
        /// <para>CPU 선호도 활성화 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether CPU affinity is enabled.</para>
        /// \endif
        /// </summary>
        public bool UseAffinity
        {
            get => _useAffinity;
            private set => SetField(ref _useAffinity, value);
        }

        /// <summary>
        /// \if KO
        /// <para>작업자에 할당된 작업 수를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the number of jobs assigned to the worker.</para>
        /// \endif
        /// </summary>
        public int JobCount
        {
            get => _jobCount;
            private set => SetField(ref _jobCount, value);
        }

        /// <summary>
        /// \if KO
        /// <para>완료된 실행 주기 수를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the number of completed cycles.</para>
        /// \endif
        /// </summary>
        public long CycleCount
        {
            get => _cycleCount;
            private set => SetField(ref _cycleCount, value);
        }

        /// <summary>
        /// \if KO
        /// <para>마지막 시작 시각을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the most recent start time.</para>
        /// \endif
        /// </summary>
        public DateTimeOffset? StartedAt
        {
            get => _startedAt;
            private set => SetField(ref _startedAt, value);
        }

        /// <summary>
        /// \if KO
        /// <para>마지막 중지 시각을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the most recent stop time.</para>
        /// \endif
        /// </summary>
        public DateTimeOffset? StoppedAt
        {
            get => _stoppedAt;
            private set => SetField(ref _stoppedAt, value);
        }

        /// <summary>
        /// \if KO
        /// <para>마지막 예외 메시지를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the most recent exception message.</para>
        /// \endif
        /// </summary>
        public string? LastErrorMessage
        {
            get => _lastErrorMessage;
            private set => SetField(ref _lastErrorMessage, value);
        }

        /// <summary>
        /// \if KO
        /// <para>작업자가 현재 실행 중인지 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether the worker is currently running, for binding convenience.</para>
        /// \endif
        /// </summary>
        public bool IsRunning => Status == DreamineThreadStatus.Running;

        /// <summary>
        /// \if KO
        /// <para>작업자가 현재 일시 정지되었는지 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether the worker is currently paused.</para>
        /// \endif
        /// </summary>
        public bool IsPaused => Status == DreamineThreadStatus.Paused;

        /// <summary>
        /// \if KO
        /// <para>작업자가 오류 상태인지 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether the worker is in a faulted state.</para>
        /// \endif
        /// </summary>
        public bool IsFaulted => Status == DreamineThreadStatus.Faulted;

        /// <summary>
        /// \if KO
        /// <para>스레드 정보 스냅샷에서 <see cref="T:Dreamine.Threading.Wpf.ViewModels.ThreadInfoRow" /> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.ViewModels.ThreadInfoRow" /> from a thread-information snapshot.</para>
        /// \endif
        /// </summary>
        /// <param name="info">
        /// \if KO
        /// <para>초기 값을 제공할 스레드 정보 스냅샷입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The thread-information snapshot providing initial values.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="info"/>가 <see langword="null"/>일 때 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="info"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public ThreadInfoRow(DreamineThreadInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            Name = info.Name;
            _status = info.Status;
            _priority = info.Priority;
            _intervalMs = info.IntervalMs;
            _coreIndex = info.CoreIndex;
            _useAffinity = info.UseAffinity;
            _jobCount = info.JobCount;
            _cycleCount = info.CycleCount;
            _startedAt = info.StartedAt;
            _stoppedAt = info.StoppedAt;
            _lastErrorMessage = info.LastErrorMessage;
        }

        /// <summary>
        /// \if KO
        /// <para>새 스냅샷에서 행을 갱신하고 실제 값이 변경된 속성에만 알림을 발생시킵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Updates the row from a new snapshot and raises notifications only for properties whose values changed.</para>
        /// \endif
        /// </summary>
        /// <param name="info">
        /// \if KO
        /// <para>적용할 새 스레드 정보 스냅샷입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The new thread-information snapshot to apply.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>하나 이상의 속성이 변경되었으면 <see langword="true"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when at least one property changed.</para>
        /// \endif
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="info"/>가 <see langword="null"/>일 때 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="info"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public bool UpdateFrom(DreamineThreadInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            var statusChanged = _status != info.Status;
            var anyChanged = false;

            anyChanged |= SetField(ref _status, info.Status, nameof(Status));
            anyChanged |= SetField(ref _priority, info.Priority, nameof(Priority));
            anyChanged |= SetField(ref _intervalMs, info.IntervalMs, nameof(IntervalMs));
            anyChanged |= SetField(ref _coreIndex, info.CoreIndex, nameof(CoreIndex));
            anyChanged |= SetField(ref _useAffinity, info.UseAffinity, nameof(UseAffinity));
            anyChanged |= SetField(ref _jobCount, info.JobCount, nameof(JobCount));
            anyChanged |= SetField(ref _cycleCount, info.CycleCount, nameof(CycleCount));
            anyChanged |= SetField(ref _startedAt, info.StartedAt, nameof(StartedAt));
            anyChanged |= SetField(ref _stoppedAt, info.StoppedAt, nameof(StoppedAt));
            anyChanged |= SetField(ref _lastErrorMessage, info.LastErrorMessage, nameof(LastErrorMessage));

            if (statusChanged)
            {
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(IsPaused));
                OnPropertyChanged(nameof(IsFaulted));
            }

            return anyChanged;
        }

        /// <summary>
        /// \if KO
        /// <para>필드 값이 달라진 경우 값을 갱신하고 속성 변경 알림을 발생시킵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Updates a field and raises property-change notification when the value differs.</para>
        /// \endif
        /// </summary>
        /// <typeparam name="T">
        /// \if KO
        /// <para>필드 값 형식입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The field-value type.</para>
        /// \endif
        /// </typeparam>
        /// <param name="field">
        /// \if KO
        /// <para>갱신할 필드 참조입니다.</para>
        /// \endif
        /// \if EN
        /// <para>A reference to the field to update.</para>
        /// \endif
        /// </param>
        /// <param name="value">
        /// \if KO
        /// <para>새 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The new value.</para>
        /// \endif
        /// </param>
        /// <param name="propertyName">
        /// \if KO
        /// <para>알림에 사용할 속성 이름입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The property name used for notification.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>값이 변경되었으면 <see langword="true"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when the value changed.</para>
        /// \endif
        /// </returns>
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
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
        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
