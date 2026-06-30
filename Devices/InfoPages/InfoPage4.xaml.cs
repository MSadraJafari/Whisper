using Microsoft.Win32;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
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
using System.Xml.Serialization;

namespace Devices.InfoPages
{
    /// <summary>
    /// Interaction logic for InfoPage5.xaml
    /// </summary>
    public partial class InfoPage4 : Page
    {
        private readonly string _status;

        private readonly string _username;
        private readonly string _bioghraphy;
        private readonly string _birthday;
        private readonly TcpClient tcpClien;
        public InfoPage4(string status, string username, string bioghraphy, string birthday, TcpClient tcpClient)
        {
            _status = status;
            _username = username;
            tcpClien = tcpClient;
            _bioghraphy = bioghraphy;
            _birthday = birthday;
            InitializeComponent();
            Loaded += InfoPage5_Loaded;
        }

        private void InfoPage5_Loaded(object sender, RoutedEventArgs e)
        {
            //if (_status == "back")
            //    txtNickname.Text = _;

            txtPlaceholder.Visibility = Visibility.Visible;

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


            string DocumentOfUser = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string MainRootDirectory = System.IO.Path.Combine(DocumentOfUser, "Whisper");
            if (!Directory.Exists(MainRootDirectory))
            {
                Directory.CreateDirectory(MainRootDirectory);
            }
            string exePath = Assembly.GetExecutingAssembly().Location;
            string RootDirectoryOfProgramRunning = Convert.ToString(System.IO.Path.GetDirectoryName(exePath));
            string filesPreDi = System.IO.Path.Combine(RootDirectoryOfProgramRunning, "Required-Data");
            if (!Directory.Exists(filesPreDi))
            {
                Directory.CreateDirectory(filesPreDi);
            }

            string FullPathOfSMP = System.IO.Path.Combine(filesPreDi, "DefaultUserProfile.jpg");
            string DestinationPath = System.IO.Path.Combine(MainRootDirectory, "Data", "Essential", "DefaultUserProfile.jpg");
            if (!File.Exists(DestinationPath))
            {
                CopyFile(FullPathOfSMP, DestinationPath);
            }
            if (File.Exists(DestinationPath)) { }
            else
            {
                MessageBox.Show($"There is something wrong with one of files please try re-installing the program.\nOr put an icon of default user's profile picture in this directoty:\n{System.IO.Path.GetDirectoryName(DestinationPath)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BitmapImage image = new BitmapImage();

            using (FileStream stream = new FileStream(DestinationPath, FileMode.Open, FileAccess.Read))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
            }

            imgAvatar.Source = image;
            btnChoosePhoto.BringIntoView();
        }

        private void CopyFile(string sourseFilePath, string DestinationPath)
        {
            if (!File.Exists(sourseFilePath))
            {
                MessageBox.Show("There is no file with this name", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            string DestinationPathD = System.IO.Path.GetDirectoryName(DestinationPath);
            if (!Directory.Exists(DestinationPathD))
            {
                Directory.CreateDirectory(DestinationPathD);
            }
            File.Copy(sourseFilePath, DestinationPath);
        }

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoWindow host = new InfoWindow();
                List<ClientSendingForUser> users = new List<ClientSendingForUser>();
                int size;
                byte[] buffer = new byte[2000000];
                var stream = tcpClien.GetStream();
                buffer = new byte[2000000];
                Client clientNew = new Client(Encrypt(_username, "1234567812345678"), tcpClien)
                {
                    bio = _bioghraphy,
                    birthDay = _birthday,
                    tagName = txtNickname.Text,
                    loginDate = DateTime.Now
                };
                byte[] fileBytes;
                BitmapSource bitmapSource = (BitmapSource)imgAvatar.Source;

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    fileBytes = ms.ToArray();
                }
                string base64 = Convert.ToBase64String(fileBytes);
                DataModel dataModel = new DataModel();
                dataModel.isFile = true;
                dataModel.fileName = $"picture{_username}";
                dataModel.fileData = base64;
                clientNew.profilePicture = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
                buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(clientNew));
                stream.Write(buffer, 0, buffer.Length);
                buffer = new byte[2000000];
                size = stream.Read(buffer, 0, buffer.Length);
                string got = Encoding.UTF8.GetString(buffer, 0, size);
                if (got.EndsWith("╧"))
                {
                    got = got.Substring(0, got.IndexOf("╧"));
                    if (Decrypt(got, "1234567812345678") == "@@!!ddCan't continue3232")
                    {
                        MessageBox.Show($"Your information is not right for logining as a used username {Decrypt(_username, "1234567812345678")}\nPlease enter the right NickName", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        host = (InfoWindow)Window.GetWindow(this);
                        host.FrameRef.Navigate(new InfoPage3("back", _username, _bioghraphy, tcpClien, _birthday));
                        return;
                    }
                }
                GlobalDataModel globalDataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalDataModel>(got);
                if (!globalDataModel.isEncrypted)
                {
                    if (globalDataModel.type == "notification")
                    {
                        NotificationModel sModel = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationModel>(globalDataModel.body);
                        if (sModel.type == "users")
                        {
                            users = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientSendingForUser>>(sModel.data);
                        }
                    }
                }
                List<string> strings = new List<string>();
                foreach (ClientSendingForUser client1 in users)
                {
                    strings.Add(client1.username);
                }

                btnFinish.IsEnabled = false;
                btnBack.IsEnabled = false;

                var exitTasks = new[]
                {
                // Left Side
                AnimateElementAsync(LogoElement, 0, -180, 320),
                AnimateElementAsync(WhisperTitle, -180, 0, 320),
                AnimateElementAsync(PersianTitle, -220, 0, 320),
                AnimateElementAsync(IdentityTitle, -260, 0, 320),
                AnimateElementAsync(IdentityDescription, -300, 0, 320),
            
                // Avatar
                AnimateElementAsync(AvatarCircle, 0, -140, 320),
                AnimateElementAsync(AvatarBorder, 0, -160, 320),
                AnimateElementAsync(btnChoosePhoto, 120, 0, 320),
                AnimateElementAsync(txtPlaceholder, 0, -180, 320),
            
                // Nickname Section
                AnimateElementAsync(NicknameLabel, 180, 0, 320),
                AnimateElementAsync(txtNickname, 220, 0, 320),
                AnimateElementAsync(txtHelper, 260, 0, 320),
            
                // Header
                AnimateElementAsync(ProfileTitle, 0, -120, 320),
                AnimateElementAsync(ProfileDescription, 0, -150, 320),
            
                // Buttons
                AnimateElementAsync(btnBack, -180, 0, 320),
                AnimateElementAsync(btnFinish, 180, 0, 320)
            };

                var fadeOutPage = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(320),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                Root.BeginAnimation(OpacityProperty, fadeOutPage);

                host = (InfoWindow)Window.GetWindow(this);
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

                var info = new MainWindow(clientNew, users, tcpClien);
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
            catch (Exception ex)
            {
                if (ex.Message == "No connection could be made because the target machine actively refused it 127.0.0.1:5000")
                {
                    MessageBox.Show("Server is not started yet", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

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
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var host = (InfoWindow)Window.GetWindow(this);
            host.FrameRef.Navigate(new InfoPage3("back", _username, _bioghraphy, tcpClien, _birthday));
        }

        private void btnChoosePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Choose Profile Picture",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.webp"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BitmapImage image = new BitmapImage();

                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                }

                CropImage cropWindow = new CropImage(image)
                {
                    Owner = Window.GetWindow(this)
                };

                bool? result = cropWindow.ShowDialog();

                if (result == true && cropWindow.CroppedImage != null)
                {
                    imgAvatar.Source = cropWindow.CroppedImage;
                    txtPlaceholder.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        private void txtNickname_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNickname.Text.Length > 32)
            {
                string text = txtNickname.Text;
                text = text.Substring(0, 32);
                txtNickname.Text = text;
                txtNickname.CaretIndex = txtNickname.Text.Length;

                txtHelper.Text = "Your NickName cannot have more than 32 characters.";
                txtHelper.Foreground = Brushes.Orange;
            }
            if (!txtNickname.Text.IsNormalized())
            {
                if (lastTyped == ' ') { }
                else
                {
                    string text = txtNickname.Text;
                    text = text.Remove(text.IndexOf(lastTyped));
                    txtNickname.Text = text;

                    txtHelper.Text = "You cannot insert invalid characters in your Nickname.";
                    txtHelper.Foreground = Brushes.Orange;
                }
            }
        }
        char lastTyped = ' ';
        private void txtNickname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            lastTyped = e.Text[0];
        }
    }
}
