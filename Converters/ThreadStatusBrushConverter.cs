using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.Converters;

/// <summary>
/// Converts Dreamine thread status values to WPF brushes.
/// </summary>
public sealed class ThreadStatusBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts a thread status value to a brush.
    /// </summary>
    /// <param name="value">The status value.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture information.</param>
    /// <returns>The converted brush.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not DreamineThreadStatus status)
        {
            return Brushes.Gray;
        }

        return status switch
        {
            DreamineThreadStatus.Running => Brushes.ForestGreen,
            DreamineThreadStatus.Paused => Brushes.DarkOrange,
            DreamineThreadStatus.Stopping => Brushes.OrangeRed,
            DreamineThreadStatus.Stopped => Brushes.Gray,
            DreamineThreadStatus.Faulted => Brushes.Crimson,
            DreamineThreadStatus.Disposed => Brushes.DimGray,
            _ => Brushes.SteelBlue
        };
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