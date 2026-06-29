using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for Window2FFI.xaml
    /// </summary>
    public partial class IntroPage2 : Page
    {
        public IntroPage2(string status)
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

            AnimateCardsSequentially();
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

            AnimateCardsSequentially();
        }

        private void AnimateCardsSequentially()
        {
            var cards = new[] { Card1, Card2, Card3, Card4 };
            var delays = new[] { 0, 80, 160, 240 };

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                var delay = TimeSpan.FromMilliseconds(delays[i]);

                var opacityAnim = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(320),
                    BeginTime = delay
                };


                var scaleAnim = new DoubleAnimation
                {
                    From = 0.75,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(320),
                    BeginTime = delay,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                var translateAnim = new DoubleAnimation
                {
                    From = (i % 2 == 0) ? -100 : 100,   // کارت‌های چپ از منفی، راست از مثبت
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(380),
                    BeginTime = delay,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                card.BeginAnimation(OpacityProperty, opacityAnim);

                if (card.RenderTransform is TransformGroup group)
                {
                    var translate = group.Children[0] as TranslateTransform;
                    var scale = group.Children[1] as ScaleTransform;

                    if (translate != null)
                        translate.BeginAnimation(TranslateTransform.XProperty, translateAnim);

                    if (scale != null)
                    {
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    }
                }
            }

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
                window.FrameRef.Navigate(new IntroPage3("next"));
            };
            this.BeginAnimation(OpacityProperty, fadeOut);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
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
                To = 70,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            fadeOut.Completed += (s, e2) =>
            {
                var window = (IntroWindow)Application.Current.MainWindow;
                window.FrameRef.Navigate(new IntroPage1("back"));
            };

            this.BeginAnimation(OpacityProperty, fadeOut);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }
    }

}
