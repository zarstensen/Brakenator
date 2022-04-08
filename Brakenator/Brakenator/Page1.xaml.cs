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

using System.Runtime.InteropServices;

namespace Brakenator
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        [DllImport(@"C:\Users\kamjo\Documents\skole\PROG\Afleveringer\Brakenator\Brakenator\x64\Debug\BNBackend.dll")]
        private static extern int printStuff(string str, int repeat);

        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        public Page1()
        {
            AllocConsole();
            int i = printStuff("Hej", 123);
            InitializeComponent();
            number.Text = i.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
