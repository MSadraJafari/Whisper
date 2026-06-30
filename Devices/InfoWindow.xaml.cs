using Model;
using System.Windows;
using System.Windows.Controls;

namespace Devices
{
    public partial class InfoWindow : Window
    {
        public Client client { get; set; }
        public Frame FrameRef => MainFrame;

        public InfoWindow()
        {
            InitializeComponent();

            MainFrame.Navigate(
                new InfoPages.InfoPage0());
        }
    }
}