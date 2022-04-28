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
        int userTimeout = 1000;
        MainWindow mainWindow;
        public Page2(MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            InitializeComponent();
        }

        private void SunIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BN.setWeather(BN.WEATHER.BN_DRY);
            mainWindow.StartUserClearTimeout(userTimeout);
        }

        private void RainIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BN.setWeather(BN.WEATHER.BN_WET);
            mainWindow.StartUserClearTimeout(userTimeout);
        }

        private void WaterlayerIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BN.setWeather(BN.WEATHER.BN_WLAYER);
            mainWindow.StartUserClearTimeout(userTimeout);
        }

        private void SnowIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            BN.setWeather(BN.WEATHER.BN_ICY);
            mainWindow.StartUserClearTimeout(userTimeout);
        }
    }
}
