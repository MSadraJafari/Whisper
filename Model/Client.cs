using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace Model
{
    public class Client
    {
        public Client(string username, TcpClient tcpClient)
        {
            this.username = username;
            loginDate = DateTime.Now;
            did = Guid.NewGuid();
            id = this.did.ToString();
            this.tcp = tcpClient;
        }
        public bool listen { get; set; } = true;
        public string id { get; set; }
        [JsonIgnore]
        public TcpClient tcp { get; set; }
        public string username { get; set; }
        public DateTime loginDate { get; set; }
        public List<string> knownOnlineUsers { get; set; }
        public string profilePicture { get; set; }
        public string bio { get; set; }
        public string phoneNumber { get; set; }
        public string tagName { get; set; }
        public string birthDay { get; set; }
        [JsonIgnore]
        public Guid did { get; set; }
    }
}
