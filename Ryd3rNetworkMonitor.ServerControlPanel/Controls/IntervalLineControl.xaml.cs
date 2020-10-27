using Ryd3rNetworkMonitor.Library;
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

namespace Ryd3rNetworkMonitor.ServerControlPanel.Controls
{
    /// <summary>
    /// Interaction logic for IntervalLineControl.xaml
    /// </summary>
    public partial class IntervalLineControl : UserControl
    {
        private double TotalSeconds { get; set; }

        private void TimeCount(double seconds)
        {
            double h = 0;
            double m = 0;
            double s = 0;

            if (seconds > 0)
            {
                if (seconds >= 0 && seconds <= 59)
                {
                    s = seconds;                   
                }
                else if (seconds > 59 && seconds <= 3599)
                {
                    m = (int)seconds / 60;
                    s = seconds - m * 60;
                }
                else if (seconds > 3600)
                {
                    h = (int)seconds / 3600;
                    if (seconds > h * 3600 && seconds - h * 3600 > 0 && seconds - h * 3600 <= 59)
                        s = seconds - h * 3600;
                    else
                    {
                        m = (int)(seconds - h * 3600) / 60;
                        s = seconds - h * 3600 - m * 60;
                    }
                }

                totalIntervalTimeLbl.Content = $"{h.ToString("00")} : {m.ToString("00")} : {s.ToString("00")}";
            }
            else
                totalIntervalTimeLbl.Content = "error";
        }

        public IntervalLineControl(IntervalTemplate temp)
        {
            InitializeComponent();

            startLbl.Content = temp.StartTime.ToString();
            endLbl.Content = temp.EndTime.ToString();
            clicksLbl.Content = temp.Clicks;
            keysLbl.Content = temp.Presses;

            TotalSeconds = (temp.EndTime.Value - temp.StartTime.Value).TotalSeconds;
            TimeCount(TotalSeconds);
        }
    }
}
