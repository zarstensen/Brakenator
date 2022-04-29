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

namespace Brakenator
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        int userTimeout = 20000;
        MainWindow mainWindow;
        public Page2(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            InitializeComponent();
        }
        double timeStart;
        double timeEnd;

        // how long the button should be held, before the press will be ignored.
        const double buttonPressTime = 200;

        private void SelectButton(BN.WEATHER button_id)
        {
            var timeDate = DateTime.Now;
            timeEnd = timeDate.TimeOfDay.TotalMilliseconds;
            if (timeEnd - timeStart < buttonPressTime)
            {
                BN.setWeather(button_id);
                if (mainWindow.userClearTimer != null)
                    mainWindow.userClearTimer.Stop();
                mainWindow.StartUserClearTimeout(userTimeout);
            }
        }

        private void SunIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton(BN.WEATHER.BN_DRY);
        }

        private void RainIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton(BN.WEATHER.BN_WET);
        }

        private void WaterlayerIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton(BN.WEATHER.BN_WLAYER);
        }

        private void SnowIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectButton(BN.WEATHER.BN_ICY);
        }

        private void Auto_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var timeDate = DateTime.Now;
            timeEnd = timeDate.TimeOfDay.TotalMilliseconds;
            if (timeEnd - timeStart < buttonPressTime)
            {
                BN.clearUserWeather();
            }
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var timeDate = DateTime.Now;
            timeStart = timeDate.TimeOfDay.TotalMilliseconds;
        }
    }
}
