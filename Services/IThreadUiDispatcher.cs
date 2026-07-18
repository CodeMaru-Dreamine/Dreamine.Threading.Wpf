using System;
using System.Windows.Threading;

namespace Dreamine.Threading.Wpf.Services;

/// <summary>
/// \if KO
/// <para>스레드 모니터 ViewModel의 UI 스레드 디스패치를 추상화합니다.</para>
/// \endif
/// \if EN
/// <para>Abstracts UI-thread dispatching for thread-monitor view models.</para>
/// \endif
/// </summary>
public interface IThreadUiDispatcher
{
    /// <summary>
    /// \if KO
    /// <para>일괄 UI 갱신에 사용하는 기본 Dispatcher를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the underlying dispatcher used by batched UI updates.</para>
    /// \endif
    /// </summary>
    Dispatcher Dispatcher { get; }

    /// <summary>
    /// \if KO
    /// <para>지정한 동작을 UI 스레드에서 동기 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Executes the specified action synchronously on the UI thread.</para>
    /// \endif
    /// </summary>
    /// <param name="action">
    /// \if KO
    /// <para>실행할 동작입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The action to execute.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="action"/>이 <see langword="null"/>이면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when <paramref name="action"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    void Invoke(Action action);

    /// <summary>
    /// \if KO
    /// <para>지정한 동작을 UI 스레드에서 비동기 예약합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Schedules the specified action asynchronously on the UI thread.</para>
    /// \endif
    /// </summary>
    /// <param name="action">
    /// \if KO
    /// <para>예약할 동작입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The action to schedule.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="action"/>이 <see langword="null"/>이면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when <paramref name="action"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    void BeginInvoke(Action action);
}
