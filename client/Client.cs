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
        //номер порта для обмена сообщениями
        int port = 8888;
        //ip адрес сервера
        string address = "127.0.0.1";
        //объявление TCP клиента
        TcpClient client = null;
        //объявление канала соединения с сервером
        NetworkStream stream = null;
        //имя пользователя
        string username = "";
        public AlternateConsole altcons;

        DateTime lastPing;
        TimeSpan difDate = new TimeSpan(0, 0, 0, 0, 3100);

        public void connect_()
        {
            //получение имени пользователя
            username = "1";
            try //если возникнет ошибка - переход в catch
            {
                //создание клиента
                client = new TcpClient(address, port);
                //получение канала для обмена сообщениями
                stream = client.GetStream();

                //создание нового потока для ожидания сообщения от сервера
                Thread listenThread = new Thread(() => listen());
                listenThread.Start();
                altcons.WriteLine("Соединение установлено");
            }
            catch (Exception ex)
            {
                altcons.WriteLine(ex.Message);
            }
        }
        //функция ожидания сообщений от сервера
        void listen()
        {
            try //в случае возникновения ошибки - переход к catch
            {
                lastPing = DateTime.Now;
                
                //цикл ожидания сообщениями
                while (true)
                {
                    if ((DateTime.Now - lastPing) < difDate)
                    {
                        //буфер для получаемых данных
                        byte[] data = new byte[64];
                        //объект для построения смтрок
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        //до тех пор, пока есть данные в потоке
                        do
                        {
                            //получение 64 байт
                            bytes = stream.Read(data, 0, data.Length);
                            //формирование строки
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        //получить строку
                        string message = builder.ToString();
                        //вывод сообщения в лог клиента
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
                        else if(message.StartsWith("."))
                        {
                            DataForUpdate?.Invoke(message,null);

                        }
                        else
                        {
                            altcons.WriteLine(message);
                        }
                    }
                    else
                    {
                        altcons.WriteLine("Сервер не отвечает более 3 секунд");
                    }
                }
            }
            catch (Exception ex)
            {
                //вывести сообщение об ошибке
                altcons.WriteLine(ex.Message);

            }
            finally
            {
                //закрыть канал связи и завершить работу клиента
                stream.Close();
                client.Close();
                altcons.WriteLine("Соединение разорвано");
            }
        }

        public event EventHandler DataForUpdate;

        public void disconnect_()
        {
            altcons.WriteLine("Отключение...");
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
                    byte[] data = new byte[64];// буфер для получаемых данных
                    string message = ms;
                    altcons.WriteLine(message);
                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex) //если возникла ошибка, вывести сообщение об ошибке
                {
                    altcons.WriteLine(ex.Message);
                }
            }
        }

        public void generate_msg(string guid, ConsoleKeyInfo key) {
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
                      altcons.WriteLine( "s|1..|c");
                }
                else if (i == 1)
                {
                    altcons.WriteLine("s|.2.|c");
                }
                else if (i == 2)
                {
                    altcons.WriteLine("s|..3|c");
                }

                Thread.Sleep(1000);
            }
        }

        private void Exit()
        {
            if (stream != null)
            {
                altcons.WriteLine("Отключение...");
                send_msg("/bye");
                stream.Close();
                client.Close();
                altcons.WriteLine("Отключено");
            }

        }

        private void send_(string msg)
        {
            if (stream != null)
            {
                //получение сообщения
                string message = msg;
                //добавление имени пользователя к сообщению
                message = String.Format("{0}: {1}", username, message);
                //преобразование сообщение в массив байтов
                byte[] data = Encoding.Unicode.GetBytes(message);
                //отправка сообщения
                stream.Write(data, 0, data.Length);
            }
        }

        public void first(string guid)
        {
            if (stream != null)
            {
                //преобразование сообщение в массив байтов
                byte[] data = Encoding.Unicode.GetBytes(guid);
                //отправка сообщения
                stream.Write(data, 0, data.Length);
            }
        }

    }
}
