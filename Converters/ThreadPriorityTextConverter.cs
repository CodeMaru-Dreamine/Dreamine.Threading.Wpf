using System;
using System.Globalization;
using System.Windows.Data;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.Converters;

/// <summary>
/// Converts Dreamine thread priority values to display text.
/// </summary>
public sealed class ThreadPriorityTextConverter : IValueConverter
{
    /// <summary>
    /// Converts a thread priority value to display text.
    /// </summary>
    /// <param name="value">The priority value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>The converted display text.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DreamineThreadPriority priority
            ? priority.ToString()
            : string.Empty;
    }

    /// <summary>
    /// ConvertBack is not supported.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>Nothing.</returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}