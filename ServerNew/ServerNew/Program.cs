using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;

namespace Server
{
    internal class Program
    {
        private static readonly byte[] bs = new byte[1024];
        private static int count;

        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 8302));//绑定端口

            Console.WriteLine("******服务器已启动，等待客户连接********");
            socket.Listen(1000);//启动监听，设置最大的队列长度
            //开始接受客户端连接请求
            socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
            Console.ReadLine();


        }

        private static void CheckListen(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void ClientAccepted(IAsyncResult ar)
        {
            count++;//计数器
            var socket = ar.AsyncState as Socket;
            //客户端Socket实例，后续保存
            if (socket != null)
            {
                var client = socket.EndAccept(ar);
                //客户端IP地址和端口信息
                IPEndPoint colientip = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine(colientip + "一个客户已连接" + count, ConsoleColor.Yellow);
                //接受客户端消息
                client.BeginReceive(bs, 0, bs.Length, SocketFlags.None, new AsyncCallback(ReceiveMsg), client);

            }
            //准备接受下一个客户端连接请求
            if (socket != null) socket.BeginAccept(new AsyncCallback(ClientAccepted), socket);
        }
        //接受客户端的消息
        public static void ReceiveMsg(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;
            //客户端IP地址和端口消息
            if (socket != null)
            {
                IPEndPoint clientipe = (IPEndPoint)socket.RemoteEndPoint;
                try
                {
                    var length = socket.EndReceive(ar);
                    var message = Encoding.UTF8.GetString(bs, 0, length);
                    var connect = Deserialize<ConnectServer>(message);
                    Console.WriteLine(clientipe + ":" + message, ConsoleColor.White);
                    var resMsg = "<?xml version=\"1.0\"?>" +
                        "<ZXEMC>" +
                        "<CommandID>ConnectServerResp</CommandID>" +
                        " <ConnectResult>1</ConnectResult>" +
                        "</ZXEMC>";
                    socket.Send(Encoding.UTF8.GetBytes(resMsg));
                    socket.BeginReceive(bs, 0, bs.Length, SocketFlags.None, new AsyncCallback(ReceiveMsg), socket);
                }
                catch (Exception ex)
                {
                    var mm = ex.Message;
                    count--;
                    Console.WriteLine(clientipe + "断开连接" + (count), ConsoleColor.Red);
                }

            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        public static T Deserialize<T>(string xmlContent)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (StringReader strReader = new StringReader(xmlContent))
            {
                XmlReader xmlReader = XmlReader.Create(strReader);
                return (T)xs.Deserialize(xmlReader);
            }
        }

    }
    [XmlRoot("ZXEMC")]
    public class ConnectServer
    {
        public string ID { get; set; }
        public string HostName { get; set; }


    }

    public class GitTest
    {
        public string Key { get; set; }
        public string Na { get; set; }
        public string Cre { get; set; }
        public string Sex{ get; set; }
        public string Age{ get; set; }
        public string Fast{ get; set; }
    }
}
