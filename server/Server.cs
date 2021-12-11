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
        public event EventHandler AddNewPlayer;
        TcpListener listener;
        TimeSpan difDate = new TimeSpan(0, 0, 0, 0, 3100);
        //public List<Player> Players = new List<Player>();
        public LobbyList Rooms = new LobbyList();
        public LobbyRoom Room = new LobbyRoom(Guid.NewGuid().ToString(), "first");

        //init server
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

        //функция ожидания и приёма запросов на подключение
        void listen()
        {
            //цикл подключения клиентов
            try
            {
                while (true)
                {
                    //принятие запроса на подключение
                    TcpClient client = listener.AcceptTcpClient();
                    Console.WriteLine("Новый клиент подключен");
                    //создание нового потока для обслуживания нового клиента
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

        //обработка сообщений от клиента
        private void Process(TcpClient tcpClient)
        {
            TcpClient client = tcpClient;
            NetworkStream stream = null; //получение канала связи с клиентом
                                         //объект, для формирования строк
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            string message = "";
            string guid = "";
            string name = "";
            byte[] data = new byte[64];// буфер для получаемых данных
            ClientInfo player = new();

            try //означает что в случае возникновении ошибки, управление перейдёт к блоку catch
            {
                //получение потока для обмена сообщениями
                stream = client.GetStream(); //получение канала связи с клиентом

                while (true)
                {
                    do
                    {
                        //из потока считываются 64 байта и записываются в data начиная с 0
                        bytes = stream.Read(data, 0, data.Length);
                        //из считанных данных формируется строка
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

                data = Encoding.Unicode.GetBytes("/ping"); //отправка первого сообщения пинг
                stream.Write(data, 0, data.Length);


                //цикл ожидания и отправки сообщений
                while (true)
                {
                    builder = new StringBuilder();
                    data = new byte[64];
                    if (player.availabel == true)
                    {
                        if (DateTime.Now - player.lastPong > difDate)
                        {
                            Room.RemoveClient(player.guid);
                            Console.WriteLine("Клиент не отвечает");
                            break;
                        }
                        //до тех пор, пока в потоке есть данные
                        do
                        {
                            //из потока считываются 64 байта и записываются в data начиная с 0
                            bytes = stream.Read(data, 0, data.Length);
                            //из считанных данных формируется строка
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        message = builder.ToString(); //преобразование сообщения
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
                            Console.WriteLine(message);
                            player.UpdateLastPong(DateTime.Now);
                            Thread pingThread = new Thread(() => ping_pong(player));
                            pingThread.Start();
                        }
                        else if (message.StartsWith("."))
                        {
                            UpdateDirection?.Invoke(message, null);
                        }
                        else
                        {
                            Console.WriteLine(message);
                            data = Encoding.Unicode.GetBytes(message);
                            foreach (ClientInfo c in Room.getClients())
                            {
                                if (c.client != c.client)
                                {
                                    if (c.availabel == true) c.stream.Write(data, 0, data.Length);
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
            {
                Console.WriteLine("process: " + ex.Message);
            }
            finally //после выхода из бесконечного цикла
            {
                //освобождение ресурсов при завершении сеанса
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                Console.WriteLine("Пользователь отлючён");
                if (Room.getClients().Count == 0)
                {
                    stop(Room);
                }
            }
        }

        public void start()
        {
            //начало прослушивания
            listener.Start();
            //создание нового потока для ожидания и подключения клиентов
            Thread listenThread = new Thread(() => listen());
            listenThread.Start();
            Console.WriteLine("Сервер запущен");
        }

        public void stop(LobbyRoom room)
        {
            send_msg("/close");
            listener.Stop();
            //тогда закрываем подключения и очищаем список
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
            Console.WriteLine("Сервер остановлен");
        }

        void send_msg(string ms)
        {
            foreach (ClientInfo c in Room.getClients())
            {
                if (c.availabel == true)
                {
                    NetworkStream ns = c.stream;
                    try
                    {
                        byte[] data = new byte[64];// буфер для получаемых данных
                        string message = ms;
                        Console.WriteLine(message);
                        data = Encoding.Unicode.GetBytes(message);
                        ns.Write(data, 0, data.Length);
                    }
                    catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
                    {
                        Console.WriteLine("send_msg: " + ex.Message);
                    }
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

            send_msg(data);
        }
        private void ping_pong(ClientInfo c)
        {
            Thread.Sleep(3000);
            if (c.availabel == true)
            {
                try
                {
                    byte[] data = new byte[64];// буфер для получаемых данных

                    data = Encoding.Unicode.GetBytes("/ping");
                    c.stream.Write(data, 0, data.Length);
                }
                catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
