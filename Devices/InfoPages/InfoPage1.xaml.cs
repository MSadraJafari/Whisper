using Devices;
using Model;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace Devices.InfoPages
{
    public partial class InfoPage1 : Page
    {
        TcpClient tcpClien;
        private readonly string _username;
        object senderr = "";
        bool successful = false;
        private readonly string _status = "";
        RoutedEventArgs ee = new RoutedEventArgs();
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 9999;
        TcpClient MainTCPClient;
        public InfoPage1(string status, IPAddress ServerIP = null, int Port = 0, TcpClient tcpClient = null, string username = "")
        {
            InitializeComponent();
            Loaded += Page_Loaded;
            _status = status;
            this.ipAddress = ServerIP;
            this.port = Port;
            _username = username;
        }
        //---------------UI--------------------
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_status == "back")
                txtUsername.Text = _username;
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


        //-----------------Work------------------

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            senderr = sender;
            ee = e;
            if (!successful)
                Task.Run(new Action(() => { successful = TryConnectToServer(); }));
            else
            {
                Dispatcher.Invoke(() =>
                {
                    var host = (InfoWindow)Window.GetWindow(this);
                    host.FrameRef.Navigate(new InfoPage2("next", txtUsername.Text, tcpClien));
                });
            }
        }

        private bool TryConnectToServer()
        {
            try
            {
                string tu = "";
                Dispatcher.Invoke(() =>
                {
                    tu = txtUsername.Text;
                });
                if (tu == "")
                {
                    MessageBox.Show("Please enter username and address", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (_status == "back")
                {
                    tcpClien = MainTCPClient;
                }
                else
                {
                    tcpClien = new TcpClient();
                    tcpClien.Connect(ipAddress, port);
                }
                Client client = new Client(Encrypt("", "1234567812345678"), tcpClien);
                List<ClientSendingForUser> users = new List<ClientSendingForUser>();
                NetworkStream stream = tcpClien.GetStream();
                byte[] buffer = new byte[2000000];
                int size = stream.Read(buffer, 0, buffer.Length);
                Dispatcher.Invoke(() =>
                {
                    client = new Client(Encrypt(txtUsername.Text, "1234567812345678"), tcpClien);
                });
                if (buffer.Take(size).SequenceEqual(Encoding.UTF8.GetBytes("Welcome")))
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    string jj = Newtonsoft.Json.JsonConvert.SerializeObject(client);
                    jj = Encrypt(jj, "1234567812345678");
                    buffer = Encoding.UTF8.GetBytes($"{jj}");
                    stream.Write(buffer, 0, buffer.Length);
                    buffer = new byte[2000000];
                    size = stream.Read(buffer, 0, buffer.Length);
                    string theMessage = Decrypt(Encoding.UTF8.GetString(buffer, 0, size), "1234567122345108");
                    if (theMessage == "isRepeated")
                    {
                        MessageBox.Show("Username is already taken.\nPlease try again with another Username.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Dispatcher.Invoke(() => { txtUsername.Text = ""; });
                        Task.Run(() => { btnNext_Click(senderr, ee); });
                        return false;
                    }
                    else
                    {
                        Task.Run(() => { btnNext_Click(senderr, ee); });
                        return true;
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "No connection could be made because the target machine actively refused it 127.0.0.1:9999" || ex.Message == "No connection could be made because the target machine actively refused it. [::ffff:127.0.0.1]:9999")
                {
                    MessageBox.Show("Server is not started yet", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return false;
        }

        public static string Encrypt(string plainText, string password)
        {
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 8, 2 };
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(password);
                aes.IV = saltBytes;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string password)
        {
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 2, 3, 4, 5, 6, 7, 8, 2 };
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(password);
                aes.IV = saltBytes;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);

            if (string.IsNullOrEmpty(ipAddress?.ToString()) || port == null || port ==0)
                host.FrameRef.Navigate(new InfoPage0("back","127.0.0.1", 9999));
            else
                host.FrameRef.Navigate(new InfoPage0("back", ipAddress.ToString(), port));
        }
    }
}