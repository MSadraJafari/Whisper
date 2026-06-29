using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
namespace Devices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class IntroPage1 : Page
    {
        public IntroPage1(string status)
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
            this.RenderTransform = trans;

            var slide = new DoubleAnimation
            {
                From = 70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
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
            this.RenderTransform = trans;

            var slide = new DoubleAnimation
            {
                From = -70,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };
            this.BeginAnimation(OpacityProperty, fadeIn);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            var trans = new TranslateTransform();
            this.RenderTransform = trans;

            var slide = new DoubleAnimation
            {
                From = 0,
                To = -70,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            fadeOut.Completed += (s, e2) =>
            {
                var window = (IntroWindow)Application.Current.MainWindow;
                window.FrameRef.Navigate(new IntroPage2("next"));
            };

            this.BeginAnimation(OpacityProperty, fadeOut);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private async void btnBack_Click(object sender, RoutedEventArgs e)
        {
            btnBack.IsEnabled = false;
            btnNext.IsEnabled = false;

            AnimateOut(LogoElement, 0, -220, 650);
            AnimateOut(WhisperText, -520, 0, 650);
            AnimateOut(PersianText, 520, 0, 650);
            AnimateOut(ASTM, 0, -180, 650);
            AnimateOut(FSL, 0, 180, 650);
            AnimateOut(btnBack, -560, 0, 650);
            AnimateOut(btnNext, 560, 0, 650);

            var fade = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(700),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            PageContainer.BeginAnimation(OpacityProperty, fade);

            await Task.Delay(1000);
            Application.Current.Shutdown();
        }

        private void AnimateOut(FrameworkElement element, double x, double y, int ms)
        {
            var trans = new TranslateTransform();
            element.RenderTransform = trans;
            element.RenderTransformOrigin = new Point(0.5, 0.5);

            var easing = new QuinticEase
            {
                EasingMode = EasingMode.EaseIn
            };

            var animX = new DoubleAnimation
            {
                From = 0,
                To = x,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = easing
            };

            var animY = new DoubleAnimation
            {
                From = 0,
                To = y,
                Duration = TimeSpan.FromMilliseconds(ms),
                EasingFunction = easing
            };

            var opacity = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(ms - 50),
                EasingFunction = easing
            };

            trans.BeginAnimation(TranslateTransform.XProperty, animX);
            trans.BeginAnimation(TranslateTransform.YProperty, animY);
            element.BeginAnimation(OpacityProperty, opacity);
        }



    }
}