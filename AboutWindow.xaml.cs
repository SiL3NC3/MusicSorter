using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicSorter
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            SetProductVersion();

        }
        private void SetProductVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            LabelVersion.Content = $"v{v.Major}.{v.Minor}";
#if (DEBUG)
            LabelVersion.Content += " [DEBUG]";
#endif
        }
    }
}
