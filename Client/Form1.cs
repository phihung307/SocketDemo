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

namespace Client
{
    public partial class Form1 : Form
    {
        static ChatPrivate chatPrivate;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            Connect();

        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            Send();
            if (textBoxChat.Text != string.Empty)
            {
                AddMessage("Tôi: " + textBoxChat.Text);
                textBoxChat.Clear();
            }
        }
        IPEndPoint IP;
        Socket client;
        void Connect()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9999);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                client.Connect(IP);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Not Connect to server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();

        }
        void close()
        {
            client.Close();
        }
        void Send()
        {
            if (client.Connected == false)
            {
                MessageBox.Show("Not Connect to server. Server is disconnected!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (textBoxChat.Text != string.Empty)
                {
                    client.Send(Serialize(textBoxChat.Text));
                }
            }
        }
        void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);

                    string message = (string)Deserialize(data);

                    if (message != string.Empty)
                    {
                        if (isPrivate(message))
                        {
                            message = message.Substring(9);
                            // if ()
                            {
                                ChatPrivate.message = message;
                                ChatPrivate.UserChat = message.Substring(0, 15);
                                ChatPrivate.client = client;
                                if (chatPrivate == null)
                                {
                                    chatPrivate = new ChatPrivate();

                                    chatPrivate.ShowDialog();
                                }
                            }
                        }
                        else
                        {
                            AddMessage(message);
                        }
                    }

                }
            }
            catch
            {
                close();
            }
        }

        void AddMessage(string s)
        {
            if (s.Contains("is connected") || s.Contains("is disconnected"))
            {
                listOnline.Items.Add(new ListViewItem() { Text = s });
            }
            else
            {
                listViewMsg.Items.Add(new ListViewItem() { Text = s });
            }
        }

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

        private void Window_Closed(object sender, EventArgs e)
        {
            close();
        }

        private void Open(object sender, EventArgs e)
        {

            ChatPrivate.UserChat = listOnline.SelectedItems[0].SubItems[0].Text.ToString().Substring(0, 15);
            ChatPrivate.client = client;

            chatPrivate = new ChatPrivate();

            chatPrivate.ShowDialog();


        }
        bool isPrivate(string s)
        {
            if (s.Count() < 9) return false;
            if (s.Substring(0, 9) == "isPrivate")
            {
                return true;
            }
            return false;
        }
    }
}
