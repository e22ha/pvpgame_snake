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
        public List<Player> Players = new List<Player>();

        public Server() { }
        public Server(string ip, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ip), port);
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
                Console.WriteLine(ex.Message);
            }
        }



        //обработка сообщений от клиента
        private void Process(TcpClient tcpClient)
        {
            TcpClient client = tcpClient;
            NetworkStream stream = null; //получение канала связи с клиентом

            try //означает что в случае возникновении ошибки, управление перейдёт к блоку catch
            {
                //получение потока для обмена сообщениями
                stream = client.GetStream(); //получение канала связи с клиентом

                byte[] data = new byte[64];// буфер для получаемых данных

                Player u = new Player();
                u.client = client;
                u.stream = stream;
                u.lastPong = DateTime.Now;
                u.availabel = true;

                Players.Add(u);

                data = Encoding.Unicode.GetBytes("/ping"); //отправка первого сообщения пинг
                u.stream.Write(data, 0, data.Length);

                //цикл ожидания и отправки сообщений
                while (true)
                {
                    if (u.availabel == true)
                    {
                        //объект, для формирования строк
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;

                        if (DateTime.Now - u.lastPong > difDate)
                        {
                            Players.Remove(u);
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
                        //преобразование сообщения
                        string message = builder.ToString();
                        if (message.StartsWith("#"))
                        {
                            message = message.Trim('#');
                            u.Guid = message;
                            AddNewPlayer?.Invoke(message, null);
                        }
                        else if (message == "/bye")
                        {
                            Console.WriteLine(message);
                            u.availabel = false;
                            Players.Remove(u);

                            break;
                        }
                        else if (message == "/pong")
                        {
                            u.lastPong = DateTime.Now;
                            Thread pingThread = new Thread(() => ping_pong(u));
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
                            foreach (Player us in Players)
                            {
                                if (u.client != us.client)
                                {
                                    //отправка сообщения обратно клиенту
                                    if (us.availabel == true) us.stream.Write(data, 0, data.Length);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
            {
                Console.WriteLine(ex.Message);
            }
            finally //после выхода из бесконечного цикла
            {
                //освобождение ресурсов при завершении сеанса
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                Console.WriteLine("Пользователь отлючён");
                if (Players.Count == 0)
                {
                    stop();
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

        public void stop()
        {
            send_msg("/close");
            listener.Stop();
            //тогда закрываем подключения и очищаем список
            try
            {
                foreach (Player ur in Players)
                {
                    ur.set(false);
                    ur.stream.Close();
                    ur.client.Close();
                }
                Players.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("stopserver: " + ex.Message);
            }
            Console.WriteLine("Сервер остановлен");
        }

        void send_msg(string ms)
        {
            foreach (Player u in Players)
            {
                if (u.availabel == true)
                {
                    NetworkStream ns = u.stream;
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
                        Console.WriteLine(ex.Message);
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
        private void ping_pong(Player u)
        {
            Thread.Sleep(3000);
            if (u.availabel == true)
            {
                try
                {
                    byte[] data = new byte[64];// буфер для получаемых данных

                    data = Encoding.Unicode.GetBytes("/ping");
                    u.stream.Write(data, 0, data.Length);
                }
                catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
