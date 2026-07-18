using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.Converters;

/// <summary>
/// \if KO
/// <para>Dreamine 스레드 상태 값을 WPF 브러시로 변환합니다.</para>
/// \endif
/// \if EN
/// <para>Converts Dreamine thread-status values to WPF brushes.</para>
/// \endif
/// </summary>
public sealed class ThreadStatusBrushConverter : IValueConverter
{
    /// <summary>
    /// \if KO
    /// <para>스레드 상태 값을 상태별 WPF 브러시로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a thread-status value to its state-specific WPF brush.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>변환할 상태 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The status value to convert.</para>
    /// \endif
    /// </param>
    /// <param name="targetType">
    /// \if KO
    /// <para>바인딩 대상 형식이며 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The binding target type, which is ignored.</para>
    /// \endif
    /// </param>
    /// <param name="parameter">
    /// \if KO
    /// <para>변환 매개변수이며 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The converter parameter, which is ignored.</para>
    /// \endif
    /// </param>
    /// <param name="culture">
    /// \if KO
    /// <para>문화권 정보이며 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The culture information, which is ignored.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>상태별 브러시이며 알 수 없는 값은 회색입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The state-specific brush, or gray for an unrecognized value.</para>
    /// \endif
    /// </returns>
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
    /// \if KO
    /// <para>역변환을 지원하지 않고 <see cref="F:System.Windows.Data.Binding.DoNothing" />을 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Does not support conversion back and returns <see cref="F:System.Windows.Data.Binding.DoNothing" />.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>사용하지 않는 원본 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unused source value.</para>
    /// \endif
    /// </param>
    /// <param name="targetType">
    /// \if KO
    /// <para>사용하지 않는 대상 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unused target type.</para>
    /// \endif
    /// </param>
    /// <param name="parameter">
    /// \if KO
    /// <para>사용하지 않는 변환 매개변수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unused converter parameter.</para>
    /// \endif
    /// </param>
    /// <param name="culture">
    /// \if KO
    /// <para>사용하지 않는 문화권입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unused culture.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>항상 <see cref="Binding.DoNothing"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always <see cref="Binding.DoNothing"/>.</para>
    /// \endif
    /// </returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
