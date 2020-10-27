using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using Ryd3rNetworkMonitor.Library;
using Ryd3rNetworkMonitor.ServerControlPanel.Controls;

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    enum Months
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    public partial class IntervalsReportWindow : Window
    {
        bool today;
        bool week;
        bool thisMonth;
        bool spDate;
        bool spMonth;
        bool spPeriod;

        double workingDayMaxSeconds;

        Host selectedHost;
        List<Host> hosts;
        List<IntervalTemplate> intervals;

        public Func<double, string> Formatter { get; set; }

        private bool Today
        {
            get { return (bool)this.todayBtn.IsChecked; }
            set
            {
                today = value;
            }
        }

        private bool Week
        {
            get { return (bool)this.lWeekBtn.IsChecked; }
            set { week = value; }
        }

        private bool ThisMonth
        {
            get { return (bool)this.lMonthBtn.IsChecked; }
            set { thisMonth = value; }
        }

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
                    endPicker.IsEnabled = false;
                }
                else
                {
                    stPicker.IsEnabled = false;
                    stPicker.SelectedDate = null;

                    endPicker.IsEnabled = false;
                    endPicker.SelectedDate = null;
                }
            }
        }

        private void TableAndChartConstruction()
        {
            if (intervals != null && intervals.Count > 0)
            {
                int maxDays = 0;

                List<string> dateLabels = new List<string>();
                List<IntervalTemplate> outputIntervals = new List<IntervalTemplate>();

                IntervalsChart.Series = null;
                SeriesCollection columns = new SeriesCollection();

                if (Today || SpDate)
                {
                    maxDays = 1;

                    dateLabels.Clear();
                    dateLabels.Add(!Today ? ((DateTime)datePicker.SelectedDate).ToShortDateString() : DateTime.Now.ToShortDateString());
                }

                else
                {
                    if (Week)
                        maxDays = 7;
                    if (ThisMonth)
                        maxDays = DateTime.Now.Date.Day;
                    if (SpMonth)
                    {
                        //last year month ???
                        maxDays = 30;

                        Months selectedMonth;
                        Enum.TryParse<Months>(monthsBox.SelectedItem.ToString(), out selectedMonth);

                        if ((int)selectedMonth == 1 || (int)selectedMonth == 3 || (int)selectedMonth == 4 || (int)selectedMonth == 5 ||
                            (int)selectedMonth == 7 || (int)selectedMonth == 8 || (int)selectedMonth == 10 || (int)selectedMonth == 12)
                            maxDays = 31;
                        else
                        {
                            if ((int)selectedMonth == 2)
                                maxDays = 29;
                            else
                                maxDays = 30;
                        }

                        var lastDate = new DateTime(DateTime.Now.Year, (int)selectedMonth, maxDays);

                        for (int i = maxDays - 1; i >= 0; i--)
                            dateLabels.Add(lastDate.AddDays((-1) * i).ToShortDateString());
                    }

                    if (SpPeriod)
                        maxDays = ((DateTime)endPicker.SelectedDate - stPicker.SelectedDate).Value.Days + 1;

                    if (!SpMonth)
                    {
                        for (int i = maxDays - 1; i >= 0; i--)
                            dateLabels.Add(DateTime.Now.AddDays((-1) * i).ToShortDateString());
                    }
                }

                if (Today)
                    outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date == DateTime.Now.Date && x.EndTime.Value.Date == DateTime.Now.Date);

                if (Week)
                    outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date >= DateTime.Now.Date.AddDays(-7) && x.EndTime.Value.Date <= DateTime.Now.Date);

                if (ThisMonth)
                    outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).Date && x.EndTime.Value.Date <= DateTime.Now.Date);

                if (SpDate)
                    outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date == datePicker.SelectedDate && x.EndTime.Value.Date == datePicker.SelectedDate);

                if (SpMonth)
                {
                    Months selectedMonth;
                    if (monthsBox.SelectedIndex != -1)
                    {
                        Enum.TryParse<Months>(monthsBox.SelectedItem.ToString(), out selectedMonth);

                        outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date >= new DateTime(DateTime.Now.Year, (int)selectedMonth, 1)
                                          && x.EndTime.Value.Date <= new DateTime(DateTime.Now.Year, (int)selectedMonth, maxDays));
                    }
                }

                if (SpPeriod)
                    outputIntervals = intervals.FindAll(x => x.StartTime.Value.Date >= stPicker.SelectedDate && x.EndTime.Value.Date <= endPicker.SelectedDate);

                if (outputIntervals != null && outputIntervals.Count > 0)
                {
                    #region table

                    for (int i = 0; i < outputIntervals.Count; i++)
                    {
                        IntervalLineControl line = new IntervalLineControl(outputIntervals[i]);
                        resPanel.Children.Add(line);
                    }

                    //visible by default
                    chartGBox.Visibility = Visibility.Visible;

                    #endregion

                    #region chart region

                    IntervalsChart.Margin = new Thickness(50, 20, 50, 20);

                    Percentage.MaxValue = 100;

                    Dates.Labels = dateLabels;

                    RowSeries onlineSeries = new RowSeries() { Title = "Online time, %", Values = new ChartValues<double>() };
                    RowSeries offlineSeries = new RowSeries() { Title = "Offline time, %", Values = new ChartValues<double>() };

                    for (int i = maxDays - 1; i >= 0; i--)
                    {
                        double dateOnlineSecondsValue = 0;

                        var currDate = new DateTime();

                        if (Today || SpDate)
                            currDate = Today ? DateTime.Now : (DateTime)datePicker.SelectedDate;
                        else
                        {
                            if (!SpMonth)
                                currDate = DateTime.Now.AddDays((-1) * i);
                            else
                                currDate = new DateTime(DateTime.Now.Year, (int)monthsBox.SelectedItem, maxDays - i);
                        }

                        var dateIntervals = outputIntervals.FindAll(x => x.StartTime.Value.Date == currDate.Date && x.EndTime.Value.Date == currDate.Date);
                        if (dateIntervals.Count > 0)
                        {
                            foreach (var interval in dateIntervals)
                            {
                                dateOnlineSecondsValue += (double)((interval.EndTime - interval.StartTime).Value.TotalSeconds);
                            }
                        }

                        if (dateOnlineSecondsValue > 0)
                        {
                            double onlinePercentage = (dateOnlineSecondsValue * 100) / workingDayMaxSeconds;

                            onlineSeries.Values.Add(Math.Round(onlinePercentage, 2));
                            offlineSeries.Values.Add(Math.Round(100 - onlinePercentage, 2));
                        }
                        else
                        {
                            onlineSeries.Values.Add(0d);
                            offlineSeries.Values.Add(100d);
                        }
                    }

                    onlineSeries.Fill = Brushes.LightGreen;

                    columns.Add(onlineSeries);
                    columns.Add(offlineSeries);

                    IntervalsChart.Series = columns;

                    #endregion                    
                }
            }
        }

        public IntervalsReportWindow()
        {
            InitializeComponent();

            this.Title = "Intervals report";
            Today = true;
            Week = false;
            ThisMonth = false;
            SpDate = false;
            SpMonth = false;
            SpPeriod = false;

            workingDayMaxSeconds = Properties.Settings.Default.WorkingHours * 3600;

            stIntervalLbl.Content = "Start of interval";
            endIntervalLbl.Content = "End of interval";
            clicksIntervalLbl.Content = "Clicks in interval";
            keysIntervalLbl.Content = "Key presses in interval";
            totalIntervalTimeLbl.Content = "Total time in interval";

            chartGBox.Visibility = Visibility.Hidden;
            tableBtn.IsChecked = true;
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

        private void todayBtn_Checked(object sender, RoutedEventArgs e)
        {
            Today = true;
        }

        private void todayBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            Today = false;
        }

        private void lWeekBtn_Checked(object sender, RoutedEventArgs e)
        {
            Week = true;
        }

        private void lWeekBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            Week = false;
        }

        private void lMonthBtn_Checked(object sender, RoutedEventArgs e)
        {
            ThisMonth = true;
        }

        private void lMonthBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            ThisMonth = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hostsBox.IsEnabled = false;
            monthsBox.ItemsSource = Enum.GetValues(typeof(Months));

            BackgroundWorker hLoader = new BackgroundWorker();
            hLoader.DoWork += HLoader_DoWork;
            hLoader.RunWorkerCompleted += HLoader_RunWorkerCompleted;
            hLoader.RunWorkerAsync();
        }

        private void HLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            hostsBox.IsEnabled = true;
            if (hosts != null && hosts.Count > 0)
                hostsBox.ItemsSource = hosts;
        }

        private void HLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            hosts = DbServer.DbGetHosts();
        }        

        private void hostsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chartGBox.Visibility = Visibility.Hidden;
        }

        private void showChartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (hostsBox.Items.Count > 0 && hostsBox.SelectedIndex != -1)
            {
                if (Today || Week || ThisMonth || SpDate || SpMonth || SpPeriod)
                {
                    MessageBoxResult res = MessageBox.Show("This may take long time. Proceed?", "Warning", MessageBoxButton.OKCancel);
                    if (res == MessageBoxResult.OK)
                    {
                        resPanel.Children.Clear();

                        //if host changed
                        if (selectedHost != (Host)hostsBox.SelectedItem)
                        {
                            selectedHost = (Host)hostsBox.SelectedItem;
                            chartGBox.Header = $"Intervals of working time for HostID '{selectedHost.HostId}'";

                            BackgroundWorker intWorker = new BackgroundWorker();
                            intWorker.DoWork += IntWorker_DoWork;
                            intWorker.RunWorkerCompleted += IntWorker_RunWorkerCompleted;
                            intWorker.RunWorkerAsync();
                        }

                        //if host didn't changed
                        else
                            TableAndChartConstruction();
                    }
                }
                else
                    MessageBox.Show("You need to choose one of periods to build table/chart");
            }
            else
                MessageBox.Show("You need to select host");
        }

        private void IntWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            intervals = DbServer.DBGetIntervals(selectedHost.HostId);

            if (intervals != null && intervals.Count > 0)
            {
                for (int i=0; i<intervals.Count; i++)
                {
                    int clicks = 0;
                    int presses = 0;

                    DbServer.DbGetIntervalMK(selectedHost.HostId, intervals[i], out clicks, out presses);

                    intervals[i].Clicks = clicks;
                    intervals[i].Presses = presses;
                }
            }
        }

        private void IntWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TableAndChartConstruction();
        }

        private void chartBtn_Click(object sender, RoutedEventArgs e)
        {
            tableBtn.IsChecked = false;
            chartBtn.IsChecked = true;

            titleGrid.Visibility = Visibility.Collapsed;
            tableViewer.Visibility = Visibility.Collapsed;

            IntervalsChart.Visibility = Visibility.Visible;
        }

        private void tableBtn_Click(object sender, RoutedEventArgs e)
        {
            tableBtn.IsChecked = true;
            chartBtn.IsChecked = false;

            IntervalsChart.Visibility = Visibility.Collapsed;

            titleGrid.Visibility = Visibility.Visible;
            tableViewer.Visibility = Visibility.Visible;
        }

        private void stPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (stPicker.SelectedDate != null)
            {
                if (endPicker.SelectedDate == null)
                {
                    endPicker.DisplayDateStart = stPicker.SelectedDate;
                    endPicker.DisplayDateEnd = stPicker.SelectedDate.Value.AddDays(30);
                }
                else
                {
                    endPicker.DisplayDateEnd = stPicker.SelectedDate.Value.AddDays(30);

                    if (endPicker.SelectedDate.Value - stPicker.SelectedDate.Value > new TimeSpan(30, 0, 0, 0, 0))
                    {                        
                        endPicker.SelectedDate = stPicker.SelectedDate.Value.AddDays(30);
                    }
                }

                endPicker.IsEnabled = true;
            }
        }
    }
}
