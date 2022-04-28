using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
using System.Runtime.InteropServices;

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

        double velocity = 0;
        double lattitude = 0;
        double longitude = 0;

        Thread debugConsoleThrd;
        bool runConsole = false;
        bool mainPage = true;

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


            debugConsoleThrd = new Thread(() => { this.DebConsoleInput(); });
            runConsole = true;
            debugConsoleThrd.Start();

            

            InitializeComponent();
            main_frame.Content = page1;
            Frame map_frame = (Frame)this.FindResource("map");
            map_frame.Content = page1;

            StartUpdateLoop();
        }

        ~MainWindow()
        {
            runConsole = false;
            debugConsoleThrd.Join();
            BN.BNcleanup();
        }

        [DllImport("Kernel32.dll")]
        public static extern bool AllocConsole();
        public void DebConsoleInput()
        {
            AllocConsole();
            
            while(runConsole)
            {
                Console.Write("value: ");
                string line = Console.ReadLine();

                if (line == "coord")
                {

                    Console.Write("lat: ");
                    double.TryParse(Console.ReadLine(), out lattitude);
                    Console.Write("lon: ");
                    double.TryParse(Console.ReadLine(), out longitude);
                }
                else if (line == "vel")
                {
                    Console.Write("vel: ");
                    double.TryParse(Console.ReadLine(), out velocity);
                }
                else if (line == "map")
                {
                    Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        this.Content = this.FindResource("map");
                        this.Height = 90 * 2;
                        this.Width = 160 * 2;
                    }));
                    
                }
                else if (line == "main")
                {
                    Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => {
                        this.Content = main;
                    }));
                }
                else
                {
                    Console.WriteLine("Invalid argument!");
                }
            }
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
            if (Math.Abs(endPoint.X - startPoint.X) > 100)
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

            
            clock.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { BN.autoWeather(lattitude, longitude); }));
            

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
            BN.getBrakingInfo(velocity, ref info);
            Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Normal,
            new Action(() => { 
                page1.brakingDistance.Text = Math.Round(info.distance).ToString() + " m";
                page1.brakingTime.Text = Math.Round(info.time).ToString() + " s";
            }));
        }
        System.Timers.Timer loopTimer;
        System.Timers.Timer userClearTimer;
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
            if (mainPage)
            {
                const double FONT_WEIGHT = 0.1;
                const double BALL_WEIGHT = 0.125;

                //Calculate ball size
                double ball_size = Math.Min(this.ActualWidth / 1.1, this.ActualHeight);

                // calculate font size
                double clock_size = Math.Min(this.ActualWidth / 2.1, this.ActualHeight);
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


                page2.sunRoadText.FontSize = font_size * .75;
                page2.rainRoadText.FontSize = font_size * .75;
                page2.waterlayerRoadText.FontSize = font_size * .75;
                page2.snowRoadText.FontSize = font_size * .75;


                }
            else
            {

            }

        }
    }
}
