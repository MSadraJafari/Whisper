using System.Windows;
using System.Windows.Controls;

namespace Devices
{
    public partial class IntroWindow : Window
    {
        public IntroWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(new IntroPage1("next"));
        }

        public Frame FrameRef
        {
            get { return MainFrame; }
        }
    }
}