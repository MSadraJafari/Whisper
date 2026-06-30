using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace Devices.InfoPages
{
    public partial class InfoPage0 : Page
    {
        public string ServerIP { get; private set; } = "127.0.0.1";
        public int ServerPort { get; private set; } = 9999;

        public InfoPage0()
        {
            InitializeComponent();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtServerPort.Text.Trim(), out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port between 1 and 65535.",
                                "Invalid Port",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            ServerPort = port;
            ServerIP = txtServerPort.Text.Trim();

            Dispatcher.Invoke(() =>
            {
                var host = (InfoWindow)Window.GetWindow(this);
                host.FrameRef.Navigate(new InfoPage1("next",IPAddress.Parse(ServerIP),ServerPort));
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Cancel / Back
        }
    }
}