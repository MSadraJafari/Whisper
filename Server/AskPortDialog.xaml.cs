using System.Net;
using System.Windows;
namespace Server
{
    public partial class AskPortDialog : Window
    {
        public int ResultPort { get; private set; }

        public AskPortDialog(int currentPort)
        {
            InitializeComponent();
            txtPort.Text = currentPort.ToString();

            Loaded += (_, __) =>
            {
                txtPort.SelectAll();
                txtPort.Focus();
            };
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtPort.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Enter a valid port between 1 and 65535.",
                                "Invalid Port",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            ResultPort = port;

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}