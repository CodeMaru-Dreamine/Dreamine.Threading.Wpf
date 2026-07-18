using System;
using System.Globalization;
using System.Windows.Data;
using Dreamine.Threading.Models;

namespace Dreamine.Threading.Wpf.Converters;

/// <summary>
/// \if KO
/// <para>Dreamine 스레드 우선순위 값을 표시 텍스트로 변환합니다.</para>
/// \endif
/// \if EN
/// <para>Converts Dreamine thread-priority values to display text.</para>
/// \endif
/// </summary>
public sealed class ThreadPriorityTextConverter : IValueConverter
{
    /// <summary>
    /// \if KO
    /// <para>스레드 우선순위 값을 열거형 이름 텍스트로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a thread-priority value to its enumeration-name text.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>변환할 우선순위 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The priority value to convert.</para>
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
    /// <para>우선순위 이름이며 값 형식이 다르면 빈 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The priority name, or an empty string when the value has another type.</para>
    /// \endif
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DreamineThreadPriority priority
            ? priority.ToString()
            : string.Empty;
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
