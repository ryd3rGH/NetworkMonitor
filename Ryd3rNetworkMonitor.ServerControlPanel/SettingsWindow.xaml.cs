﻿using System;
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

namespace Ryd3rNetworkMonitor.ServerControlPanel
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public Settings settings;

        public SettingsWindow()
        {
            InitializeComponent();

            wHoursBox.ItemsSource = new[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 16 };

            settings = new Settings();
            settings.GetSettings();

            ipTxt.Text = settings.Ip;
            mesPortTxt.Text = settings.MesPort.ToString();
            timeTxt.Text = settings.MessageCheckTime.ToString();
            noMesTxt.Text = settings.HostOfflineCheckTime.ToString();

            if (settings.IpDisplay)
                ipChBox.IsChecked = true;

            if (settings.LoginDisplay)
                loginChBox.IsChecked = true;

            if (settings.MessageDisplay)
                messageChBox.IsChecked = true;

            for (int i=0; i<wHoursBox.Items.Count; i++)
            {
                if ((int)wHoursBox.Items[i] == settings.WorkingHours)
                {
                    wHoursBox.SelectedItem = wHoursBox.Items[i];
                    break;
                }
            }

            logMaxLinesTxt.Text = settings.LogMaxLines.ToString();

            if (settings.SaveOnClose)
                closeSaveChBox.IsChecked = true;

            if (settings.SaveOnClear)
                clearSaveChBox.IsChecked = true;
        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings != null)
            {
                if (mesPortTxt.Text != string.Empty)
                {
                    Settings newSettings = new Settings(!String.IsNullOrWhiteSpace(ipTxt.Text) ? ipTxt.Text : "127.0.0.1",
                                          !String.IsNullOrWhiteSpace(mesPortTxt.Text) ? Convert.ToInt32(mesPortTxt.Text) : 17178,
                                          (bool)ipChBox.IsChecked ? true : false,
                                          (bool)loginChBox.IsChecked ? true : false,
                                          (bool)messageChBox.IsChecked ? true : false,
                                          !String.IsNullOrWhiteSpace(timeTxt.Text) ? Convert.ToInt32(timeTxt.Text) : 9,
                                          !String.IsNullOrWhiteSpace(noMesTxt.Text) ? Convert.ToInt32(noMesTxt.Text) : 50,
                                          wHoursBox.SelectedItem != null ? (int)wHoursBox.SelectedItem : 8,
                                          !String.IsNullOrWhiteSpace(logMaxLinesTxt.Text) ? Convert.ToInt32(logMaxLinesTxt.Text) : 50,
                                          (bool)closeSaveChBox.IsChecked ? true : false, 
                                          (bool)clearSaveChBox.IsChecked ? true : false);

                    if (!settings.Equals(newSettings))
                    {
                        settings.SaveSettings(newSettings.Ip, newSettings.MesPort, newSettings.IpDisplay,
                                          newSettings.LoginDisplay, newSettings.MessageDisplay, newSettings.MessageCheckTime,
                                          newSettings.HostOfflineCheckTime, newSettings.WorkingHours, newSettings.LogMaxLines,
                                          newSettings.SaveOnClose, newSettings.SaveOnClear);

                        MessageBox.Show("Settings saved.");
                    }
                    
                    this.Close();
                }
                else
                    MessageBox.Show("\nRegistration port and message port must have different values.", "Error");
            }
        }

        private void regPortTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void mesPortTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void timeTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void noMesTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void logMaxLinesTxt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }
    }
}
