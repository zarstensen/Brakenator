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
using System.IO;

namespace Brakenator
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        MainWindow mainWindow;
        public Page1(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            InitializeComponent();
            
            number.Text = Directory.GetCurrentDirectory();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
