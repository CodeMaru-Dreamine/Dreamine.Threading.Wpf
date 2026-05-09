namespace Dreamine.Threading.Wpf.Options
{
    /// <summary>
    /// Provides configuration options for Dreamine WPF threading registration.
    /// </summary>
    public sealed class DreamineThreadingWpfOptions
    {
        /// <summary>
        /// Gets or sets whether Windows-specific threading services are registered.
        /// </summary>
        public bool RegisterWindowsServices { get; set; } = true;

        /// <summary>
        /// Gets or sets whether adaptive CPU cycle policy is used.
        /// </summary>
        public bool UseAdaptiveCpuPolicy { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the WPF thread monitor ViewModel is registered.
        /// </summary>
        public bool RegisterThreadMonitor { get; set; } = true;

        /// <summary>
        /// Gets or sets whether existing registrations can be overwritten.
        /// </summary>
        public bool AllowOverride { get; set; } = true;
    }
}
