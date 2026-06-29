using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
namespace Server
{
    public partial class server : Window
    {
        private CancellationTokenSource ctsListener;
        volatile bool isListening;
        List<Client> clients = new List<Client>();
        List<Client> clientsLeft = new List<Client>();
        List<Client> clientsOnline = new List<Client>();
        TcpListener tcpListener;
        NetworkStream stream;
        bool waitingForAnsewr = false;
        List<Client> ClientsWaitingForAnsewrs = new List<Client>();
        int startPressed = 0;
        List<DataModel> MessagesWaitingForCorrection = new List<DataModel>();
        public server()
        {
            InitializeComponent();
            btnStop.IsEnabled = false;
        }
        private void HandleNewClient(TcpClient tcpClient)
        {
            NetworkStream streamm = stream;
            try
            {
                streamm = tcpClient.GetStream();
            }
            catch (Exception ex)
            {
                return;
            }
            byte[] buffer = Encoding.UTF8.GetBytes("Welcome");
            streamm.Write(buffer, 0, buffer.Length);
            buffer = new byte[2000000];
            int size = streamm.Read(buffer, 0, buffer.Length);
            string userSerialized = Encoding.UTF8.GetString(buffer, 0, size);
            userSerialized = Decrypt(userSerialized, "1234567812345678");
            Client clien1 = JsonConvert.DeserializeObject<Client>(userSerialized);
            clien1.tcp = tcpClient;
            bool isRepeated = false;
            bool isCameBack = false;
            foreach (Client c in clientsLeft)
            {
                if (c.username == Decrypt(clien1.username, "1234567812345678"))
                {
                    isRepeated = false;
                    isCameBack = true;
                }
            }
            if (!isCameBack)
            {
                foreach (Client cli in clientsOnline)
                {
                    if (cli.username == Decrypt(clien1.username, "1234567812345678"))
                        isRepeated = true;
                }
            }
            if (isRepeated)
            {
                string message = "isRepeated";
                buffer = Encoding.UTF8.GetBytes(Encrypt(message, "1234567122345108"));
                clien1.tcp.GetStream().Write(buffer, 0, buffer.Length);
                return;
            }
            else
            {
                string message = "is not repeated";
                buffer = Encoding.UTF8.GetBytes(Encrypt((message), "1234567122345108"));
                clien1.tcp.GetStream().Write(buffer, 0, buffer.Length);
            }
            Dispatcher.Invoke(new Action(() =>
            {
                txtShowTexts.AppendText("User ");
                txtShowTexts.AppendText(Decrypt(clien1.username, "1234567812345678"));
                txtShowTexts.AppendText(" is logging in...\n");
                buffer = new byte[2000000];
            }));
            size = streamm.Read(buffer, 0, buffer.Length);
            string seriali = Encoding.UTF8.GetString(buffer, 0, size);
            Client client1 = Newtonsoft.Json.JsonConvert.DeserializeObject<Client>(seriali);
            client1.username = Decrypt(client1.username, "1234567812345678");
            client1.tcp = tcpClient;
            bool canContinue = true;
            if (isCameBack)
            {
                int index = 0;
                foreach (Client client in clientsLeft)
                {
                    if (client.username == client1.username)
                    {
                        break;
                    }
                    index++;
                }
                if (clients[index].tagName == client1.tagName) { }
                else
                {
                    canContinue = false;
                }
            }
            string ser = Newtonsoft.Json.JsonConvert.SerializeObject(MakeClientsReadyForUser());
            NotificationModel notification = new NotificationModel()
            {
                type = "users",
                data = ser
            };
            GlobalDataModel globalDataModel = new GlobalDataModel()
            {
                type = "notification",
                body = Newtonsoft.Json.JsonConvert.SerializeObject(notification),
                isEncrypted = false
            };
            buffer = new byte[8192];
            ser = Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel);
            if (canContinue)
            {
                buffer = Encoding.UTF8.GetBytes(ser);
                client1.tcp.GetStream().Write(buffer, 0, buffer.Length);
            }
            else
            {
                buffer = Encoding.UTF8.GetBytes(Encrypt("@@!!ddCan't continue3232", "1234567812345678") + "╧");
                client1.tcp.GetStream().Write(buffer, 0, buffer.Length);
                Dispatcher.Invoke(new Action(() =>
                {
                    txtShowTexts.AppendText("User ");
                    txtShowTexts.AppendText(Decrypt(clien1.username, "1234567812345678"));
                    txtShowTexts.AppendText(" Could not log in.\n");
                }));
                return;
            }
            if (isCameBack)
            {
                int index = 0;
                foreach (Client client in clientsLeft)
                {
                    if (client.username == client1.username)
                    {
                        break;
                    }
                    index++;
                }
                clientsLeft.RemoveAt(index);
            }
            else
                clients.Add(client1);
            Dispatcher.Invoke(new Action(() =>
            {
                txtShowTexts.AppendText("User ");
                txtShowTexts.AppendText(Decrypt(clien1.username, "1234567812345678"));
                txtShowTexts.AppendText(" has just logged in.\n");
            }));
            clientsOnline.Add(client1);
            alertAllNewUser();
            Thread thread = new Thread(() => MessageHandlerNewClient(client1));
            thread.Start();
        }
        private List<ClientSendingForUser> MakeClientsReadyForUser()
        {
            List<ClientSendingForUser> clientsSFU = new List<ClientSendingForUser>();
            foreach (Client cc in clients)
            {
                ClientSendingForUser clientS = new ClientSendingForUser
                {
                    bio = cc.bio,
                    phoneNumber = cc.phoneNumber,
                    profPicture = cc.profilePicture,
                    tagName = cc.tagName,
                    username = cc.username
                };
                clientsSFU.Add(clientS);
            }
            return clientsSFU;
        }
        private void MessageHandlerNewClient(Client client)
        {
            byte[] buffer = new byte[8192];
            string cach = "";
            while (isListening)
            {
                //try
                //{
                buffer = new byte[8192];
                int size = client.tcp.GetStream().Read(buffer, 0, buffer.Length);
                if (size == 0)
                {
                    continue;
                }
                cach += Encoding.UTF8.GetString(buffer, 0, size);
                while (true)
                {
                    int idx = cach.IndexOf("╬");
                    if (idx == -1)
                    {
                        break;
                    }
                    string gotRaw = cach.Substring(0, idx);
                    cach = cach.Substring(idx + 1);
                    if (string.IsNullOrWhiteSpace(gotRaw))
                    {
                        break;
                    }
                    DataModel dataModel = new DataModel();
                    GlobalDataModel got = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalDataModel>(gotRaw);
                    if (got.type == "message")
                    {
                        if (got != null)
                        {
                            string salt = "da;%$%DST%^%$DTFGFFFDddd";
                            dataModel = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                            DataModel Clone = Newtonsoft.Json.JsonConvert.DeserializeObject<DataModel>(got.body);
                            dataModel.Sign = null;
                            var hash = ComputeMD5Hash(Newtonsoft.Json.JsonConvert.SerializeObject(dataModel));
                            hash = ComputeMD5Hash(hash + salt);
                            if (Clone.Sign == hash)
                            {
                                client.bio = Clone.Sign;
                                MessagesWaitingForCorrection.Add(Clone);
                                if (sendItToThisName(Clone))
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
                                        type = "MessageCorrectionBack",
                                        isEncrypted = true,
                                        body = JsonConvert.SerializeObject(correctionData)
                                    };
                                    buffer = new byte[4096];
                                    buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                    client.tcp.GetStream().Write(buffer, 0, buffer.Length);
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
                                    type = "MessageCorrectionBack",
                                    isEncrypted = true,
                                    body = JsonConvert.SerializeObject(correctionData)
                                };
                                buffer = new byte[4096];
                                buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                client.tcp.GetStream().Write(buffer, 0, buffer.Length);
                            }
                        }

                    }
                    else if (got.type == "file")
                    {
                        DataModel clone = JsonConvert.DeserializeObject<DataModel>(got.body);

                        DataModel tempForHash = JsonConvert.DeserializeObject<DataModel>(got.body);
                        tempForHash.Sign = null;
                        string serialized = JsonConvert.SerializeObject(tempForHash);
                        string salt = "da;%$%DST%^%$DTFGFFFDddd";
                        string hash = ComputeMD5Hash(ComputeMD5Hash(serialized) + salt);
                        if (clone.Sign == hash)
                        {
                            client.bio = clone.Sign;
                            MessagesWaitingForCorrection.Add(clone);
                            if (sendItToThisName(clone))
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
                                    type = "MessageCorrectionBack",
                                    isEncrypted = true,
                                    body = JsonConvert.SerializeObject(correctionData)
                                };
                                buffer = new byte[4096];
                                buffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel) + "╬");
                                client.tcp.GetStream().Write(buffer, 0, buffer.Length);
                            }


                            CorrectionData correction = new CorrectionData()
                            {
                                sender = client.username,
                                receiver = clone.sender,
                                message = Encrypt("CorrectGotIt9247813", "1234567812345678"),
                                Sign = clone.Sign,
                                time = DateTime.Now
                            };

                            GlobalDataModel correctionGlobal = new GlobalDataModel()
                            {
                                type = "MessageCorrectionBack",
                                isEncrypted = true,
                                body = JsonConvert.SerializeObject(correction)
                            };

                            byte[] respBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(correctionGlobal) + "╬");
                            client.tcp.GetStream().Write(respBuffer, 0, respBuffer.Length);
                        }
                        else
                        {
                            // Invalid signature
                            CorrectionData correctionData = new CorrectionData()
                            {
                                sender = client.username,
                                receiver = clone.sender,
                                message = Encrypt("NotSent87#1645fasfsd4", "1234567812345678"),
                                Sign = clone.Sign
                            };

                            GlobalDataModel globalDataModel = new GlobalDataModel()
                            {
                                type = "MessageCorrectionBack",
                                isEncrypted = true,
                                body = JsonConvert.SerializeObject(correctionData)
                            };
                            buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(globalDataModel) + "╬");
                            client.tcp.GetStream().Write(buffer, 0, buffer.Length);
                        }
                    }
                    else if (got.type == "messageCorrection")
                    {
                        CorrectionData correctionData = JsonConvert.DeserializeObject<CorrectionData>(got.body);
                        Client DestinationClient;
                        bool isThere = false;
                        foreach (DataModel ddm in MessagesWaitingForCorrection)
                        {
                            if (ddm.Sign == correctionData.Sign)
                            {
                                isThere = true;
                                break;
                            }
                        }
                        if (!isThere)
                        {
                            MessageBox.Show($"A Fake request detected\n{correctionData.Sign}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                        }
                        if (Decrypt(correctionData.message, "1234567812345678") == "CorrectGotIt9247813") { }
                        else
                        {
                            foreach (Client cc in ClientsWaitingForAnsewrs)
                            {
                                if (cc.bio == correctionData.Sign)
                                {
                                    got.type = "MessageCorrectionBack";
                                    gotRaw = JsonConvert.SerializeObject(got);
                                    byte[] bytes = Encoding.UTF8.GetBytes(gotRaw + "╬");
                                    cc.tcp.GetStream().Write(bytes, 0, bytes.Length);
                                }
                            }
                        }
                    }
                    else if (got.type == "UpdateClientInfo")
                    {
                        if (!got.isEncrypted)
                        {
                            Client ccnew = Newtonsoft.Json.JsonConvert.DeserializeObject<Client>(got.body);
                            int index = 0;
                            foreach (Client ccc in clients)
                            {
                                if (ccc.username == ccnew.username)
                                {
                                    break;
                                }
                                index++;
                            }
                            ccnew.tcp = clients[index].tcp;
                            clients[index] = ccnew;
                            alertAllNewUser();
                        }
                    }
                    else if (got.type == "notification")
                    {
                        NotificationModel notification = JsonConvert.DeserializeObject<NotificationModel>(got.body);
                        string message = Decrypt(notification.data, "1234567812345678");
                        if (notification.type == "Client Leaving")
                        {
                            if (message == "Client Leaving1209")
                            {
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    txtShowTexts.AppendText($"Client {client.username} left the connection");
                                }));
                                clientsLeft.Add(client);
                                clientsOnline.Remove(client);
                                return;
                            }
                        }
                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //}
                }
            }
        }
        private void alertAllNewUser()
        {
            NotificationModel notificationModel = new NotificationModel()
            {
                type = "loggedIns",
                data = Newtonsoft.Json.JsonConvert.SerializeObject(MakeClientsReadyForUser())
            };
            GlobalDataModel globalDataModel = new GlobalDataModel()
            {
                isEncrypted = false,
                type = "notification",
                body = Newtonsoft.Json.JsonConvert.SerializeObject(notificationModel)
            };
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel);
            byte[] buffer = Encoding.UTF8.GetBytes(serialized + '╬');
            foreach (Client clientt in clientsOnline)
            {
                clientt.tcp.GetStream().Write(buffer, 0, buffer.Length);
            }

        }
        private bool sendItToThisName(DataModel dataModel)
        {
            try
            {
                int index = 0;

                foreach (var client in clients)
                {
                    if (client.username == dataModel.receiver)
                    {
                        string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dataModel);
                        GlobalDataModel globalDataModel = new GlobalDataModel();
                        if (dataModel.isFile)
                        {
                            globalDataModel = new GlobalDataModel()
                            {
                                body = serialized,
                                type = "file",
                                isEncrypted = true
                            };
                        }
                        else
                        {
                            globalDataModel = new GlobalDataModel()
                            {
                                body = serialized,
                                type = "message",
                                isEncrypted = true
                            };
                        }
                        serialized = Newtonsoft.Json.JsonConvert.SerializeObject(globalDataModel);
                        byte[] buffer = Encoding.UTF8.GetBytes(serialized + '╬');
                        client.tcp.GetStream().Write(buffer, 0, buffer.Length);
                        return true;
                    }
                }
                MessageBox.Show($"Username {dataModel.receiver} could not be found or it is not online", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;

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

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            ctsListener?.Cancel();
            btnStart.IsEnabled = false;
            isListening = false;
            byte[] stop = Encoding.UTF8.GetBytes("Stopping Server!!!56");
            tcpListener?.Stop();
            foreach (var client in clients)
            {
                client.tcp.GetStream().Write(stop, 0, stop.Length);
                Thread.Sleep(1000);
            }
            Dispatcher.Invoke(new Action(() =>
            {
                txtShowTexts.AppendText("Server Stopped\n");
            }));
            Dispatcher.Invoke(new Action(() => { btnStart.IsEnabled = true; }));
            Dispatcher.Invoke(new Action(() => { btnStop.IsEnabled = false; }));
            StatusText.Text = "Stopped";
            StatusDot.Fill = Brushes.Red;

            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Running";
                StatusDot.Fill = Brushes.LimeGreen;
                startPressed++;
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = true;
                isListening = false;
                btnStart.IsEnabled = false;
                txtShowTexts.AppendText("Server started!\n");
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                tcpListener = new TcpListener(ip, 9999);
                tcpListener.Start();
                isListening = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Task.Run(() =>
            {
                while (isListening)
                {
                    try
                    {
                        if (startPressed > 1)
                        {
                            tcpListener.Start();
                            isListening = true;
                            byte[] stop = Encoding.UTF8.GetBytes("Server Started!@#100003");
                            foreach (var client in clients)
                            {
                                client.tcp.GetStream().Write(stop, 0, stop.Length);
                                Thread.Sleep(1000);
                            }
                        }
                        else
                        {
                            tcpListener.Start();
                            isListening = true;
                        }
                        ctsListener = new CancellationTokenSource();
                        TcpClient tcpClient = new TcpClient();
                        while (!ctsListener.Token.IsCancellationRequested)
                        {
                            try
                            {
                                tcpClient = tcpListener.AcceptTcpClient();
                            }
                            catch (Exception ex)
                            {
                                if (ex.GetType() == typeof(SocketException) && ex.Message == "A blocking operation was interrupted by a call to WSACancelBlockingCall.")
                                {
                                    continue;
                                }
                                else
                                    MessageBox.Show($"{ex.Message}");
                            }
                            Task.Run(() =>
                            {
                                HandleNewClient(tcpClient);
                            });
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "A blocking operation was interrupted by a call to WSACancelBlockingCall")
                        {
                            continue;
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

            });
        }
    }
}
