﻿using BUS.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// State object for reading client data asynchronously  
public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;
}

public class AsynchronousSocketListener
{
    // Thread signal.  
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    public static List<Socket> ListConnected = new List<Socket>();
    public AsynchronousSocketListener()
    {
    }

    public static void StartListening()
    {
        // Establish the local endpoint for the socket.  
        // The DNS name of the computer  
        // running the listener is "host.contoso.com".  
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);
        // Bind the socket to the local endpoint and listen for incoming connections.  
        try
        {
            while (true)
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");

                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  
        allDone.Set();

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);
        ListConnected.Add(handler);
        // Create the state object.          
        Console.WriteLine(handler.RemoteEndPoint?.ToString() + " is connected. Total:" + ListConnected.Count());
        Thread listen = new Thread(() =>
        {
            while (true)
            {
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
new AsyncCallback(ReadCallback), state);      
            }
        });
        listen.IsBackground = true;
        listen.Start();
      
    }

    public static void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
       if (!SocketConnected(handler))
        {
            return;
        }
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {
                Send(handler, "You Out");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            else
            {
                // Not all data received. Get more.  
                //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                //new AsyncCallback(ReadCallback), state);
                var c = content.Split();
                if (c?.Count() > 0)
                {
                    switch (c[0].ToLower())
                    {
                        case "login":

                            if (Login(new User { UserName = c[1], Password = c[2] }))
                            {
                                Send(handler, "login success");
                            }
                            else
                            {
                                Send(handler, "login fail");
                            }
                            break;
                        case "register":
                            if (Register(new User { UserName = c[1], Password = c[2] }))
                            {
                                Send(handler, "register success");
                            }
                            else
                            {
                                Send(handler, "register fail");
                            }
                            break;
                        default:
                            break;
                    }
                }
              
            }
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        Console.WriteLine("Send data to:" + handler?.RemoteEndPoint?.ToString());
        handler?.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
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

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
            Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    public static bool Register(User user)
    {
        try
        {
            using (var db = new SocketContext())
            {
                var u = db.Users.FirstOrDefault(x => x.UserName == user.UserName);
                if (u != null)
                {
                    return false;
                }
                db.Users.Add(user);
                if (db.SaveChanges() > 0)
                {
                    return true;
                };
            }
        }
        catch (Exception)
        {

            throw;
        }
        return false;
    }

    public static bool Login(User user)
    {
        try
        {
            using (var db = new SocketContext())
            {
                var u = db.Users.FirstOrDefault(x => x.UserName == user.UserName && x.Password == user.Password);
                if (u != null)
                {
                    return true;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
        return false;
    }
    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}