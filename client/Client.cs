using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace console_snake
{
    class Client
    {
        // Port for message exchange
        int port = 8888;
        // Server IP address
        string address = "127.0.0.1";
        // TCP client
        TcpClient client = null;
        // Network stream for server connection
        NetworkStream stream = null;
        // Username
        string username = "";
        public AlternateConsole altcons;
        string guid = "";
        int score = 0;

        DateTime lastPing;
        TimeSpan difDate = new TimeSpan(0, 0, 0, 0, 3100);

        public void connect_(string address, int port, string g)
        {
            guid = g;
            username = "1";
            try
            {
                // Create TCP client and connect to server
                client = new TcpClient(address, port);
                // Get the network stream
                stream = client.GetStream();

                // Start a dedicated thread to listen for server messages
                Thread listenThread = new Thread(() => listen());
                listenThread.Start();
                altcons.WriteLine("Connection established");
            }
            catch (Exception ex)
            {
                altcons.WriteLine(ex.Message);
            }
        }

        // Listens for incoming server messages
        void listen()
        {
            try
            {
                lastPing = DateTime.Now;

                while (true)
                {
                    if ((DateTime.Now - lastPing) < difDate)
                    {
                        // Buffer for received data
                        byte[] data = new byte[1024];
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        // Read all available data from the stream
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        string message = builder.ToString();
                        if (message == "/close")
                        {
                            break;
                        }
                        else if (message == "/ping")
                        {
                            Thread pp = new Thread(() => show_ping_pong());
                            pp.Start();

                            lastPing = DateTime.Now;
                            data = Encoding.Unicode.GetBytes("/pong");
                            stream.Write(data, 0, data.Length);
                        }
                        else if (message.StartsWith("."))
                        {
                            DataForUpdate?.Invoke(message, null);
                        }
                        else if (message.StartsWith("/play_"))
                        {
                            PlaySound?.Invoke(message, null);
                            score += 10;
                        }
                        else if (message.StartsWith("/win"))
                        {
                            string s = String.Concat(message, ",", score.ToString());
                            IamWin?.Invoke(s, null);
                        }
                        else if (message.StartsWith("/lose"))
                        {
                            string s = String.Concat(message, ",", score.ToString());
                            IamLose?.Invoke(s, null);
                        }
                        else
                        {
                            altcons.WriteLine(message);
                        }
                    }
                    else
                    {
                        altcons.WriteLine("Server not responding for more than 3 seconds");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                altcons.WriteLine(ex.Message);
            }
            finally
            {
                // Close the stream and terminate the client
                stream.Close();
                client.Close();
                altcons.WriteLine("Connection closed");
            }
        }

        public event EventHandler DataForUpdate;
        public event EventHandler PlaySound;
        public event EventHandler IamWin;
        public event EventHandler IamLose;

        public void disconnect_()
        {
            altcons.WriteLine("Disconnecting...");
            send_msg("/bye");
            stream.Close();
            client.Close();
        }

        public void send_msg(string ms)
        {
            if (stream != null)
            {
                try
                {
                    stream = client.GetStream();
                    // Buffer for outgoing data
                    string message = ms;
                    altcons.WriteLine(message);
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    altcons.WriteLine(ex.Message);
                }
            }
        }

        public void generate_msg(string guid, ConsoleKeyInfo key)
        {
            string data = String.Concat(
                                        ".",
                                        key.Key.ToString(),
                                        ".",
                                        guid
                                        );

            send_msg(data);
        }

        private void show_ping_pong()
        {
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    Console.Title = "GAME " + guid.Substring(0, 4) + " SCORE: " + score + " s|1..|c";
                }
                else if (i == 1)
                {
                    Console.Title = "GAME " + guid.Substring(0, 4) + " SCORE: " + score + " s|.2.|c";
                }
                else if (i == 2)
                {
                    Console.Title = "GAME " + guid.Substring(0, 4) + " SCORE: " + score + " s|..3|c";
                }

                Thread.Sleep(1000);
            }
        }

        private void Exit()
        {
            if (stream != null)
            {
                altcons.WriteLine("Disconnecting...");
                send_msg("/bye");
                stream.Close();
                client.Close();
                altcons.WriteLine("Disconnected");
            }
        }

        private void send_(string msg)
        {
            if (stream != null)
            {
                string message = msg;
                // Prepend username to message
                message = String.Format("{0}: {1}", username, message);
                // Encode message to bytes
                byte[] data = Encoding.Unicode.GetBytes(message);
                // Send the message
                stream.Write(data, 0, data.Length);
            }
        }

        public void first(string guid)
        {
            if (stream != null)
            {
                altcons.WriteLine(guid);
                // Encode and send registration message
                byte[] data = Encoding.Unicode.GetBytes(guid);
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
