using System;
using Dreamine.MVVM.Core;
using Dreamine.Threading.Options;
using Dreamine.Threading.Registration;
using Dreamine.Threading.Windows.Registration;
using Dreamine.Threading.Wpf.Options;
using Dreamine.Threading.Wpf.Services;
using Dreamine.Threading.Wpf.ViewModels;

namespace Dreamine.Threading.Wpf.Registration;

/// <summary>
/// \if KO
/// <para>Dreamine WPF 스레딩 서비스를 등록하는 도우미를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides helpers for registering Dreamine WPF threading services.</para>
/// \endif
/// </summary>
public static class DreamineThreadingWpfRegistration
{
    /// <summary>
    /// \if KO
    /// <para>선택적 구성에 따라 Windows·핵심·모니터 WPF 스레딩 서비스를 등록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Registers Windows, core, and monitor WPF threading services according to optional configuration.</para>
    /// \endif
    /// </summary>
    /// <param name="configure">
    /// \if KO
    /// <para>WPF 스레딩 등록 옵션을 수정할 선택적 대리자입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The optional delegate that modifies WPF threading registration options.</para>
    /// \endif
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>컨테이너 서비스 등록 또는 확인이 실패할 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when container service registration or resolution fails.</para>
    /// \endif
    /// </exception>
    public static void Register(Action<DreamineThreadingWpfOptions>? configure = null)
    {
        var options = new DreamineThreadingWpfOptions();
        configure?.Invoke(options);

        if (options.RegisterWindowsServices)
        {
            DreamineThreadingWindowsRegistration.Register();
        }

        DreamineThreadingRegistration.Register(core =>
        {
            core.UseAdaptiveCpuPolicy = options.UseAdaptiveCpuPolicy;
            core.AllowOverride = options.AllowOverride;
        });

        if (options.RegisterThreadMonitor)
        {
            RegisterThreadMonitor();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>WPF UI Dispatcher와 스레드 모니터 ViewModel 팩터리를 전역 컨테이너에 등록합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Registers the WPF UI dispatcher and thread-monitor view-model factory in the global container.</para>
    /// \endif
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>필수 서비스 등록 또는 확인이 실패할 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when required service registration or resolution fails.</para>
    /// \endif
    /// </exception>
    private static void RegisterThreadMonitor()
    {
        DMContainer.RegisterSingleton<WpfThreadUiDispatcher>(
            new WpfThreadUiDispatcher());
        DMContainer.RegisterSingleton<IThreadUiDispatcher>(
            DMContainer.Resolve<WpfThreadUiDispatcher>());

        DMContainer.Register<DreamineThreadMonitorViewModel>(() =>
            new DreamineThreadMonitorViewModel(
                DMContainer.Resolve<Dreamine.Threading.Interfaces.IDreamineThreadManager>(),
                DMContainer.Resolve<IThreadUiDispatcher>()));
    }
}
