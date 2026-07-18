using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dreamine.Threading.Wpf.Views
{
    /// <summary>
    /// \if KO
    /// <para><c>DreamineThreadMonitorView.xaml</c>의 WPF 상호 작용 논리를 제공합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Provides WPF interaction logic for <c>DreamineThreadMonitorView.xaml</c>.</para>
    /// \endif
    /// </summary>
    public partial class DreamineThreadMonitorView : UserControl
    {
        /// <summary>
        /// \if KO
        /// <para>XAML 구성 요소를 로드해 <see cref="DreamineThreadMonitorView"/> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of <see cref="DreamineThreadMonitorView"/> by loading its XAML components.</para>
        /// \endif
        /// </summary>
        public DreamineThreadMonitorView()
        {
            InitializeComponent();
        }
    }
}
