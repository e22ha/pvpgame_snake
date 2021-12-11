using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace console_snake
{
    class AlternateConsole : IDisposable
    {
        readonly NamedPipeServerStream pipeServer;
        readonly TextWriter writer;
        readonly TextReader reader;

        AlternateConsole(NamedPipeServerStream pipeServer)
        {
            this.pipeServer = pipeServer;
            writer = new StreamWriter(pipeServer, leaveOpen: true) { AutoFlush = true };
            reader = new StreamReader(pipeServer, leaveOpen: true);
        }

        public static AlternateConsole CreateServer(Action<string> startClientProcessCallback)
        {
            string pipeToken = Guid.NewGuid().ToString();
            NamedPipeServerStream pipeServer = null;
            try
            {
                pipeServer = new NamedPipeServerStream(pipeToken);
                startClientProcessCallback(pipeToken);
                pipeServer.WaitForConnection();
            }
            catch (IOException)
            {
                pipeServer?.Dispose();
                throw;
            }
            return new AlternateConsole(pipeServer);
        }

        public static void RunClient(string pipeToken)
        {
            using NamedPipeClientStream pipeClient = new NamedPipeClientStream(pipeToken);
            pipeClient.Connect();
            using var sr = new StreamReader(pipeClient, leaveOpen: true);
            using var sw = new StreamWriter(pipeClient, leaveOpen: true) { AutoFlush = true };
            while (sr.ReadLine() is string s)
            {
                var message = Unescape(s);
                if (message == "")
                    throw new ProtocolViolationException("Message should start with a prefix");
                switch (message[0])
                {
                    case 'W':
                        Console.Write(message[1..]);
                        break;
                    case 'R':
                        var input = Console.ReadLine();
                        sw.WriteLine(Escape(input));
                        break;
                    case 'T':
                        Console.Title = message[1..];
                        break;
                    default:
                        throw new ProtocolViolationException("Unknown message prefix");
                }
            }
        }

        public void Write(string s) => writer.WriteLine(Escape('W' + s));
        public void WriteLine(string s) => writer.WriteLine(Escape('W' + s + '\n'));
        public void WriteLine() => writer.WriteLine(Escape("W\n"));
        public string ReadLine()
        {
            writer.WriteLine("R");
            return Unescape(reader.ReadLine());
        }
        public void SetTitle(string title) => writer.WriteLine(Escape('T' + title));

        static string Escape(string s) => s.Replace("\\", @"\\").Replace("\n", @"\n").Replace("\r", @"\r");
        static string Unescape(string s) => s.Replace(@"\\", "\\").Replace(@"\n", "\n").Replace(@"\r", "\r");

        public void Dispose()
        {
            reader.Dispose();
            writer.Dispose();
            pipeServer.Dispose();
        }
    }
}
