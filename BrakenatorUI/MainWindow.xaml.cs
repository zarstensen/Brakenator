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
using System.IO;

namespace Brakenator
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public BN.WEATHER currentWeather;
        int pageNumber = 1;
        Page1 page1;
        Page2 page2;

        const string ROAD_SUN = "road_sun";
        const string ROAD_RAIN = "road_rain";
        const string ROAD_SNOW = "road_snow";
        const string ROAD_WATERLAYER = "road_waterlayer";


        public MainWindow()
        {
            BN.BNinit();

            page1 = new Page1(this);
            page2 = new Page2(this);
            BN.setWeatherKey(Directory.GetCurrentDirectory() + @"\weather_key.txt");

            BN.addCoeff(BN.WEATHER.BN_DRY, 50, 1);
            BN.addCoeff(BN.WEATHER.BN_WET, 50, .5);
            BN.addCoeff(BN.WEATHER.BN_WLAYER, 50, .4);
            BN.addCoeff(BN.WEATHER.BN_ICY, 50, .1);
            BN.addCoeff(BN.WEATHER.BN_DRY, 90, .95);
            BN.addCoeff(BN.WEATHER.BN_WET, 90, .2);
            BN.addCoeff(BN.WEATHER.BN_WLAYER, 90, .1);
            BN.addCoeff(BN.WEATHER.BN_DRY, 130, .9);
            BN.addCoeff(BN.WEATHER.BN_WET, 130, .2);
            BN.addCoeff(BN.WEATHER.BN_WLAYER, 130, .1);

            InitializeComponent();
            main_frame.Content = page1;
            BN.autoWeather(1, 1);
            StartUpdateLoop();
        }

        ~MainWindow()
        {
            BN.BNcleanup();
        }

        public static Point GetMousePositionWindowsForms()
        {
            var point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        
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

            //BN.autoWeather(55.779037, 12.532600);
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { BN.autoWeather(67.621862, 59.1948173); }));
            

            BN.WEATHER w = BN.getWeather();

            string contentKey = "";
            string sunKey = "_unpressed";
            string rainKey = "_unpressed";
            string waterlayerKey = "_unpressed";
            string snowKey = "_unpressed";
            switch (BN.getWeather())
            {
                case BN.WEATHER.BN_DRY:
                    contentKey = ROAD_SUN;
                    sunKey = "_pressed";


                    break;
                case BN.WEATHER.BN_WET:
                    contentKey = ROAD_RAIN;
                    rainKey = "_pressed";

                    break;
                case BN.WEATHER.BN_WLAYER:
                    contentKey = ROAD_WATERLAYER;
                    waterlayerKey = "_pressed";
                    break;
                case BN.WEATHER.BN_ICY:
                    contentKey = ROAD_SNOW;
                    snowKey = "_pressed";
                    break;

            }
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { weatherIcon.Content = Application.Current.FindResource(contentKey);
                page2.sunIcon.Content = Application.Current.FindResource(ROAD_SUN + sunKey);
                page2.rainIcon.Content = Application.Current.FindResource(ROAD_RAIN + rainKey);
                page2.waterlayerIcon.Content = Application.Current.FindResource(ROAD_WATERLAYER+ waterlayerKey);
                page2.snowIcon.Content = Application.Current.FindResource(ROAD_SNOW + snowKey);
            }));
            

            //get braking info
            BN.BrakingInfo info = new BN.BrakingInfo();
            BN.getBrakingInfo(69, ref info);
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { page1.brakingDistance.Text = Math.Round(info.distance).ToString() + " m";
                page1.brakingTime.Text = Math.Round(info.time).ToString() + " s";
            }));


            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,

            new Action(() => {
                string test = this.ActualHeight.ToString() + "  " +  this.ActualWidth.ToString();
                brakingDistance.Text = test;
            }));
        }
        Timer loopTimer;
        Timer userClearTimer;
        public void StartUpdateLoop()
        {
            //make timer
            loopTimer = new System.Timers.Timer(250);
            loopTimer.Enabled = true;
            loopTimer.Elapsed += OnTimedEvent;
        }

        /// <summary>
        /// starts a timer that clears the user weather data when timeout has been exceeded
        /// </summary>
        /// <param name="timeout">how long before the user weather is cleared (ms).</param>
        

        public void StartUserClearTimeout(int timeout)
        {
            //make timer
            userClearTimer = new System.Timers.Timer(timeout);
            userClearTimer.AutoReset = false;
            userClearTimer.Enabled = true;
            userClearTimer.Elapsed += ClearUserWeather;
        }

        private void ClearUserWeather(Object source, System.Timers.ElapsedEventArgs e)
        {
            BN.clearUserWeather();
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
            const double FONT_WEIGHT = 0.1;
            const double BALL_WEIGHT = 0.125;

            //Calculate ball size
            double ball_size = Math.Min(this.ActualWidth / 1.1, this.ActualHeight);

            // calculate font size
            double clock_size = Math.Min(this.ActualWidth / 2.1 , this.ActualHeight);
            double font_size = clock_size * FONT_WEIGHT;


            brakingDistance.FontSize = font_size;
            clock.FontSize = font_size;

            //change font on pages
            if (pageNumber == 1)
            {
                page1.brakingDistance.FontSize = font_size * 2.2; 
                page1.brakingTime.FontSize = font_size * 2.2;
            }
            else
            { 
            }

            // calculate ball size
            ball1.Width = ball_size * BALL_WEIGHT;
            ball2.Width = ball_size * BALL_WEIGHT;
            ball1.Height = ball_size * BALL_WEIGHT;
            ball2.Height = ball_size * BALL_WEIGHT;

        }
    }
}
