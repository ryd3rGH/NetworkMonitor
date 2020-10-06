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
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    /// <summary>
    /// Interaction logic for IntervalsReportWindow.xaml
    /// </summary>
    public partial class IntervalsReportWindow : Window
    {
        bool spDate;
        bool spMonth;
        bool spPeriod;

        private bool SpDate 
        { 
            get { return (bool)this.dateBtn.IsChecked; }
            set
            {
                spDate = value;
                if (spDate)
                    datePicker.IsEnabled = true;
                else
                    datePicker.IsEnabled = false;
            }
        }

        private bool SpMonth
        {
            get { return (bool)this.monthBtn.IsChecked; }
            set
            {
                spMonth = value;
                if (spMonth)
                    monthsBox.IsEnabled = true;
                else
                    monthsBox.IsEnabled = false;
            }
        }

        private bool SpPeriod
        {
            get { return (bool)this.periodBtn.IsChecked; }
            set
            {
                spPeriod = value;
                if (spPeriod)
                {
                    stPicker.IsEnabled = true;
                    endPicker.IsEnabled = true;
                }
                else
                {
                    stPicker.IsEnabled = false;
                    endPicker.IsEnabled = false;
                }
            }
        }

        public IntervalsReportWindow()
        {
            InitializeComponent();

            this.Title = "Intervals report";
            SpDate = false;
            SpMonth = false;
            SpPeriod = false;
        }

        private void dateBtn_Checked(object sender, RoutedEventArgs e)
        {
            SpDate = true;
        }

        private void dateBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            SpDate = false;
        }

        private void monthBtn_Checked(object sender, RoutedEventArgs e)
        {
            SpMonth = true;
        }

        private void monthBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            SpMonth = false;
        }

        private void periodBtn_Checked(object sender, RoutedEventArgs e)
        {
            SpPeriod = true;
        }

        private void periodBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            SpPeriod = false;
        }
    }
}
