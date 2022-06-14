using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using dataclient;
using System.Windows.Forms;
namespace dataserver
{
    public delegate string StrHandler(string str);

    public class ChatSocket
    {
        public Socket socket;
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;
        public StrHandler inHandler;
        public EndPoint remoteEndPoint;
        public bool isDead = false;

        public ChatSocket(Socket s)
        {
            socket = s;
            try
            {
                stream = new NetworkStream(s);
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
            }
            catch
            {
                MessageBox.Show("123");
            }
            remoteEndPoint = socket.RemoteEndPoint;
        }

        public string receive()
        {
            return reader.ReadLine();
        }

        public ChatSocket send(string line)
        {
            writer.WriteLine(line);
            writer.Flush();
            return this;
        }

        public static ChatSocket connect(string ip, int port)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipep);
            }
            catch
            {
                return null;
            }
            
            return new ChatSocket(socket);
        }

        public Thread newListener(StrHandler pHandler)
        {
            inHandler = pHandler;

            Thread listenThread = new Thread(new ThreadStart(listen));
            listenThread.Start();
            return listenThread;
        }

        public void listen()
        {
            try
            {
                while (true)
                {
                    string line = receive();
                    inHandler(line);
                }
            }
            catch (Exception ex)
            {
                isDead = true;
                myLog.Write("已經與伺服器斷線");
            }
        }
    }
}
