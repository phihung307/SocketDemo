using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class ChatPrivate : Form
    {
        public static string UserChat;
        public static Socket client;
        public static string message;
        public ChatPrivate()
        {
            InitializeComponent();
            AddMessage("Bạn đang nhắn tin đến cho: " + UserChat);
            AddMessage(message);
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();

        }
        void AddMessage(string s)
        {
            listViewMsg.Items.Add(new ListViewItem() { Text = s });
        }
        void Send()
        {
            if (textBoxChat.Text != string.Empty)
            {
                client.Send(Serialize(UserChat + textBoxChat.Text));
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
                        AddMessage(message.Substring(9));
                    }

                }
            }
            catch (Exception ex)
            {

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

        private void buttonSend_Click(object sender, EventArgs e)
        {
            Send();
            if (textBoxChat.Text != String.Empty)
            {
                AddMessage("Tôi: " + textBoxChat.Text);
                textBoxChat.Clear();

            }
        }
    }
}