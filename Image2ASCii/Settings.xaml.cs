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
using System.Windows.Shapes;

namespace Image2ASCii
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            RowSize.Text = "" + Properties.Settings.Default.RowSize;
            ColSize.Text = "" + Properties.Settings.Default.ColSize;
            ThreadSize.Text = "" + Properties.Settings.Default.ThreadSize;
            FontSize.Text = "" + Properties.Settings.Default.FontSize;
            if (Properties.Settings.Default.AllowMultithreading)
            {
                check.IsChecked = true;
            }
            else
            {
                check.IsChecked = false;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState!=WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.RowSize = SafeTransform.SafeStringToInt(RowSize.Text, 10);
            Properties.Settings.Default.ColSize = SafeTransform.SafeStringToInt(ColSize.Text, 5);
            Properties.Settings.Default.ThreadSize = SafeTransform.SafeStringToInt(ThreadSize.Text, 2);
            Properties.Settings.Default.FontSize = SafeTransform.SafeStringToInt(FontSize.Text, 4);
        }

        private void check_Click(object sender, RoutedEventArgs e)
        {
            if (check.IsChecked == true)
            {
                ThreadLabel.IsEnabled = true;
                ThreadSize.IsEnabled = true;
                Properties.Settings.Default.AllowMultithreading = true;
            }
            else
            {
                ThreadLabel.IsEnabled = false;
                ThreadSize.IsEnabled = false;
                Properties.Settings.Default.AllowMultithreading = false;
            }
        }
    }
}
