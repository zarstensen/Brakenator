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
using System.Timers;


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
            InitTimer();
        }

        private Timer timer;



        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            var timeDate = DateTime.Now;
            int hour = timeDate.Hour;
            int minute = timeDate.Minute;
            string time;

            time = hour + ":" + minute;


            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { clock.Text = time; } ));
        }

        public void InitTimer()
        {
            //make timer
            timer = new Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += OnTimedEvent;
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            const double FONT_WEIGHT = .05;
            double font_size = main_frame.ActualWidth * FONT_WEIGHT;
            brakingDistance.FontSize = font_size;
            clock.FontSize = font_size;
        }
    }
}
