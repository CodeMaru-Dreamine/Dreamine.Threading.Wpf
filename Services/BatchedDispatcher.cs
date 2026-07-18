using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace Dreamine.Threading.Wpf.Services
{
    /// <summary>
    /// \if KO
    /// <para>빈번한 UI 갱신을 WPF UI 스레드에서 처리하는 일괄 작업으로 병합합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Coalesces high-frequency UI updates into batches dispatched on the WPF UI thread.</para>
    /// \endif
    /// </summary>
    /// <remarks>
    /// \if KO
    /// <para>생산자는 어느 스레드에서나 <see cref="Enqueue"/>를 호출할 수 있습니다. 동시에 하나의 <see cref="DispatcherOperation"/>만 예약하며 한 번에 최대 <see cref="MaxBatchSize"/>개를 처리하고 나머지는 다음 패스로 넘깁니다. WPF 패키지 간 의존성을 피하기 위해 Logging.Wpf 구성 요소를 내부에 반영한 구현입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Producers may call <see cref="Enqueue"/> from any thread. Only one <see cref="DispatcherOperation"/> is pending at a time; each pass processes up to <see cref="MaxBatchSize"/> items and defers overflow to the next pass. This internal mirror of the Logging.Wpf component avoids an inter-WPF-package dependency.</para>
    /// \endif
    /// </remarks>
    /// <typeparam name="T">
    /// \if KO
    /// <para>일괄 전달할 항목 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The item type delivered in batches.</para>
    /// \endif
    /// </typeparam>
    internal sealed class BatchedDispatcher<T>
    {
        /// <summary>
        /// \if KO
        /// <para>한 UI 스레드 일괄 작업에서 처리할 최대 항목 수입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The maximum number of items processed in one UI-thread batch.</para>
        /// \endif
        /// </summary>
        public const int MaxBatchSize = 256;

        /// <summary>
        /// \if KO
        /// <para>dispatcher 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the dispatcher value.</para>
        /// \endif
        /// </summary>
        private readonly Dispatcher _dispatcher;
        /// <summary>
        /// \if KO
        /// <para>on Batch 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the on batch value.</para>
        /// \endif
        /// </summary>
        private readonly Action<IReadOnlyList<T>> _onBatch;
        /// <summary>
        /// \if KO
        /// <para>priority 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the priority value.</para>
        /// \endif
        /// </summary>
        private readonly DispatcherPriority _priority;
        /// <summary>
        /// \if KO
        /// <para>pending 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the pending value.</para>
        /// \endif
        /// </summary>
        private readonly ConcurrentQueue<T> _pending = new();
        /// <summary>
        /// \if KO
        /// <para>scheduled 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the scheduled value.</para>
        /// \endif
        /// </summary>
        private int _scheduled;

        /// <summary>
        /// \if KO
        /// <para>대상 Dispatcher와 일괄 콜백으로 <see cref="T:Dreamine.Threading.Wpf.Services.BatchedDispatcher`1" /> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.Services.BatchedDispatcher`1" /> with a target dispatcher and batch callback.</para>
        /// \endif
        /// </summary>
        /// <param name="dispatcher">
        /// \if KO
        /// <para>대상 WPF Dispatcher입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The target WPF dispatcher.</para>
        /// \endif
        /// </param>
        /// <param name="onBatch">
        /// \if KO
        /// <para>배출된 항목 목록과 함께 UI 스레드에서 호출할 콜백입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The callback invoked on the UI thread with the drained batch.</para>
        /// \endif
        /// </param>
        /// <param name="priority">
        /// \if KO
        /// <para>Dispatcher 우선순위이며 기본값은 <see cref="DispatcherPriority.Background"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The dispatcher priority; the default is <see cref="DispatcherPriority.Background"/>.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para><paramref name="dispatcher"/> 또는 <paramref name="onBatch"/>가 <see langword="null"/>일 때 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when <paramref name="dispatcher"/> or <paramref name="onBatch"/> is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public BatchedDispatcher(
            Dispatcher dispatcher,
            Action<IReadOnlyList<T>> onBatch,
            DispatcherPriority priority = DispatcherPriority.Background)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _onBatch = onBatch ?? throw new ArgumentNullException(nameof(onBatch));
            _priority = priority;
        }

        /// <summary>
        /// \if KO
        /// <para>UI 일괄 전달을 위해 항목을 큐에 넣고 필요하면 배출 작업을 예약합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Enqueues an item for batched UI delivery and schedules a flush when needed.</para>
        /// \endif
        /// </summary>
        /// <param name="item">
        /// \if KO
        /// <para>전달할 항목입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The item to deliver.</para>
        /// \endif
        /// </param>
        public void Enqueue(T item)
        {
            _pending.Enqueue(item);
            ScheduleFlushIfNeeded();
        }

        /// <summary>
        /// \if KO
        /// <para>아직 배출 작업이 예약되지 않은 경우 Dispatcher 콜백 하나를 원자적으로 예약합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Atomically schedules one dispatcher callback when no flush is already pending.</para>
        /// \endif
        /// </summary>
        private void ScheduleFlushIfNeeded()
        {
            if (Interlocked.CompareExchange(ref _scheduled, 1, 0) != 0)
            {
                return;
            }

            _dispatcher.BeginInvoke(_priority, new Action(Flush));
        }

        /// <summary>
        /// \if KO
        /// <para>대기 큐에서 최대 일괄 크기만큼 항목을 배출하고 콜백에 전달한 뒤 남은 항목을 다시 예약합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Drains up to one batch from the pending queue, invokes the callback, and reschedules remaining items.</para>
        /// \endif
        /// </summary>
        /// <remarks>
        /// \if KO
        /// <para>일괄 콜백 예외가 발생해도 예약 플래그는 finally에서 초기화됩니다.</para>
        /// \endif
        /// \if EN
        /// <para>The scheduled flag is reset in a finally block even when the batch callback throws.</para>
        /// \endif
        /// </remarks>
        private void Flush()
        {
            try
            {
                var buffer = new List<T>(Math.Min(_pending.Count, MaxBatchSize));

                while (buffer.Count < MaxBatchSize && _pending.TryDequeue(out var item))
                {
                    buffer.Add(item);
                }

                if (buffer.Count > 0)
                {
                    _onBatch(buffer);
                }
            }
            finally
            {
                Volatile.Write(ref _scheduled, 0);

                if (!_pending.IsEmpty)
                {
                    ScheduleFlushIfNeeded();
                }
            }
        }
    }
}
