using Microsoft.Win32;
using Model;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
namespace Devices
{
    public partial class MainWindow : Window
    {
        string RootDirectoryOfProgramRunning = "";
        private string MainRootDirectory = "";
        TcpClient Thisclient;
        NetworkStream stream;
        Client client;
        bool isRunning = true;
        int stt = 0;
        string biographyForYaroo = "";
        List<ClientSendingForUser> clients = new List<ClientSendingForUser>();
        bool isFullPBOX = false;
        string SavedMesssagesIconPath = "";
        List<DataModel> MessagesWaitingForCorrection = new List<DataModel>();
        private bool isClosing = false;
        public MainWindow(Client client, List<ClientSendingForUser> clients, TcpClient tcpClient)
        {
            InitializeComponent();
            string DocumentOfUser = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            MainRootDirectory = Path.Combine(DocumentOfUser, "Whisper");
            if (!Directory.Exists(MainRootDirectory))
            {
                Directory.CreateDirectory(MainRootDirectory);
            }
            Thisclient = tcpClient;
            stream = tcpClient.GetStream();
            this.client = client;
            client.username = Decrypt(client.username, "1234567812345678");

            string exePath = Assembly.GetExecutingAssembly().Location;
            RootDirectoryOfProgramRunning = Convert.ToString(Path.GetDirectoryName(exePath));
            string directOfPerson = Path.Combine(MainRootDirectory, $"Client{client.username}Data");
            Directory.CreateDirectory(directOfPerson);
            string filesPreDi = Path.Combine(RootDirectoryOfProgramRunning, "Required-Data");
            if (!Directory.Exists(filesPreDi))
            {
                Directory.CreateDirectory(filesPreDi);
            }

            string FullPathOfSMP = Path.Combine(filesPreDi, "SavedMessagesIcon.png");
            string DestinationPath = Path.Combine(MainRootDirectory, "Data", "Essential", "SavedMessagesIcon.png");
            if (!File.Exists(DestinationPath))
            {
                CopyFile(FullPathOfSMP, DestinationPath);
            }
            if (File.Exists(DestinationPath))
            {
                SavedMesssagesIconPath = DestinationPath;
            }
            else
            {
                MessageBox.Show($"There is something wrong with one of files please try re-installing the program.\nOr put an icon of saved messages in this directoty:\n{Path.GetDirectoryName(DestinationPath)}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Hide();
                this.Close();
                return;
            }
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RunLoad();
        }

        private void RunLoad()
        {
            RootGrid.Opacity = 0;

            ScaleTransform scale = new ScaleTransform(0.95, 0.95);

            RootGrid.RenderTransform = scale;
            RootGrid.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            DoubleAnimation fade =
                new DoubleAnimation(0, 1,
                TimeSpan.FromMilliseconds(500));

            DoubleAnimation scaleX =
                new DoubleAnimation(0.95, 1,
                TimeSpan.FromMilliseconds(500));

            DoubleAnimation scaleY =
                new DoubleAnimation(0.95, 1,
                TimeSpan.FromMilliseconds(500));

            RootGrid.BeginAnimation(OpacityProperty, fade);

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);

            txtSearch.Text = "Search";
            lblUsername.Text = $"Your username: {client.username}";
            Task.Run(() =>
            {
                setInfoRight();
                Dispatcher.Invoke(new Action(() =>
                {
                    LoadImage(imgSavedMessages, SavedMesssagesIconPath);
                }));
            });
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ////}
            Task.Run(() =>
            {
                string cach = "";
                while (isRunning)
                {
                    //try
                    //{
                    byte[] buffer = new byte[8192];
                    int size = 0;
                    try
                    {
                        size = stream.Read(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        if (isClosing)
                        {
                            isRunning = false;
                            return;
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    string whatReceived = Encoding.UTF8.GetString(buffer, 0, size);
                    if (whatReceived.Contains("Server Started!@#100003"))
                    {
                        continue;
                    }
                    if (whatReceived.Contains("Stopping Server!!!56"))
                    {
                        stt = 0;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            MessageBox.Show("Server has been Stopped!\nPlease do not try to send messages until any further messages.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            ContactStatus.Text = "Server has been stopped.";
                            ContactStatus.Foreground = Brushes.Orange;
                        }));
                        Thread thread = new Thread(() => listenerWhenServerIsStopped());
                        thread.Start();
                        isRunning = false;
                        break;
                    }
                    if (whatReceived.EndsWith("╦"))
                    {
                        if (Decrypt(whatReceived, "1234567812345678") == "Client is closing7779822!!")
                        {
                            isRunning = false;
                            break;
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            string gotRaw = Encoding.UTF8.GetString(buffer, 0, size);
                            DataModel dataModel = new DataModel();
                            NotificationModel notificationModel = new NotificationModel();
                            cach += gotRaw;
                            while (cach.Contains("╬"))
                            {
                                if (string.IsNullOrWhiteSpace(cach))
                                {
                                    continue;
                                }
                                int index = cach.IndexOf("╬");
                                string message = cach.Substring(0, index);
                                cach = cach.Substring(index + 1);
                                if (string.IsNullOrWhiteSpace(message))
                                    continue;
                                GlobalDataModel got = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalDataModel>(message);
                                if (got.type == "notification")
                                {
                                    if (!got.isEncrypted)
                                    {
                                        notificationModel = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationModel>(got.body);
                                        if (notificationModel.type == "loggedIns")
                                        {
                                            List<ClientSendingForUser> unames = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ClientSendingForUser>>(notificationModel.data);
                                            clients = unames;
                                            Dispatcher.Invoke(new Action(() =>
                                            {
                                                lstChats.Items.Clear();
                                                foreach (ClientSendingForUser csfu in unames)
                                                {
                                                    if (csfu.username == client.username)
                                                    {
                                                        lstChats.Items.Add("Saved Messages");
                                                    }
                                                    else
                                                        lstChats.Items.Add(csfu.tagName);
                                                }
                                            }));
                                        }
                                    }
                                }
                                else if (got.type == "MessageCorrectionBack")
                                {
                                    CorrectionData correctionDataS = JsonConvert.DeserializeObject<CorrectionData>(got.body);
                                    if (got.isEncrypted)
                                    {
                                        foreach (DataModel dm in MessagesWaitingForCorrection.ToList())
                                        {
                                            if (dm.Sign == correctionDataS.Sign)
                                            {
                                                MessagesWaitingForCorrection.Remove(dm);
                                                if (Decrypt(correctionDataS.message, "1234567812345678") == "CorrectGotIt9247813")
                                                {
                                                    if (dm.isFile)
                                                    {
                                                        byte[] fileBytes = Convert.FromBase64String(Decrypt(dm.fileData, "1234567812345678"));

                                                        string folderPath = Path.Combine(MainRootDirectory, "History",
                                                                                         client.username, dm.receiver, "Uploads");
                                                        Directory.CreateDirectory(folderPath);

                                                        string filePath = Path.Combine(folderPath, dm.fileName);
                                                        File.WriteAllBytes(filePath, fileBytes);

                                                        dm.message = "File Sent.";
                                                        dm.fileData = filePath;

                                                        string historyPath = Path.Combine(MainRootDirectory, "History",
                                                                                          client.username, dm.receiver, "chat.txt");
                                                        Directory.CreateDirectory(Path.GetDirectoryName(historyPath));

                                                        string serialized = JsonConvert.SerializeObject(dm);
                                                        File.AppendAllText(historyPath, serialized + Environment.NewLine);

                                                        Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            lstMessages.Items.Add(new ChatMessage
                                                            {
                                                                Text = dm.fileName + " (Done)",
                                                                Time = dataModel.time.ToString("HH:mm"),
                                                                IsMine = true
                                                            });
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        string historyPath = Path.Combine(
                                                            MainRootDirectory, "History", client.username, dm.receiver, "chat.txt");
                                                        if (!Directory.Exists(Path.GetDirectoryName(historyPath)))
                                                            Directory.CreateDirectory(Path.GetDirectoryName(historyPath));

                                                        File.AppendAllText(historyPath, JsonConvert.SerializeObject(dm) + Environment.NewLine);
                                                    }
                                                }
                                                else
                                                {
                                                    if (dm.isFile)
                                                    {
                                                        Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            lstMessages.Items.Add(new ChatMessage
                                                            {
                                                                Text = dm.fileName + " (Not Sent)",
                                                                Time = DateTime.Now.ToString("HH:mm"),
                                                                IsMine = true
                                                            });
                                                        }));
                                                    }
                                                    else
                                                    {
                                                        Dispatcher.Invoke(new Action(() =>
                                                        {
                                                            lstMessages.Items.Add(new ChatMessage
                                                            {
                                                                Text = Decrypt(dm.message, "1234567812345678") + " (Couldn't Send)",
                                                                Time = DateTime.Now.ToString("HH:mm"),
                                                                IsMine = true
                                                            });
                                                        }));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (got.type == "message")
                                {
                                    if (got.isEncrypted)
                                    {
                                        string salt = "da;%$%DST%^%$DTFGFFFDddd";
                                        dataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                                        string path = Path.Combine(MainRootDirectory, "History", client.username, dataModel.sender.ToString());
                                        DataModel Clone = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                                        dataModel.Sign = null;
                                        var hash = ComputeMD5Hash(Newtonsoft.Json.JsonConvert.SerializeObject(dataModel));
                                        string ss = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
                                        hash = ComputeMD5Hash(hash + salt);
                                        if (Clone.Sign == hash)
                                        {
                                            CorrectionData correctionData = new CorrectionData()
                                            {
                                                sender = client.username,
                                                receiver = Clone.sender,
                                                message = Encrypt("CorrectGotIt9247813", "1234567812345678"),
                                                Sign = Clone.Sign
                                            };
                                            GlobalDataModel globalDataModel = new GlobalDataModel()
                                            {
                                                type = "messageCorrection",
                                                isEncrypted = true,
                                                body = JsonConvert.SerializeObject(correctionData)
                                            };
                                            buffer = new byte[4096];
                                            buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                            stream.Write(buffer, 0, buffer.Length);
                                            if (dataModel.receiver == "Saved Messages") { }
                                            else
                                            {
                                                string text = got.body + Environment.NewLine;
                                                if (File.Exists(Path.Combine(path, "chat.txt")))
                                                {
                                                    File.AppendAllText(Path.Combine(path, "chat.txt"), text);
                                                    if (lstChats.SelectedItem != null)
                                                    {
                                                        if (getUserName(lstChats.SelectedItem.ToString()).ToString() == dataModel.sender)
                                                        {
                                                            appendToTxtBox(dataModel);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (!Directory.Exists(path))
                                                        Directory.CreateDirectory(path);
                                                    path = Path.Combine(path, "chat.txt");
                                                    File.AppendAllText(path, "");
                                                    File.AppendAllText(path, text);
                                                    if (lstChats.SelectedItem != null)
                                                    {
                                                        if (getUserName(lstChats.SelectedItem.ToString()).ToString() == dataModel.sender)
                                                        {
                                                            appendToTxtBox(dataModel);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            CorrectionData correctionData = new CorrectionData()
                                            {
                                                sender = client.username,
                                                receiver = Clone.sender,
                                                message = Encrypt("NotSent87#1645fasfsd4", "1234567812345678"),
                                                Sign = Clone.Sign
                                            };
                                            GlobalDataModel globalDataModel = new GlobalDataModel()
                                            {
                                                type = "messageCorrection",
                                                isEncrypted = true,
                                                body = JsonConvert.SerializeObject(correctionData)
                                            };
                                            buffer = new byte[4096];
                                            buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                            stream.Write(buffer, 0, buffer.Length);
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("You have a prob in 578 not encrypted", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else if (got.type == "file")
                                {
                                    dataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                                    string path = Path.Combine(MainRootDirectory, "History", client.username, dataModel.sender.ToString());
                                    DataModel clone = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                                    dataModel.Sign = null;
                                    string serializedTesting = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
                                    string salt = "da;%$%DST%^%$DTFGFFFDddd";
                                    string hash = ComputeMD5Hash(ComputeMD5Hash(serializedTesting) + salt);
                                    if (clone.Sign == hash)
                                    {
                                        CorrectionData correctionData = new CorrectionData()
                                        {
                                            sender = client.username,
                                            receiver = clone.sender,
                                            message = Encrypt("CorrectGotIt9247813", "1234567812345678"),
                                            Sign = clone.Sign
                                        };
                                        GlobalDataModel globalDataModel = new GlobalDataModel()
                                        {
                                            type = "messageCorrection",
                                            isEncrypted = true,
                                            body = JsonConvert.SerializeObject(correctionData)
                                        };
                                        buffer = new byte[4096];
                                        buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                        stream.Write(buffer, 0, buffer.Length);
                                        byte[] fileBytes = Convert.FromBase64String(Decrypt(dataModel.fileData, "1234567812345678"));
                                        string folderPath = Path.Combine(MainRootDirectory, "History",
                             client.username, dataModel.sender, "Downloads");
                                        Directory.CreateDirectory(folderPath);
                                        string filePath = Path.Combine(folderPath, dataModel.fileName);
                                        File.WriteAllBytes(filePath, fileBytes);
                                        string historyPath = Path.Combine(
                                            MainRootDirectory, "History",
                                            client.username,
                                            dataModel.sender,
                                            "chat.txt");
                                        string dPath = Path.Combine(
                                            MainRootDirectory, "History",
                                            client.username,
                                            dataModel.sender);
                                        Directory.CreateDirectory(dPath);
                                        dataModel.fileData = filePath;
                                        dataModel.message = "File Recieved!";
                                        string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
                                        File.AppendAllText(historyPath, serialized + Environment.NewLine);
                                        if (lstChats.SelectedItem != null)
                                        {
                                            if (getUserName(lstChats.SelectedItem.ToString()).ToString() == clone.sender)
                                            {
                                                appendToTxtBox(dataModel);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        CorrectionData correctionData = new CorrectionData()
                                        {
                                            sender = client.username,
                                            receiver = clone.sender,
                                            message = Encrypt("NotSent87#1645fasfsd4", "1234567812345678"),
                                            Sign = clone.Sign
                                        };
                                        GlobalDataModel globalDataModel = new GlobalDataModel()
                                        {
                                            type = "messageCorrection",
                                            isEncrypted = true,
                                            body = JsonConvert.SerializeObject(correctionData)
                                        };
                                        buffer = new byte[4096];
                                        buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                        stream.Write(buffer, 0, buffer.Length);
                                    }
                                }
                            }

                        }));

                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //}
                }

            });
        }
        private void CopyFile(string sourseFilePath, string DestinationPath)
        {
            if (!File.Exists(sourseFilePath))
            {
                MessageBox.Show("There is no file with this name", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            string DestinationPathD = Path.GetDirectoryName(DestinationPath);
            if (!Directory.Exists(DestinationPathD))
            {
                Directory.CreateDirectory(DestinationPathD);
            }
            File.Copy(sourseFilePath, DestinationPath);
        }
        private void btnSend_Click(object sender, EventArgs e)
        {
            //try
            //{
            if (txtMessage.Text != "")
            {
                if (lstChats.SelectedItem.ToString() != "")
                {
                    Task.Run(new Action(() => { sendThisAction(); }));
                }
                else
                {
                    MessageBox.Show("You cannot send message to no one", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("You cannot send empty message", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        private void sendThisAction(string messager = null, Client clientForSendThisAction = null)
        {
            //try
            //{
            string receiver = "";
            bool isFile = false;
            bool isClient = false;
            Dispatcher.Invoke(() =>
            {
                if (messager == null)
                    messager = txtMessage.Text;
                else
                    isFile = true;
            });
            if (clientForSendThisAction == null) { }
            else
                isClient = true;
            if (isRunning)
            {
                byte[] buffer = new byte[8192];
                Dispatcher.Invoke(new Action(() =>
                {
                    DataModel whichIsGonnaBeInList = new DataModel();
                    if (isFile)
                    {
                        DataModel model = JsonConvert.DeserializeObject<DataModel>(messager);
                        receiver = model.receiver;

                        model.Sign = null;

                        string serializedModel = JsonConvert.SerializeObject(model);
                        string salt = "da;%$%DST%^%$DTFGFFFDddd";
                        model.Sign = ComputeMD5Hash(ComputeMD5Hash(serializedModel) + salt);

                        GlobalDataModel globalDataModel = new GlobalDataModel()
                        {
                            type = "file",
                            body = JsonConvert.SerializeObject(model),
                            isEncrypted = true
                        };

                        string finalMessage = JsonConvert.SerializeObject(globalDataModel) + "╬";
                        buffer = Encoding.UTF8.GetBytes(finalMessage);

                        whichIsGonnaBeInList = model;
                    }
                    else if (isClient)
                    {
                        GlobalDataModel globalDataModel = new GlobalDataModel()
                        {
                            body = Newtonsoft.Json.JsonConvert.SerializeObject(client),
                            isEncrypted = false,
                            type = "UpdateClientInfo"
                        };
                        string alright = Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬";
                        buffer = Encoding.UTF8.GetBytes(alright);
                    }
                    else
                    {
                        DataModel dataModel = new DataModel();
                        dataModel.time = DateTime.Now;
                        dataModel.message = Encrypt(messager, "1234567812345678");
                        dataModel.sender = client.username;
                        dataModel.receiver = getUserName(lstChats.SelectedItem.ToString()).ToString();
                        dataModel.isFile = isFile;
                        receiver = dataModel.receiver;
                        string serialized = JsonConvert.SerializeObject(dataModel);
                        var hash = ComputeMD5Hash(serialized);
                        string salt = "da;%$%DST%^%$DTFGFFFDddd";
                        hash = ComputeMD5Hash(hash + salt);
                        dataModel.Sign = hash.ToString();
                        serialized = JsonConvert.SerializeObject(dataModel);
                        GlobalDataModel globalDataModel = new GlobalDataModel()
                        {
                            type = "message",
                            body = serialized,
                            isEncrypted = true
                        };
                        serialized = JsonConvert.SerializeObject(globalDataModel);
                        serialized += "╬";
                        buffer = new byte[8192];
                        buffer = Encoding.UTF8.GetBytes(serialized);
                        whichIsGonnaBeInList = dataModel;
                    }
                    if (receiver != "Saved Messages")
                    {
                        stream.Write(buffer, 0, buffer.Length);
                        MessagesWaitingForCorrection.Add(whichIsGonnaBeInList);
                    }
                    else
                    {
                        string RootDirectoryForSavedMessages = Path.Combine(MainRootDirectory, "History", client.username, getUserName(lstChats.SelectedItem.ToString()).ToString());
                        if (!Directory.Exists(RootDirectoryForSavedMessages))
                        {
                            Directory.CreateDirectory(RootDirectoryForSavedMessages);
                        }
                        string FilePath = Path.Combine(RootDirectoryForSavedMessages, "chat.txt");
                        File.AppendAllText(FilePath, "");
                        if (!whichIsGonnaBeInList.isFile)
                            File.AppendAllText(FilePath, JsonConvert.SerializeObject(whichIsGonnaBeInList) + Environment.NewLine);
                        else
                        {
                            byte[] fileBytes = Convert.FromBase64String(Decrypt(whichIsGonnaBeInList.fileData, "1234567812345678"));

                            string folderPath = Path.Combine(MainRootDirectory, "History",
                                                             client.username, getUserName(lstChats.SelectedItem.ToString()).ToString(), "Uploads");
                            Directory.CreateDirectory(folderPath);

                            string filePath = Path.Combine(folderPath, whichIsGonnaBeInList.fileName);
                            File.WriteAllBytes(filePath, fileBytes);

                            whichIsGonnaBeInList.message = "File Sent.";
                            whichIsGonnaBeInList.fileData = filePath;

                            string historyPath = Path.Combine(MainRootDirectory, "History",
                                                              client.username, getUserName(lstChats.SelectedItem.ToString()).ToString(), "chat.txt");
                            Directory.CreateDirectory(Path.GetDirectoryName(historyPath));

                            string serialized = JsonConvert.SerializeObject(whichIsGonnaBeInList);
                            File.AppendAllText(historyPath, serialized + Environment.NewLine);
                        }
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (isFile)
                        {
                            lstMessages.Items.Add(
                                new ChatMessage
                                {
                                    Text = "File is being sent...",
                                    Time = DateTime.Now.ToString("HH:mm"),
                                    IsMine = true
                                }
                            );
                            lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                        }
                        else
                        {
                            lstMessages.Items.Add(
                                new ChatMessage
                                {
                                    Text = txtMessage.Text,
                                    Time = DateTime.Now.ToString("HH:mm"),
                                    IsMine = true
                                }
                            );
                            lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                            txtMessage.Clear();
                            txtMessage.Foreground = Brushes.Gray;
                            txtMessage.Text += "Type your message here";
                        }
                    }));


                }));

            }
            else
            {
                MessageBox.Show("Server has been stopped please try again later.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }
        private void setInfoRight()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                DataModel dataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(client.profilePicture);
                byte[] fileBytes = Convert.FromBase64String(dataModel.fileData);
                string clientFolder = Path.Combine(MainRootDirectory, $"Client{client.username}Data");
                Directory.CreateDirectory(clientFolder);
                string filePath = Path.Combine(clientFolder, "profile.jpg");
                File.WriteAllBytes(filePath, fileBytes);
                LoadImage(imgProfileBrush, filePath);
            }));
            Dispatcher.Invoke(new Action(() =>
            {
                lblBirthday.Text = client.birthDay;
                try
                {
                    string forNow = client.bio.ToString();
                    int index = 1;
                    string final = "";
                    foreach (char var in forNow)
                    {
                        if (index == 13)
                        {
                            final += var;
                            if (forNow[index] == ' ')
                                index = 0;
                            else
                            {
                                int ind = 0;
                                foreach (char var2 in forNow)
                                {
                                    if (ind != index)
                                        continue;
                                    else
                                    {
                                        if (forNow[ind] == ' ')
                                        {
                                            final += '\n';
                                            break;
                                        }
                                        else
                                        {
                                            if (forNow[(ind + 1)] == ' ' || forNow[(ind + 2)] == ' ' || forNow[(ind + 3)] == ' ')
                                            {
                                                final += var2;
                                            }
                                            else
                                            {
                                                int iii = 0;
                                                for (int jii = ind; forNow[jii] == ' '; jii++)
                                                    iii++;
                                                if (iii < 5)
                                                    lblBio.Text += iii;
                                                else
                                                {
                                                    int imi = 0;
                                                    for (int jii = ind; forNow[jii] == ' '; jii--)
                                                        imi++;
                                                    final += '\n';
                                                }
                                            }
                                        }
                                    }
                                }
                            }


                        }
                        else
                            final += var;
                        index++;
                    }
                    lblBio.Text = final;
                }
                catch (Exception e)
                {

                }
            }));
            Dispatcher.Invoke(new Action(() =>
            {
                lblNickname.Text = client.tagName.ToString();
            }));
        }
        private string getUserName(string given)
        {
            if (given == "Saved Messages")
            {
                return given;
            }
            foreach (ClientSendingForUser clientSendingforGUN in clients)
            {
                if (given == clientSendingforGUN.tagName)
                {
                    given = clientSendingforGUN.username;
                }
            }
            return given;
        }

        private void appendToTxtBox(DataModel dataModel)
        {
            if (dataModel.sender != client.username)
            {
                if (dataModel.isFile)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        lstMessages.Items.Add(new ChatMessage
                        {
                            Text = $"File received: file:///{dataModel.fileData}",
                            Time = DateTime.Now.ToString("HH:mm"),
                            IsMine = false
                        });
                        lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                    }));
                }
                else
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        lstMessages.Items.Add(new ChatMessage
                        {
                            Text = Decrypt(dataModel.message, "1234567812345678"),
                            Time = DateTime.Now.ToString("HH:mm"),
                            IsMine = false
                        });
                        lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                    }));
                }
            }
        }
        private void listenerWhenServerIsStopped()
        {
            while (true)
            {
                //try
                //{
                byte[] dda = new byte[8192];
                int ize = stream.Read(dda, 0, dda.Length);
                string Received = Encoding.UTF8.GetString(dda);
                if (Received.Contains("Server Started!@#100003") && stt == 0)
                {
                    stt += 1;
                    isRunning = true;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Server has been started!\nYou can send your messages now.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        ContactStatus.Text = "Server has been stopped.";
                        ContactStatus.Foreground = Brushes.Orange;
                    }));
                    RunLoad();
                    break;
                }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
        }

        static string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Convert the byte array to a hexadecimal string.
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Format as hexadecimal
                }
                return sb.ToString();
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

        private void lstChats_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                lstMessages.Items.Clear();
            }));
            Task.Run(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (lstChats.SelectedItem == null) return;

                    string name = lstChats.SelectedItem.ToString();
                    if (string.IsNullOrEmpty(name)) return;

                    if (name == "Saved Messages")
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            usNameYaroo.Text = "Saved Messages";
                            ContactStatus.Text = "You can save your messages here.";
                            lblUsername.Text = "Your username: " + client.username;
                        }));
                        Dispatcher.Invoke(new Action(() =>
                        {
                            if (isFullPBOX)
                            {
                                imgProfileBrush.ImageSource = null;
                                LoadImage(imgProfileContactBrush, SavedMesssagesIconPath);
                            }
                            else
                                LoadImage(imgProfileContactBrush, SavedMesssagesIconPath);
                        }));
                        return;
                    }
                    else
                    {
                        if (isFullPBOX)
                        {
                            imgProfileContactBrush.ImageSource = null;
                        }
                        else
                            isFullPBOX = true;
                        ClientSendingForUser csf = new ClientSendingForUser();
                        foreach (ClientSendingForUser cliennt in clients)
                        {
                            if (cliennt.username == getUserName(name))
                            {
                                csf = cliennt;
                            }
                        }
                        DataModel dataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(csf.profPicture);
                        byte[] fileBytes = Convert.FromBase64String(dataModel.fileData);

                        string baseFolder = Path.Combine(MainRootDirectory, "Data");
                        Directory.CreateDirectory(baseFolder);

                        string clientFolder = Path.Combine(baseFolder, $"Client{client.username}Data");
                        Directory.CreateDirectory(clientFolder);

                        string filePath = Path.Combine(clientFolder, $"profile{csf.username}.jpg");
                        File.WriteAllBytes(filePath, fileBytes);
                        Dispatcher.Invoke(new Action(() =>
                        {
                            Dispatcher.Invoke(new Action(() => { LoadImage(imgProfileContactBrush, filePath); }));
                        }));
                        Dispatcher.Invoke(new Action(() =>
                        {
                            lblUsername.Text = "This User's Username: Hidden";
                            usNameYaroo.Text = csf.tagName;
                            lblBio.Text = csf.bio;
                            lblNickname.Text = csf.tagName;
                            lblBirthday.Text = "Hidden";
                            ContactStatus.Text = "Online";
                        }));
                    }
                }));
            });
            Task.Run(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if (lstChats.SelectedItem == null) { return; }

                    string path = Path.Combine(MainRootDirectory, "History", client.username, getUserName(lstChats.SelectedItem.ToString()).ToString(), "chat.txt");
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }
                    if (File.Exists(path))
                    {
                        using (StreamReader ss = new StreamReader(path))
                        {
                            string line;
                            while ((line = ss.ReadLine()) != null)
                            {
                                DataModel dataModel = JsonConvert.DeserializeObject<DataModel>(line);
                                if (dataModel.message == null)
                                    break;
                                if (!dataModel.isFile)
                                {
                                    if (Decrypt(dataModel.message, "1234567812345678") != null)
                                    {
                                        if (dataModel.sender == client.username)
                                        {
                                            lstMessages.Items.Add(new ChatMessage
                                            {
                                                Text = Decrypt(dataModel.message, "1234567812345678"),
                                                Time = dataModel.time.ToString("HH:mm"),
                                                IsMine = true
                                            });
                                        }
                                        else
                                        {
                                            lstMessages.Items.Add(new ChatMessage
                                            {
                                                Text = Decrypt(dataModel.message, "1234567812345678"),
                                                Time = dataModel.time.ToString("HH:mm"),
                                                IsMine = false
                                            });
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("File chat.txt is edited somehow please try deleting the folder *Whisper* in Document folder.\nTHIS WILL DELETE YOUR PROGRAM'S DATA!");
                                    }
                                }
                                else if (dataModel.isFile)
                                {
                                    if (File.Exists(dataModel.fileData))
                                    {
                                        if (dataModel.sender == client.username)
                                        {
                                            lstMessages.Items.Add(new ChatMessage
                                            {
                                                Text = "Uploaded File: " + "file:///" + dataModel.fileData,
                                                Time = dataModel.time.ToString("HH:mm"),
                                                IsMine = true
                                            });
                                        }
                                        else
                                        {
                                            lstMessages.Items.Add(new ChatMessage
                                            {
                                                Text = "Downloaded File: " + "file:///" + dataModel.fileData,
                                                Time = dataModel.time.ToString("HH:mm"),
                                                IsMine = false
                                            });
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("File is missing from disk.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("An unknown type of message recongnized!\n(Around 1032)", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            if (lstMessages.Items.Count != 0)
                                lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                        }
                    }
                    else
                    {
                        string folderPath = Path.Combine(MainRootDirectory, $@"History\{client.username}\{getUserName(lstChats.SelectedItem.ToString())}");
                        Directory.CreateDirectory(folderPath);
                        string filePath = Path.Combine(folderPath, "chat.txt");
                        File.AppendAllText(filePath, "");
                    }

                }));
            });
        }

        private void txtWhatIWrite_MouseClick(object sender, MouseEventArgs e)
        {
            if (txtMessage.Text == "Type your message here")
            {
                txtMessage.Text = "";
                txtMessage.Foreground = Brushes.White;
            }
        }

        private void txtWhatIWrite_MouseLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                if (!txtMessage.IsFocused)
                {
                    txtMessage.Text = "Type message here";
                    txtMessage.Foreground = Brushes.Gray;
                }
            }
        }

        private void txtWhatIWrite_TextChanged(object sender, EventArgs e)
        {
            if (txtMessage.Text == "Type message here")
            {
                txtMessage.Text = "";
                txtMessage.Foreground = Brushes.White;
            }
        }

        private void txtWhatIWrite_Enter(object sender, EventArgs e)
        {
            if (txtMessage.Text == "Type your message here")
            {
                txtMessage.Text = "";
                txtMessage.Foreground = Brushes.White;
            }
        }
        private Client compareTwoClients(Client client1, Client client2)
        {
            foreach (var prop in client2.GetType().GetProperties())
            {
                if (prop.GetValue(client2) == null)
                    continue;
                else
                {
                    prop.SetValue(client1, prop.GetValue(client2));
                }
            }
            return client1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //changingPersonalInfoForm cpif = new changingPersonalInfoForm(client);
            //if (cpif.ShowDialog() == DialogResult.OK)
            //{
            //    client = cpif.clientWeTurnBack;
            //    setInfoRight();
            //}
            //sendThisAction(null, client);
        }

        private void fileToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DataModel model = new DataModel();
            ofd.Filter = "Supported files (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt";
            if (ofd.ShowDialog() != true)
                return;

            byte[] file = File.ReadAllBytes(ofd.FileName);
            model.isFile = true;
            model.fileName = Path.GetFileName(ofd.FileName);
            model.fileData = Encrypt(Convert.ToBase64String(file), "1234567812345678");
            model.sender = client.username;
            model.receiver = getUserName(lstChats.SelectedItem.ToString())?.ToString() ?? "";
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            string salt = "da;%$%DST%^%$DTFGFFFDddd";
            model.Sign = ComputeMD5Hash(ComputeMD5Hash(serialized) + salt);

            sendThisAction(Newtonsoft.Json.JsonConvert.SerializeObject(model));
        }

        private void SendAudio_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "Audio Files|*.mp3";

            if (ofd.ShowDialog() != true)
                return;

            string serialized = "";

            byte[] fileBytes = File.ReadAllBytes(ofd.FileName);

            string base64OfFile = Convert.ToBase64String(fileBytes);

            string encrypted = Encrypt(base64OfFile, "1234567812345678");

            DataModel dataModel = new DataModel()
            {
                isFile = true,
                fileName = Path.GetFileName(ofd.FileName),
                fileData = encrypted,
                sender = client.username,
                receiver = getUserName(lstChats.SelectedItem.ToString())?.ToString() ?? ""
            };

            serialized = JsonConvert.SerializeObject(dataModel);

            dataModel.Sign =
                ComputeMD5Hash(
                    ComputeMD5Hash(serialized)
                    + "da;%$%DST%^%$DTFGFFFDddd");

            serialized = JsonConvert.SerializeObject(dataModel);

            sendThisAction(serialized);
        }

        private void toolStripMenuItemImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() != true)
                return;

            string serialized = "";

            byte[] fileBytes = File.ReadAllBytes(ofd.FileName);
            string base64 = Convert.ToBase64String(fileBytes);
            DataModel dataModel = new DataModel()
            {
                isFile = true,
                fileData = Encrypt(base64, "1234567812345678"),
                fileName = Path.GetFileName(ofd.FileName),
                sender = client.username,
                receiver = getUserName(lstChats.SelectedItem.ToString())?.ToString() ?? ""
            };
            string dataSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
            string salt = "da;%$%DST%^%$DTFGFFFDddd";
            string hash = ComputeMD5Hash(ComputeMD5Hash(dataSerialized) + salt);
            dataModel.Sign = hash;
            serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);

            sendThisAction(serialized);
        }
        private void lstMessages_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstMessages.SelectedItem == null)
                return;

            ChatMessage textt = (ChatMessage)lstMessages.SelectedItem;
            string text = textt.Text;

            if (text.Contains("file:///"))
            {
                string path = "";
                if (text.StartsWith("Downloaded File: "))
                {
                    path = text.Replace("Downloaded File: ", "")
                                      .Replace("file:///", "");
                }
                else if (text.StartsWith("Uploaded File: "))
                {
                    path = text.Replace("Uploaded File: ", "")
                                      .Replace("file:///", "");
                }
                else if (text.StartsWith("File received: file:///"))
                {
                    path = text.Replace("File received: ", "")
                  .Replace("file:///", "");
                }
                if (File.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                }
            }
        }
        private void LoadImage(ImageBrush pb, string path)
        {
            if (!File.Exists(path))
                return;
            if (pb.ImageSource != null)
                pb.ImageSource = null;

            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(path);
            bi.EndInit();
            bi.Freeze();

            pb.ImageSource = bi;
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosing)
                return;

            e.Cancel = true;
            isClosing = true;

            try
            {
                NotificationModel notificationModel = new NotificationModel()
                {
                    data = Encrypt("Client Leaving1209", "1234567812345678"),
                    type = "Client Leaving"
                };

                GlobalDataModel global = new GlobalDataModel()
                {
                    body = JsonConvert.SerializeObject(notificationModel),
                    isEncrypted = true,
                    type = "notification"
                };

                byte[] buffer =
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(global) + "╬");

                if (stream != null && stream.CanWrite)
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                    await stream.FlushAsync();
                }
            }
            catch
            {
            }
            finally
            {
                try { stream?.Close(); } catch { }
                try { client?.tcp?.Close(); } catch { }

                Application.Current.Shutdown();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text.Length == 0)
            {
                txtSearch.Text = "Search";
            }
        }

        private void txtSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtSearch.Text == "Search")
            {
                txtSearch.Text = "";
                txtSearch.Focus();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstChats.SelectedItem == null) return;
                if (string.IsNullOrEmpty(lstChats.SelectedItem.ToString())) return;
                if (lstChats.SelectedItem.ToString() == "Saved Messages") return;
                string historyPath = Path.Combine(MainRootDirectory, "History", client.username, getUserName(lstChats.SelectedItem.ToString()));

                if (Directory.Exists(historyPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = historyPath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("You haven't communicate with this user.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            popupAttach.IsOpen = !popupAttach.IsOpen;
        }
    }
}
