using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Devices.InfoPages
{
    public partial class InfoPage4 : Page
    {
        private readonly string _status;

        private readonly string _username;
        private readonly string _phonenumber;
        private readonly string _bioghraphy;
        private readonly string _birthday;
        private readonly TcpClient _tcpClient;
        public InfoPage4(string status, string username, string phonenumber, string bioghraphy,TcpClient tcpClient,string birthday = "")
        {
            InitializeComponent();
            _status = status;
            _username = username;
            _phonenumber = phonenumber;
            _bioghraphy = bioghraphy;
            _birthday = birthday;
            _tcpClient = tcpClient;
            Loaded += InfoPage4_Loaded;
        }

        private void InfoPage4_Loaded(object sender, RoutedEventArgs e)
        {
            if (_status == "back")
                txtBirthday.Text = _birthday;

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

        private bool checkFormat()
        {
            string month = txtBirthday.Text.Substring(0, 2);
            string day = txtBirthday.Text.Substring(3, 2);
            string year = txtBirthday.Text.Substring(6, 4);

            bool isValid = true;
            
            foreach (char c in month) { if (!char.IsNumber(c)) isValid = false; }
            foreach (char c in day) { if (!char.IsNumber(c)) isValid = false; }
            foreach (char c in year) { if (!char.IsNumber(c)) isValid = false; }

            return isValid;
        }
        char lastTyped = ' ';
        private void txtBirthday_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
            lastTyped = e.Text[0];
        }
        private void txtBirthday_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBirthday.Text))
            {
                txtHelper.Text = "Example: 06/25/2006";
                txtHelper.Foreground = new SolidColorBrush(
    (Color)ColorConverter.ConvertFromString("#6C7AA8"));
            }
            if (txtBirthday.Text.Length > 10 && checkFormat())
            {
                string bd = txtBirthday.Text;
                bd = bd.Substring(0, 10);
                txtBirthday.Text = bd;
                txtBirthday.CaretIndex = txtBirthday.Text.Length;
            }
            else if(txtBirthday.Text.Length > 10 && !checkFormat())
            {
                if(lastTyped != ' ')
                {
                    txtHelper.Text = "Birthday is not valid.";
                    txtHelper.Foreground = Brushes.Orange;
                    return;
                }
                string text = txtBirthday.Text;
                int i = text.IndexOf(lastTyped);
                text = text.Remove(i);
                txtBirthday.Text = text;

                txtHelper.Text = "Birthday is not valid.";
                txtHelper.Foreground = Brushes.Orange;
                return;
            }
            if (txtBirthday.Text.Length == 2 || txtBirthday.Text.Length == 5)
            {
                string bd = txtBirthday.Text + "/";
                txtBirthday.Text = bd;
                txtBirthday.CaretIndex = txtBirthday.Text.Length;
            }
            if (txtBirthday.Text.Length == 10)
            {
                string month = txtBirthday.Text.Substring(0, 2);
                string day = txtBirthday.Text.Substring(3, 2);
                string year = txtBirthday.Text.Substring(6, 4);
                if (Convert.ToInt32(month) > 12)
                {
                    txtHelper.Text = "Month cannot be more than 12";
                    txtHelper.Foreground = Brushes.Orange;
                    return;
                }
                if (Convert.ToInt32(day) > 30 && Convert.ToInt32(month) > 6)
                {
                    txtHelper.Text = "This month doesn't have more than 30 days";
                    txtHelper.Foreground = Brushes.Orange;
                    return;
                }
                else if (Convert.ToInt32(day) > 31 && Convert.ToInt32(month) <= 6)
                {
                    txtHelper.Text = "This month doesn't have more than 31 days";
                    txtHelper.Foreground = Brushes.Orange;
                    return;
                }
                if (Convert.ToInt32(year) > 2025 || Convert.ToInt32(year) < 1900)
                {
                    txtHelper.Text = "Year is not valid.";
                    txtHelper.Foreground = Brushes.Orange;
                    return;
                }

                txtHelper.Text = "Valid Birthday ✓";
                txtHelper.Foreground = Brushes.LightGreen;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (txtHelper.Text == "Valid Birthday ✓" || string.IsNullOrWhiteSpace(txtBirthday.Text))
            {
                var host = (InfoWindow)Window.GetWindow(this);
                host.FrameRef.Navigate(new InfoPage5("next", _username, _phonenumber,_bioghraphy,txtBirthday.Text,_tcpClient));
            }
            else
            {
                MessageBox.Show("Your Birthday is not valid please reassure that your birthday is valid", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);
            host.FrameRef.Navigate(new InfoPage3("back", _username, _phonenumber, _tcpClient,_bioghraphy));
        }
    }
}
