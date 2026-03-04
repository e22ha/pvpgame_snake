using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server
{
    class Server
    {
        public event EventHandler UpdateDirection;
        public event EventHandler AllPlayerReady;
        public event EventHandler AddNewPlayer;
        TcpListener listener;
        TimeSpan difDate = new TimeSpan(0, 0, 0, 0, 3100);
        public LobbyList Rooms = new LobbyList();
        public LobbyRoom Room = new LobbyRoom(Guid.NewGuid().ToString(), "first");

        // Initialize the server
        public Server(string ip, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            Console.WriteLine(ipEndPoint.ToString());
            listener = new TcpListener(ipEndPoint);
        }

        public struct Player
        {
            public DateTime lastPong;
            public TcpClient client;
            public NetworkStream stream;
            public bool availabel;
            public void set(bool st)
            {
                availabel = st;
            }
            public string Guid;
        }

        // Waits for and accepts incoming connection requests
        void listen()
        {
            // Client connection loop
            try
            {
                while (true)
                {
                    // Accept incoming connection request
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("New client connected");
                    // Create a new thread to handle the new client
                    Thread clientThread = new Thread(() => Process(client));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("listen: " + ex.Message);
            }
        }

        bool checkMsg(string m)
        {
            if (m.StartsWith('#') | m.Contains('/')) return true;
            else return false;
        }

        // Process messages from a connected client
        private void Process(TcpClient tcpClient)
        {
            TcpClient client = tcpClient;
            NetworkStream stream = null;
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            string message = "";
            string guid = "";
            string name = "";
            byte[] data = new byte[1024]; // Buffer for received data
            ClientInfo player = new();

            try
            {
                // Get the stream for message exchange
                stream = client.GetStream();

                while (true)
                {
                    do
                    {
                        // Read bytes from the stream into data starting at offset 0
                        bytes = stream.Read(data, 0, data.Length);
                        // Build a string from the received bytes
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);
                    message = builder.ToString();
                    player = new ClientInfo(DateTime.Now, client, stream, "", "");
                    if (checkMsg(message))
                    {
                        string[] d = message.Split('/');

                        guid = d[0].Trim('#');
                        if (d.Length > 1)
                        {
                            name = d[1];
                        }
                        else
                        {
                            name = guid.Substring(4);
                        }

                        player = new ClientInfo(DateTime.Now, client, stream, guid, name);
                        Room.AddNewClient(player);
                        AddNewPlayer?.Invoke(player.guid, null);
                        break;
                    }
                    else
                    {
                        data = Encoding.Unicode.GetBytes("Send your guid(and name)");
                        player.stream.Write(data, 0, data.Length);
                    }
                }

                // Send initial ping to the client
                data = Encoding.Unicode.GetBytes("/ping");
                stream.Write(data, 0, data.Length);

                // Main message receive/send loop
                while (true)
                {
                    builder = new StringBuilder();
                    data = new byte[1024];
                    if (player.availabel == true)
                    {
                        if (DateTime.Now - player.lastPong > difDate)
                        {
                            Room.RemoveClient(player.guid);
                            Console.WriteLine("Client not responding");
                            break;
                        }
                        // Keep reading while data is available in the stream
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        message = builder.ToString();
                        if (message == "/bye")
                        {
                            Console.WriteLine(message);
                            player.availabel = false;
                            Room.RemoveClient(player.guid);
                            break;
                        }
                        else if (message.StartsWith("#"))
                        {
                            Console.WriteLine(message);
                        }
                        else if (message == "/pong")
                        {
                            player.UpdateLastPong(DateTime.Now);
                            // Use thread pool to avoid creating a new thread per pong
                            Task.Run(() => ping_pong(player));
                        }
                        else if (message.StartsWith("."))
                        {
                            UpdateDirection?.Invoke(message, null);
                        }
                        else if (message.StartsWith("/clientlist"))
                        {
                        }
                        else if (message.StartsWith("/ready"))
                        {
                            player.gameready = true;
                            int i = 0;
                            foreach (ClientInfo c in Room.getClients())
                            {
                                if (c.gameready) i++;
                            }
                            if (Room.getClients().Count == i) AllPlayerReady?.Invoke(null, null);
                        }
                        else
                        {
                            // Broadcast unknown message to all other clients
                            data = Encoding.Unicode.GetBytes(message);
                            foreach (ClientInfo c in Room.getClients())
                            {
                                if (c.client != player.client)
                                {
                                    if (c.availabel == true) c.stream.Write(data, 0, data.Length);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("process: " + ex.Message);
            }
            finally
            {
                // Release resources when session ends
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                Console.WriteLine("User disconnected");
                if (Room.getClients().Count == 0)
                {
                    stop(Room);
                }
            }
        }

        public void start()
        {
            // Start listening for incoming connections
            listener.Start();
            // Start a dedicated thread to accept incoming connections
            Thread listenThread = new Thread(() => listen());
            listenThread.Start();
            Console.WriteLine("Server started");
        }

        public void stop(LobbyRoom room)
        {
            send_msg_all("/close");
            listener.Stop();
            // Close all connections and clear the client list
            try
            {
                foreach (ClientInfo c in room.getClients())
                {
                    c.availabel = false;
                    c.stream.Close();
                    c.client.Close();
                }
                room.getClients().Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("stopserver: " + ex.Message);
            }
            Console.WriteLine("Server stopped");
        }

        void send_msg_all(string ms)
        {
            foreach (ClientInfo c in Room.getClients())
            {
                sendToOne(ms, c);
            }
        }

        internal void EndGame(object sender, EventArgs e)
        {
            foreach (ClientInfo client in Room.getClients())
            {
                if (client.guid == (string)sender) sendToOne("/lose", client);
                else sendToOne("/win", client);
            }
        }

        private static void sendToOne(string ms, ClientInfo c)
        {
            if (c.availabel == true)
            {
                NetworkStream ns = c.stream;
                try
                {
                    string message = ms;
                    Console.WriteLine(message);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    ns.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("send_msg: " + ex.Message);
                }
            }
        }

        public void sendSound_eat(object sender, EventArgs e)
        {
            foreach (ClientInfo clientInfo in Room.getClients())
            {
                if (clientInfo.guid == (string)sender)
                {
                    sendToOne("/play_eat", clientInfo);
                    break;
                }
            }
        }

        public void sendField(object sender, EventArgs e)
        {
            string data = "";

            foreach (var item in (List<object>)sender)
            {
                data = String.Concat(data, ".", item);
            }

            send_msg_all(data);
        }

        private void ping_pong(ClientInfo c)
        {
            Thread.Sleep(3000);
            if (c.availabel == true)
            {
                try
                {
                    byte[] data = Encoding.Unicode.GetBytes("/ping");
                    c.stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
