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
/// Provides registration helpers for Dreamine WPF threading services.
/// </summary>
public static class DreamineThreadingWpfRegistration
{
    /// <summary>
    /// Registers Dreamine threading services for WPF applications.
    /// </summary>
    /// <param name="configure">The optional WPF threading configuration action.</param>
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

    private static void RegisterThreadMonitor()
    {
        DMContainer.RegisterSingleton<WpfThreadUiDispatcher>(
            new WpfThreadUiDispatcher());

        DMContainer.Register<DreamineThreadMonitorViewModel>(() =>
            new DreamineThreadMonitorViewModel(
                DMContainer.Resolve<Dreamine.Threading.Interfaces.IDreamineThreadManager>(),
                DMContainer.Resolve<WpfThreadUiDispatcher>()));
    }
}