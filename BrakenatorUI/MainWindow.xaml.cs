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

namespace Brakenator
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        short currentWeather;
        int pageNumber = 1;
        Page1 page1;
        Page2 page2;

        const string ROAD_SUN = "road_sun";
        const string ROAD_RAIN = "road_rain";
        const string ROAD_SNOW = "road_snow";


        public MainWindow()
        {
            page1 = new Page1(this);
            page2 = new Page2(this);
            BN.setWeatherKey(Directory.GetCurrentDirectory() + @"\weather_key.txt");
            InitializeComponent();
            main_frame.Content = page1;
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
            short return_code = BN.autoWeather(51.142622, 9.493477);
            currentWeather = (short)BN.getWeather();

            string contentKey;
            
            switch (currentWeather)
            {
                case 0:
                    contentKey = ROAD_SUN;

                    break;
                case 1:
                    contentKey = ROAD_RAIN;

                    break;
                case 2:
                    contentKey = ROAD_SNOW;
                    
                    break;
                default:
                    contentKey = "";
                    break;
            }
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { weatherIcon.Content = System.Windows.Application.Current.FindResource("road_sun"); }));
            

            //get braking info
            BN.BrakingInfo info = new BN.BrakingInfo();
            BN.getBrakingInfo(69, ref info);
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { page1.brakingDistance.Text = Math.Round(info.distance).ToString() + " m";
                page1.brakingTime.Text = Math.Round(info.time).ToString() + " s";
            }));
        }

        public void InitTimer()
        {
            //make timer
            timer = new System.Timers.Timer(250);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += OnTimedEvent;
        }

        //change page function that is called when swipea
        void changePage(bool direction)
        {
            if (direction)
            {
                if (pageNumber == 1)
                {
                    main_frame.Content = page2;
                    pageNumber = 2;
                    ball2.Fill = new SolidColorBrush(Color.FromRgb(190, 190, 190));
                    ball1.Fill = new SolidColorBrush();
                }
            }
            else
            {
                if (pageNumber == 2)
                {
                    main_frame.Content = page1;
                    pageNumber = 1;
                    ball1.Fill = new SolidColorBrush(Color.FromRgb(190, 190, 190));
                    ball2.Fill = new SolidColorBrush();
                }
            }
        }



        //changes font size everytime windows is resized
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            const int COLOUMN_COUNT = 7;
            const double FONT_WEIGHT = .05;

            // calculate font size

            double font_size = main_frame.ActualWidth * FONT_WEIGHT;
            double screen_size = main_frame.ActualWidth;
            brakingDistance.FontSize = font_size;
            clock.FontSize = font_size;

            //change font on pages
            if (pageNumber == 1)
            {
                page1.brakingDistance.FontSize = font_size * 2;
                page1.brakingTime.FontSize = font_size * 2;
            }
            else
            { 
            }

            // calculate ball size

            ball1.Width = screen_size / (COLOUMN_COUNT * 2);
            ball2.Width = screen_size / (COLOUMN_COUNT * 2);
            ball1.Height = screen_size / (COLOUMN_COUNT * 2);
            ball2.Height = screen_size / (COLOUMN_COUNT * 2);
        }
    }
}
