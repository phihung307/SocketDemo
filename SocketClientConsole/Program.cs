using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using BUS.Models;

// State object for receiving data from remote device.  
public class StateObject
{
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 256;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
}
public class AsynchronousClient
{
    // The port number for the remote device.  
    private const int port = 11000;

    // ManualResetEvent instances signal completion.  
    private static ManualResetEvent connectDone =
        new ManualResetEvent(false);
    private static ManualResetEvent sendDone =
        new ManualResetEvent(false);
    private static ManualResetEvent receiveDone =
        new ManualResetEvent(false);

    // The response from the remote device.  
    private static String response = String.Empty;
    private static bool checkLogin = false;
    private static void StartClient()
    {
        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
            Console.WriteLine("IP server: ");
            var IP = Console.ReadLine();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(IP);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
            // Create a TCP/IP socket.  
            Socket client = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.  
            client.BeginConnect(remoteEP,
          new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            while (true)
            {
                string? choose = string.Empty;
                while (checkLogin == false)
                {
                    Console.WriteLine("1. Login");
                    Console.WriteLine("2. Resgiter");
                    choose = Console.ReadLine();
                    if (choose == "2")
                    {
                        if (Register(client))
                        {
                            if (Login(client))
                            {
                                GetDataMoney(client);
                            };
                        }
                    }
                    else if (choose == "1")
                    {
                        if (Login(client))
                        {
                            GetDataMoney(client);
                        };
                    }
                    else
                    {
                        Console.WriteLine("Enter your choose again:");
                    }
                }

            }

            // Receive the response from the remote device.  
            // Write the response to the console.  



            // Release the socket.  
            //client.Shutdown(SocketShutdown.Both);
            //client.Close();

        }
        catch (Exception e)
        {
            StartClient();
            Console.WriteLine(e.Message);
        }
    }
    private static bool Register(Socket client)
    {
        var user = new User();
        Console.WriteLine("UserName: ");
        user.UserName = Console.ReadLine();
        Console.WriteLine("Password: ");
        user.Password = Console.ReadLine();
        SendRegister(client, user);
        sendDone.WaitOne();
        sendDone = new ManualResetEvent(false);
        Receive(client);
        receiveDone.WaitOne();
        receiveDone = new ManualResetEvent(false);
        if (response.Contains("register succes"))
        {
            Console.WriteLine("{0}", response);
            response = String.Empty;
            return true;
        }
        else
        {
            Console.WriteLine("{0}", response);
            response = String.Empty;
        }
        return false;
    }
    private static bool Login(Socket client)
    {
        var user = new User();
        Console.WriteLine("LOGIN");

        Console.WriteLine("UserName: ");
        user.UserName = Console.ReadLine();
        Console.WriteLine("Password: ");
        user.Password = Console.ReadLine();
        SendLogin(client, user);
        sendDone.WaitOne();
        sendDone = new ManualResetEvent(false);
        Receive(client);
        receiveDone.WaitOne();
        receiveDone = new ManualResetEvent(false);
        if (response.Contains("login succes"))
        {
            Console.WriteLine("{0}", response);
            response = String.Empty;
            checkLogin = true;
            return true;
        }
        else
        {
            Console.WriteLine("{0}", response);
            response = String.Empty;
        }
        return false;
    }

    private static void GetDataMoney(Socket client)
    {
        while (checkLogin)
        {
            var user = new User();
            Console.WriteLine("\n CURRENCY PRICE TO VND");
            Console.WriteLine(" -- Enter 'Out' to logout or \nEnter currency: AUD or CAD or CHF or EUR or GBP or JPY or USD to get currency price");
            var currency = Console.ReadLine();
            if (currency?.ToLower() == "out")
            {
                SendOut(client);
                sendDone.WaitOne();
                sendDone = new ManualResetEvent(false);
                Receive(client);
                receiveDone.WaitOne();
                receiveDone = new ManualResetEvent(false);
                Console.WriteLine("{0}", response);
                // Release the socket.  
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                checkLogin = false;
                Environment.Exit(0);
            }
            Console.WriteLine("Enter day (example: 12/31/2021): ");
            var day = Console.ReadLine();
            DateTime date;
            while (!DateTime.TryParse(day, out date))
            {
                Console.WriteLine("Wrong date please try again. Enter day (example: 12/31/2021): ");
                day = Console.ReadLine();
            }

            SendGetCurrency(client, currency ?? string.Empty, date);
            sendDone.WaitOne();
            sendDone = new ManualResetEvent(false);
            Receive(client);
            receiveDone.WaitOne();
            receiveDone = new ManualResetEvent(false);
            Console.WriteLine("{0}", response);
            response = String.Empty;
        }
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete the connection.  
            client.EndConnect(ar);

            Console.WriteLine("Socket connected to {0}",
                client.RemoteEndPoint.ToString());

            // Signal that the connection has been made.  
            connectDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Receive(Socket client)
    {
        try
        {
            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = client;

            // Begin receiving the data from the remote device.  
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the state object and the client socket
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            // Read data from the remote device.  
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.  
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                receiveDone.Set();
                //// Get the rest of the data.  
                //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //    new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to server. Waiting!!!", bytesSent);

            // Signal that all bytes have been sent.  
            sendDone.Set();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    private static void SendLogin(Socket client, User user)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes("login" + "\n" + user.UserName + "\n" + user.Password);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendRegister(Socket client, User user)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes("register" + "\n" + user.UserName + "\n" + user.Password);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendGetCurrency(Socket client, string currency, DateTime date)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes("getcurrency" + "\n" + currency + "\n" + date);

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendOut(Socket client)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes("Out <EOF>");

        // Begin sending the data to the remote device.  
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        if (part1 && part2)
            return false;
        else
            return true;
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}
