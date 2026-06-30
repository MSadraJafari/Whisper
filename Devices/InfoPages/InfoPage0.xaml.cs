using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Devices.InfoPages
{
    public partial class InfoPage0 : Page
    {
        public string ServerIP { get; private set; } = "127.0.0.1";
        public int ServerPort { get; private set; } = 9999;

        private readonly string _status;
        public InfoPage0(string status,string IP= "127.0.0.1",int port = 9999)
        {
            InitializeComponent();
            
            _status = status;
            ServerIP = IP;
            ServerPort = port;

            Loaded += Page_Loaded;
        }

        //UI
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_status == "back")
            {
                txtServerIP.Text = ServerIP;
                txtServerPort.Text = ServerPort.ToString();
            }

            Root.Opacity = 0;

            var trans = new TranslateTransform();
            Root.RenderTransform = trans;

            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(420),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var slide = new DoubleAnimation
            {
                From = _status == "back" ? -60 : 60,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(420),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Root.BeginAnimation(OpacityProperty, fade);
            trans.BeginAnimation(TranslateTransform.XProperty, slide);
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var exitTasks = new[]
            {
                AnimateElementAsync(LogoElement,     0,   -220, 650),
                AnimateElementAsync(WhisperText,   -520,     0, 650),
                AnimateElementAsync(SetupText,        0,   -180, 650),
                AnimateElementAsync(HintText,         0,    180, 650),
                AnimateElementAsync(CardElement,      0,    260, 650),
                AnimateElementAsync(NextButton,     560,      0, 650),
                AnimateElementAsync(CancelButton,  -560,      0, 650)
            };

            var fadeOutPage = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(320),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Root.BeginAnimation(OpacityProperty, fadeOutPage);

            var host = Window.GetWindow(this);
            if (host != null)
            {
                var fadeOutWindow = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(320),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                host.BeginAnimation(Window.OpacityProperty, fadeOutWindow);
            }

            await Task.WhenAll(exitTasks);
            await Task.Delay(1000);

            host?.Close();
        }

        private static Task AnimateElementAsync(FrameworkElement element, double toX, double toY, int milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();

            var trans = new TranslateTransform();
            element.RenderTransform = trans;
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var easing = new CubicEase { EasingMode = EasingMode.EaseIn };

            var storyboard = new Storyboard();

            if (toX != 0)
            {
                var animX = new DoubleAnimation
                {
                    From = 0,
                    To = toX,
                    Duration = TimeSpan.FromMilliseconds(milliseconds),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(animX, element);
                Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                storyboard.Children.Add(animX);
            }

            if (toY != 0)
            {
                var animY = new DoubleAnimation
                {
                    From = 0,
                    To = toY,
                    Duration = TimeSpan.FromMilliseconds(milliseconds),
                    EasingFunction = easing
                };
                Storyboard.SetTarget(animY, element);
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(animY);
            }

            storyboard.Completed += (_, __) => tcs.TrySetResult(true);
            storyboard.Begin(element, true);

            return tcs.Task;
        }



        //Work

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
            ServerIP = txtServerIP.Text.Trim();

            Dispatcher.Invoke(() =>
            {
                var host = (InfoWindow)Window.GetWindow(this);
                host.FrameRef.Navigate(new InfoPage1("next",IPAddress.Parse(ServerIP),ServerPort));
            });
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}