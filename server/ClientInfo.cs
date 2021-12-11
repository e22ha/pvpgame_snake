using System;
using System.Net.Sockets;

namespace server
{
    public class ClientInfo
    {
        public DateTime lastPong { get; private set; }
        public TcpClient client { get; private set; }
        public NetworkStream stream { get; private set; }
        public bool availabel { get; set; }
        public string guid { get; private set; }
        public string name { get; private set; }
        public LobbyRoom lroom { get; set; }
        public ClientInfo()
        {

        }

        public ClientInfo(DateTime lasrPong, TcpClient client, NetworkStream stream, string guid, string name)
        {
            this.lastPong = lasrPong;
            this.client = client;
            this.stream = stream;
            this.guid = guid;
            if (name == null) this.name = guid.Substring(4);
            else this.name = name;
            availabel = true;
        }

        public void UpdateLastPong(DateTime dt)
        {
            this.lastPong = dt;
        }

        

    }
}
