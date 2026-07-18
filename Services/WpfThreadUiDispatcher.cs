using System;
using System.Windows;
using System.Windows.Threading;

namespace Dreamine.Threading.Wpf.Services;

/// <summary>
/// \if KO
/// <para>Dreamine 스레딩 WPF 구성 요소에 UI 스레드 디스패치를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides UI-thread dispatching for Dreamine threading WPF components.</para>
/// \endif
/// </summary>
public sealed class WpfThreadUiDispatcher : IThreadUiDispatcher
{
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
    /// <para>기본 WPF <see cref="P:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher.Dispatcher" />를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the underlying WPF <see cref="P:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher.Dispatcher" />.</para>
    /// \endif
    /// </summary>
    public Dispatcher Dispatcher => _dispatcher;

    /// <summary>
    /// \if KO
    /// <para>현재 애플리케이션 Dispatcher 또는 현재 스레드 Dispatcher로 <see cref="T:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher" /> using the current application or thread dispatcher.</para>
    /// \endif
    /// </summary>
    public WpfThreadUiDispatcher()
        : this(Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 Dispatcher로 <see cref="T:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.Threading.Wpf.Services.WpfThreadUiDispatcher" /> with the specified dispatcher.</para>
    /// \endif
    /// </summary>
    /// <param name="dispatcher">
    /// \if KO
    /// <para>UI 작업을 실행할 WPF Dispatcher입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The WPF dispatcher on which UI work executes.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="dispatcher"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="dispatcher"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public WpfThreadUiDispatcher(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// \if KO
    /// <para>이미 UI 스레드이면 즉시, 아니면 Dispatcher를 통해 지정한 동작을 동기 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Executes the action immediately on the UI thread, or synchronously through the dispatcher otherwise.</para>
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
    /// <para><paramref name="action"/>이 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="action"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>이미 UI 스레드이면 즉시, 아니면 <see cref="F:System.Windows.Threading.DispatcherPriority.Background" /> 우선순위로 동작을 예약합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Executes immediately on the UI thread, or schedules the action at <see cref="F:System.Windows.Threading.DispatcherPriority.Background" /> priority otherwise.</para>
    /// \endif
    /// </summary>
    /// <param name="action">
    /// \if KO
    /// <para>실행하거나 예약할 동작입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The action to execute or schedule.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="action"/>이 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="action"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
