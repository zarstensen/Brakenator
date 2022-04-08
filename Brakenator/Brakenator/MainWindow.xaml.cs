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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        int pageNumber = 1;

        public MainWindow()
        {
            InitializeComponent();
            main_frame.Content = new Page1();

        }

        private void page1Button_Click(object sender, RoutedEventArgs e)
        {
            if (pageNumber != 1)
            {
                main_frame.Content = new Page1();
            }

            pageNumber = 1;
        }

        private void page2Button_Click(object sender, RoutedEventArgs e)
        {
            if (pageNumber != 2)
            {
                main_frame.Content = new Page2();
            }
            pageNumber = 2;
        }
    }
}
