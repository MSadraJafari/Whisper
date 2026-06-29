using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Devices
{
    public partial class IntroPage5 : Page
    {
        public IntroPage5(string status)
        {
            InitializeComponent();

            if (status == "next")
                Loaded += IntroPage2_LoadedNormal;
            else
                Loaded += IntroPage2_LoadedBacked;
        }

        private void IntroPage2_LoadedNormal(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var trans = new TranslateTransform();
            Root.RenderTransform = trans;

            var slide = new DoubleAnimation
            {
                From = 70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            Root.BeginAnimation(OpacityProperty, fadeIn);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void IntroPage2_LoadedBacked(object sender, RoutedEventArgs e)
        {
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var trans = new TranslateTransform();
            Root.RenderTransform = trans;

            var slide = new DoubleAnimation
            {
                From = -70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            Root.BeginAnimation(OpacityProperty, fadeIn);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            LaunchButton.IsEnabled = false;
            BackButton.IsEnabled = false;

            var exitTasks = new[]
            {
                AnimateElementAsync(LogoElement, 0, -120, 320),
                AnimateElementAsync(TitleStack, 0, 140, 320),
                AnimateElementAsync(LaunchButton, 160, 0, 320),
                AnimateElementAsync(BackButton, -160, 0, 320)
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

            var info = new InfoWindow();
            Application.Current.MainWindow = info;
            info.Opacity = 0;
            info.Show();

            var fadeInInfo = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(350),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            info.BeginAnimation(Window.OpacityProperty, fadeInInfo);

            host?.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null && this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
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
    }
}