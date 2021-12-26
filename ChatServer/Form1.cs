using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;
            Connect();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            foreach (var item in SocketList)
            {
                Send(item);
            }

            AddMessage(textBoxChat.Text);

        }
        IPEndPoint IP;
        Socket server;
        List<Socket> SocketList;
        List<ChatModel> ListChatModel;
        void Connect()
        {
            SocketList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            ListChatModel = new List<ChatModel>();
            server.Bind(IP);
            Thread listen = new Thread(() =>
            {
               
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        SocketList.Add(client);
                        ChatModel chat = new ChatModel();
                        chat.Socket = client;
                        chat.UserName = client.RemoteEndPoint.ToString();
                        ListChatModel.Add(chat);
                        foreach (var item in SocketList)
                        {

                            if (item != client)
                            {
                                item.Send(Serialize(client.RemoteEndPoint.ToString() + " is connected"));
                                client.Send(Serialize(item.RemoteEndPoint.ToString() + " is connected"));
                            }
                            else
                            {
                                client.Send(Serialize("You are connected. Your name is: "+client.RemoteEndPoint.ToString()));
                            }
                        }
                        AddMessage(client.RemoteEndPoint.ToString() + " is connected");
                       
                        if (client.Connected == true)
                        {
                            Thread receive = new Thread(Receive);
                            receive.Priority = ThreadPriority.Lowest;
                            receive.IsBackground = true;
                            receive.Start(chat);
                        }
                    }
                }
                catch (Exception ex)
                {
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
            });
            listen.IsBackground = true;
            listen.Start();


        }
        void Close()
        {
            server.Close();
        }
        void Send(Socket socket)
        {
            if (textBoxChat.Text != string.Empty)
            {
                socket.Send(Serialize("Server: " + textBoxChat.Text));
            }
        }
        void Receive(object obj)
        {
            ChatModel chat = obj as ChatModel;
            Socket client = chat.Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                    
                    if (isPrivate(message)!=-1)
                    {                        
                        SocketList[isPrivate(message)].Send(Serialize("isPrivate"+ chat.UserName + ": " + message.Substring(15)));
                        AddMessage(message);
                    }
                    else
                    {
                        foreach (var item in ListChatModel)
                        {
                            if (item.Socket != client)
                            {
                             
                                item.Socket.Send(Serialize(chat.UserName + ": " + message));
                            }
                        }
                        AddMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (var item in SocketList)
                {
                    if (item != client)
                    {
                        item.Send(Serialize(client.RemoteEndPoint.ToString() + " is disconnected"));
                    }
                }
                SocketList.Remove(client);
                ListChatModel.Remove(chat);
                AddMessage(client.RemoteEndPoint.ToString() + " is disconnected");
                client.Close();
            }

        }

        void AddMessage(string s)
        {
            listViewMsg.Items.Add(new ListViewItem() { Text = s });
        }
        static ASCIIEncoding encoding = new ASCIIEncoding();
        byte[] Serialize(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return stream.ToArray();

        }
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);

        }

        // trả về vị trí của người nhận tin nhắn trong List socket các client
        int isPrivate(string s)
        {
            if (s.Count() < 15) { return -1; }
            for (int i=0;i< SocketList.Count; i++)
            {
                if (s.Substring(0, 15) == SocketList[i].RemoteEndPoint.ToString())
                {
                    return i;
                }
            }
            return -1;
        }
        private void listViewMsg_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
