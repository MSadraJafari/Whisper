using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace Devices
{
    /// <summary>
    /// Interaction logic for IntroPage4.xaml
    /// </summary>
    public partial class IntroPage4 : Page
    {
        public IntroPage4(string status)
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

        private void txtGitHub_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MSadraJafari",
                    UseShellExecute = true
                });
            }
            catch { }
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
                window.FrameRef.Navigate(new IntroPage3("back"));
            };

            this.BeginAnimation(OpacityProperty, fadeOut);
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
                window.FrameRef.Navigate(new IntroPage5("next"));
            };
            this.BeginAnimation(OpacityProperty, fadeOut);
            trans.BeginAnimation(TranslateTransform.YProperty, slide);
        }
    }
}
