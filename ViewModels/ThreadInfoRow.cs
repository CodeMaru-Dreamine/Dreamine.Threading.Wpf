using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.ViewModels
{
    /// <summary>
    /// Mutable, observable wrapper around <see cref="DreamineThreadInfo"/>
    /// snapshots used by <see cref="DreamineThreadMonitorViewModel"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="DreamineThreadInfo"/> is an immutable snapshot, so binding
    /// the raw type to a <c>DataGrid</c> requires replacing the row object on
    /// every refresh, which causes selection flicker and forced row re-render.
    /// This wrapper is a stable per-thread row that raises <see cref="PropertyChanged"/>
    /// only for fields that actually changed, so the grid keeps its row
    /// containers and selection across refreshes.
    /// </remarks>
    public sealed class ThreadInfoRow : INotifyPropertyChanged
    {
        private DreamineThreadStatus _status;
        private DreamineThreadPriority _priority;
        private int _intervalMs;
        private int? _coreIndex;
        private bool _useAffinity;
        private int _jobCount;
        private long _cycleCount;
        private DateTimeOffset? _startedAt;
        private DateTimeOffset? _stoppedAt;
        private string? _lastErrorMessage;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the worker thread name. Stable identifier; never changes after construction.
        /// </summary>
        public string Name { get; }

        /// <summary>Gets the worker thread status.</summary>
        public DreamineThreadStatus Status
        {
            get => _status;
            private set => SetField(ref _status, value);
        }

        /// <summary>Gets the worker thread priority.</summary>
        public DreamineThreadPriority Priority
        {
            get => _priority;
            private set => SetField(ref _priority, value);
        }

        /// <summary>Gets the worker thread interval in milliseconds.</summary>
        public int IntervalMs
        {
            get => _intervalMs;
            private set => SetField(ref _intervalMs, value);
        }

        /// <summary>Gets the assigned CPU core index.</summary>
        public int? CoreIndex
        {
            get => _coreIndex;
            private set => SetField(ref _coreIndex, value);
        }

        /// <summary>Gets a value indicating whether CPU affinity is enabled.</summary>
        public bool UseAffinity
        {
            get => _useAffinity;
            private set => SetField(ref _useAffinity, value);
        }

        /// <summary>Gets the number of jobs assigned to the worker.</summary>
        public int JobCount
        {
            get => _jobCount;
            private set => SetField(ref _jobCount, value);
        }

        /// <summary>Gets the number of completed cycles.</summary>
        public long CycleCount
        {
            get => _cycleCount;
            private set => SetField(ref _cycleCount, value);
        }

        /// <summary>Gets the last started time.</summary>
        public DateTimeOffset? StartedAt
        {
            get => _startedAt;
            private set => SetField(ref _startedAt, value);
        }

        /// <summary>Gets the last stopped time.</summary>
        public DateTimeOffset? StoppedAt
        {
            get => _stoppedAt;
            private set => SetField(ref _stoppedAt, value);
        }

        /// <summary>Gets the last exception message.</summary>
        public string? LastErrorMessage
        {
            get => _lastErrorMessage;
            private set => SetField(ref _lastErrorMessage, value);
        }

        /// <summary>
        /// Gets a value indicating whether the worker is currently running.
        /// Convenience flag for binding (e.g. button enable state).
        /// </summary>
        public bool IsRunning => Status == DreamineThreadStatus.Running;

        /// <summary>
        /// Gets a value indicating whether the worker is currently paused.
        /// </summary>
        public bool IsPaused => Status == DreamineThreadStatus.Paused;

        /// <summary>
        /// Gets a value indicating whether the worker is in a faulted state.
        /// </summary>
        public bool IsFaulted => Status == DreamineThreadStatus.Faulted;

        /// <summary>
        /// Initializes a new instance of <see cref="ThreadInfoRow"/> from a snapshot.
        /// </summary>
        /// <param name="info">The snapshot.</param>
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
        /// Updates the row from a new snapshot, raising <see cref="PropertyChanged"/>
        /// only for fields whose values changed.
        /// </summary>
        /// <param name="info">The new snapshot.</param>
        /// <returns>True if any property changed.</returns>
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

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
