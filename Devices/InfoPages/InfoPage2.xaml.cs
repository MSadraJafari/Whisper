using Model;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Devices.InfoPages
{
    public partial class InfoPage2 : Page
    {
        private readonly List<Country> _countries = new List<Country>
        {
            new Country { Name = "Iran", Flag = "🇮🇷", Code = "+98" },
            new Country { Name = "Turkey", Flag = "🇹🇷", Code = "+90" },
            new Country { Name = "Germany", Flag = "🇩🇪", Code = "+49" },
            new Country { Name = "United States", Flag = "🇺🇸", Code = "+1" },
            new Country { Name = "United Kingdom", Flag = "🇬🇧", Code = "+44" }
        };
        private Country _currentCountry;
        private readonly string _status;
        private readonly string _username;
        private readonly string _phonenumber;
        private readonly TcpClient _tcpClient;

        public InfoPage2(string status, string username,TcpClient tcpClient, string phonenumber ="")
        {
            InitializeComponent();
            _username = username;
            _status = status;
            Loaded += InfoPage2_Loaded;
            _phonenumber = phonenumber;
            _tcpClient = tcpClient;
        }

        private void SetCountry(Country country)
        {
            _currentCountry = country;
            txtFlag.Text = country.Flag;
            txtCountryCode.Text = country.Code;
        }

        private void InfoPage2_Loaded(object sender, RoutedEventArgs e)
        {
            if (_status == "back")
                txtPhoneNumber.Text = _phonenumber;
            SetCountry(_countries.First(c => c.Code == "+98"));
            txtPhoneNumber.Focus();

            
            Root.Opacity = 0;
            var trans = new TranslateTransform();
            Root.RenderTransform = trans;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            var slide = new DoubleAnimation(_status == "back" ? -70 : 70, 0, TimeSpan.FromMilliseconds(250));

            Root.BeginAnimation(OpacityProperty, fadeIn);
            trans.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        private void btnCountry_Click(object sender, RoutedEventArgs e)
        {
            var selector = new CountrySelectorWindow(_countries, _currentCountry);
            selector.Owner = System.Windows.Application.Current.MainWindow;
            selector.CountrySelected += (s, selectedCountry) => SetCountry(selectedCountry);
            selector.ShowDialog();
        }

        private void txtPhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text[0]);
        }

        private bool _updatingPhoneText = false;

        private void txtPhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_updatingPhoneText)
                return;

            try
            {
                _updatingPhoneText = true;

                string digits = new string(txtPhoneNumber.Text
                    .Where(char.IsDigit)
                    .ToArray());

                if (_currentCountry?.Code == "+98")
                {
                    if (digits.Length > 10)
                        digits = digits.Substring(0, 10);

                    if (digits.Length == 0)
                    {
                        txtHelper.Text = "";
                        txtPhoneNumber.Text = "";
                        return;
                    }

                    if (!digits.StartsWith("9"))
                    {
                        txtHelper.Text = "Iran's standard phone number starts with 9";
                        txtHelper.Foreground = Brushes.Orange;
                    }
                    else if (digits.Length < 10)
                    {
                        txtHelper.Text = "Your Phone Number Must have 10 numbers";
                        txtHelper.Foreground = Brushes.Orange;
                    }
                    else
                    {
                        txtHelper.Text = "Valid Number ✓";
                        txtHelper.Foreground = Brushes.LightGreen;
                    }

                    string formatted = digits;

                    if (formatted.Length > 3)
                        formatted = formatted.Insert(3, " ");

                    if (formatted.Length > 7)
                        formatted = formatted.Insert(7, " ");

                    if (txtPhoneNumber.Text != formatted)
                    {
                        txtPhoneNumber.Text = formatted;
                        txtPhoneNumber.CaretIndex = txtPhoneNumber.Text.Length;
                    }
                }
                else
                {
                    if (txtPhoneNumber.Text != digits)
                    {
                        txtPhoneNumber.Text = digits;
                        txtPhoneNumber.CaretIndex = txtPhoneNumber.Text.Length;
                    }
                }
            }
            finally
            {
                _updatingPhoneText = false;
            }
        }

        private void txtPhoneNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back && txtPhoneNumber.Text.EndsWith(" "))
            {
                txtPhoneNumber.Text = txtPhoneNumber.Text.TrimEnd();
                txtPhoneNumber.CaretIndex = txtPhoneNumber.Text.Length;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            string rawNumber = txtPhoneNumber.Text.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(rawNumber))
            {
                MessageBox.Show("Please Insert your phone number", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(txtHelper.Text != "Valid Number ✓")
            {
                MessageBox.Show("Your Phone Number is not valid please reassure that your phone number is valid", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                var host = (InfoWindow)Window.GetWindow(this);
                host.FrameRef.Navigate(new InfoPage3("next", _username,txtPhoneNumber.Text.Trim(),_tcpClient));
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);
            host.FrameRef.Navigate(new InfoPage1("back",_username));
        }
    }

}