using Model;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Devices.InfoPages
{
    public partial class InfoPage2 : Page
    {
        private readonly string _username;
        private readonly string _status;
        private readonly string _phoneNumber;
        private readonly string _bio;
        private readonly TcpClient _tcpClient;
        public InfoPage2(string status,string username,TcpClient tcpClient,string bio = "")
        {
            _status = status;
            _username = username;
            _bio = bio;
            _tcpClient = tcpClient;
            InitializeComponent();
            Loaded += InfoPage3_Loaded;
        }
        private void InfoPage3_Loaded(object sender, RoutedEventArgs e)
        {
            if(_status =="back")
                txtBiography.Text = _bio;

            Root.Opacity = 0;

            var trans = new TranslateTransform();
            Root.RenderTransform = trans;

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var slide = new DoubleAnimation
            {
                From = _status == "back" ? -70 : 70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            Root.BeginAnimation(OpacityProperty, fadeIn);
            trans.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        private void txtBiography_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtCount.Text = $"{txtBiography.Text.Length}/250";

            if(txtCount.Text == "250/250")
                txtCount.Foreground = Brushes.Orange;
            else
                txtCount.Foreground = Brushes.LightGreen;

            if (txtBiography.Text.Length > 250)
            {
                string text = txtBiography.Text;
                text = text.Substring(0, 250);
                txtBiography.Text = text;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);
            host.FrameRef.Navigate(new InfoPage3("next", _username, txtBiography.Text,_tcpClient));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);
            host.FrameRef.Navigate(new InfoPage1("back",_username));
        }
    }
}
