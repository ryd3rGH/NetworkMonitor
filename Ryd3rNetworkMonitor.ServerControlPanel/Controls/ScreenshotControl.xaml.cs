using Ryd3rNetworkMonitor.Library;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ryd3rNetworkMonitor.ServerControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for ScreenshotControl.xaml
    /// </summary>
    public partial class ScreenshotControl : UserControl
    {
        private byte[] ImBytes { get; set; }
        private DateTime Dt { get; set; }
        private HostMessage Mes { get; set; }

        public ScreenshotControl(byte[] imBytes, DateTime dt)
        {
            ImBytes = imBytes;
            Dt = dt;

            InitializeComponent();

            if (imBytes != null && Dt != null)
            {
                BitmapImage bim = new BitmapImage();

                using (var ms = new MemoryStream(ImBytes))
                {
                    bim.BeginInit();
                    bim.StreamSource = ms;
                    bim.CacheOption = BitmapCacheOption.OnLoad;
                    bim.EndInit();

                    if (bim != null)
                        screenIm.Source = bim;
                }

                timeLbl.Content = Dt != null ? $"{Dt.ToShortDateString()} {Dt.ToLongTimeString()}" : "error";
            }
        }

        public ScreenshotControl(HostMessage mes)
        {
            Mes = mes;

            InitializeComponent();

            if (Mes != null)
            {
                if (Mes.InnerMessage != null && Mes.InnerMessage.Type == InnerMessageTypes.Screenshot && Mes.InnerMessage.ImageBytes != null)
                {
                    BitmapImage bim = new BitmapImage();

                    using (var ms = new MemoryStream(Mes.InnerMessage.ImageBytes))
                    {
                        bim.BeginInit();
                        bim.StreamSource = ms;
                        bim.CacheOption = BitmapCacheOption.OnLoad;
                        bim.EndInit();

                        if (bim != null)
                            screenIm.Source = bim;
                    }

                    timeLbl.Content = mes.MessageTime != null ? $"{mes.MessageTime.ToShortDateString()} {mes.MessageTime.ToLongTimeString()}" : "error";
                }
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Mes != null)
            {
                ScreensWindow screen = new ScreensWindow(Mes.InnerMessage.ImageBytes, Mes.MessageTime);
                screen.ShowDialog();
            }

            else
            {
                ScreensWindow screen = new ScreensWindow(ImBytes, Dt);
                screen.ShowDialog();
            }
        }
    }
}
