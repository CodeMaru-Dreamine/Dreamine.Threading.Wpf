using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Dreamine.Threading.Models;
using Dreamine.Threading.Wpf.Converters;
using Dreamine.Threading.Wpf.Options;

namespace Dreamine.Threading.Wpf.Tests;

public sealed class ConverterAndOptionsTests
{
    [Theory]
    [InlineData(DreamineThreadPriority.Low)]
    [InlineData(DreamineThreadPriority.Normal)]
    [InlineData(DreamineThreadPriority.High)]
    public void PriorityConverterReturnsEnumName(DreamineThreadPriority priority)
    {
        var converter = new ThreadPriorityTextConverter();

        Assert.Equal(priority.ToString(), converter.Convert(priority, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Same(Binding.DoNothing, converter.ConvertBack("", typeof(object), null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PriorityConverterReturnsEmptyForUnknownValue()
    {
        var converter = new ThreadPriorityTextConverter();
        Assert.Equal(string.Empty, converter.Convert(new object(), typeof(string), null!, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(DreamineThreadStatus.Running, "ForestGreen")]
    [InlineData(DreamineThreadStatus.Paused, "DarkOrange")]
    [InlineData(DreamineThreadStatus.Faulted, "Crimson")]
    [InlineData(DreamineThreadStatus.Stopping, "OrangeRed")]
    [InlineData(DreamineThreadStatus.Disposed, "DimGray")]
    [InlineData(DreamineThreadStatus.Stopped, "Gray")]
    public void StatusConverterReturnsExpectedBrush(DreamineThreadStatus status, string expected)
    {
        var converter = new ThreadStatusBrushConverter();
        var brush = Assert.IsType<SolidColorBrush>(
            converter.Convert(status, typeof(Brush), null!, CultureInfo.InvariantCulture));

        Assert.Equal(((SolidColorBrush)new BrushConverter().ConvertFromString(expected)!).Color, brush.Color);
        Assert.Same(Binding.DoNothing, converter.ConvertBack("", typeof(object), null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void StatusConverterUsesGrayForUnknownValue()
    {
        var converter = new ThreadStatusBrushConverter();
        Assert.Same(Brushes.Gray, converter.Convert("unknown", typeof(Brush), null!, CultureInfo.InvariantCulture));
        Assert.Same(
            Brushes.SteelBlue,
            converter.Convert((DreamineThreadStatus)int.MaxValue, typeof(Brush), null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void OptionsHaveSafeDefaults()
    {
        var options = new DreamineThreadingWpfOptions();

        Assert.True(options.RegisterWindowsServices);
        Assert.True(options.UseAdaptiveCpuPolicy);
        Assert.True(options.RegisterThreadMonitor);
        Assert.True(options.AllowOverride);
    }
}
