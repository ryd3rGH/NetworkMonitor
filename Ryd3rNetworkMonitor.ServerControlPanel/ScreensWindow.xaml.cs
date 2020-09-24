using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    /// <summary>
    /// Interaction logic for ScreensWindow.xaml
    /// </summary>
    public partial class ScreensWindow : Window
    {
        public ScreensWindow(byte[] imBytes, DateTime dt)
        {
            InitializeComponent();

            this.Title = $"Screenshot {dt.ToShortDateString()} {dt.ToLongTimeString()}";
            timeLbl.Content = $"{dt.ToShortDateString()} {dt.ToLongTimeString()}";

            if (imBytes != null)
            {
                BitmapImage bim = new BitmapImage();

                using (var ms = new MemoryStream(imBytes))
                {
                    bim.BeginInit();
                    bim.StreamSource = ms;
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.EndInit();

                    if (bim != null)
                        currImage.Source = bim;
                }
            }
        }
    }
}
