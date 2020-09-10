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

            settings = new Settings();
            settings.GetSettings();

            ipTxt.Text = settings.Ip;
            regPortTxt.Text = settings.RegPort.ToString();
            mesPortTxt.Text = settings.MesPort.ToString();

        }

        private void applyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (settings != null)
            {
                if (regPortTxt.Text != mesPortTxt.Text)
                {
                    settings.SaveSettings(!String.IsNullOrWhiteSpace(ipTxt.Text) ? ipTxt.Text : "127.0.0.1",
                                          !String.IsNullOrWhiteSpace(regPortTxt.Text) ? Convert.ToInt32(regPortTxt.Text) : 17179,
                                          !String.IsNullOrWhiteSpace(mesPortTxt.Text) ? Convert.ToInt32(mesPortTxt.Text) : 17178);

                    MessageBox.Show("Settings saved.");
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
    }
}
