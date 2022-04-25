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

using System.Timers;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace Brakenator
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        short currentWeather;
        int pageNumber = 1;

        const string DAY_CLOUDY_PATH = @"\resources\day_cloudy.png";
        const string RAIN_PATH = @"\resources\rain.png";
        const string SNOW_PATH = @"\resources\snow.png";

        public MainWindow()
        {
            BN.setWeatherKey(@"C:\Users\kamjo\Documents\skole\PROG\Afleveringer\Brakenator\build\BrakenatorUI\Debug\weather_key.txt");
            InitializeComponent();
            main_frame.Content = new Page1(this);
            InitTimer();
            
        }
        public static Point GetMousePositionWindowsForms()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        private System.Timers.Timer timer;
        Point startPoint, endPoint;

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = GetMousePositionWindowsForms();
        }
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            endPoint = GetMousePositionWindowsForms();
            if (Math.Abs(endPoint.X - startPoint.X) > main_frame.ActualWidth / 10)
            {
                bool direction = startPoint.X - endPoint.X > 0;
                changePage(direction);
            }
            
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {

            var timeDate = DateTime.Now;
            string hour = timeDate.Hour.ToString().PadLeft(2, '0');
            string minute = timeDate.Minute.ToString().PadLeft(2, '0');
            string time = hour + ":" + minute;


            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { clock.Text = time; } ));


            //BN.autoWeather(55.777960, 12.527173);
            BN.autoWeather(49.08939117171668, 12.90991949851313);
            currentWeather = (short)BN.getWeather();
            string currentWeatherPath;
            
            switch (currentWeather)
            {
                case 0:
                    currentWeatherPath = DAY_CLOUDY_PATH;

                    break;
                case 1:
                    currentWeatherPath = RAIN_PATH;

                    break;
                case 2:
                    currentWeatherPath = SNOW_PATH;
                    
                    break;
                default:
                    currentWeatherPath = "";
                    break;
            }
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { weatherIcon.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + currentWeatherPath)); }));

            //change selected elipse for page
        }

        public void InitTimer()
        {
            //make timer
            timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += OnTimedEvent;
        }

        void changePage(bool direction)
        {
            if (direction)
            {
                if (pageNumber == 1)
                {
                    main_frame.Content = new Page2(this);
                    pageNumber = 2;
                    ball2.Fill = new SolidColorBrush(Color.FromRgb(190, 190, 190));
                    ball1.Fill = new SolidColorBrush();
                }
            }
            else
            {
                if (pageNumber == 2)
                {
                    main_frame.Content = new Page1(this);
                    pageNumber = 1;
                    ball1.Fill = new SolidColorBrush(Color.FromRgb(190, 190, 190));
                    ball2.Fill = new SolidColorBrush();
                }
            }
        }



        //changes font size everytime windows is resized
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            const double FONT_WEIGHT = .05;
            double font_size = main_frame.ActualWidth * FONT_WEIGHT;
            double screen_size = main_frame.ActualWidth;
            brakingDistance.FontSize = font_size;
            clock.FontSize = font_size;
            ball1.Width = screen_size / 14;
            ball2.Width = screen_size / 14;
            ball1.Height = screen_size / 14;
            ball2.Height = screen_size / 14;
        }
    }
}
