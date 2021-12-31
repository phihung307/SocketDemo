using BUS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http.Headers;
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
    public static string bearerToken = string.Empty;
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
        StartListening();

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
            try
            {
                while (true)
                {
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
    new AsyncCallback(ReadCallback), state);
                }
            }
            catch (Exception)
            {
                while (true)
                {
                    StateObject state = new StateObject();
                    state.workSocket = handler;
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
    new AsyncCallback(ReadCallback), state);
                }
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
                Send(handler, "Log out success");
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
                                Send(handler, "register fail, please register another user");
                            }
                            break;
                        case "getcurrency":
                            var currency = GetCurrency(c[1],DateTime.Parse(c[2]));
                            if (currency != null)
                            {
                                string response =String.Format("{0} Buy:{1} Sell: {2}", currency.Currency,currency.Buy, currency.Sell);
                                Send(handler, response);
                            }
                            else
                            {
                                Send(handler, "Currency wrong or date not have data");
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
        try
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        catch (Exception){
            return false;
        }
        
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

    public static CurrencyPrice GetCurrency(string name, DateTime date)
    {
        try
        {
            using (var db = new SocketContext())
            {
                var currencyPrice = db.CurrencyPrices.FirstOrDefault(x => x.Currency == name && x.CreatedDate.Value.Date == date);
                if (currencyPrice != null)
                {
                    return currencyPrice;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
        return null;
    }
    public static void GetData()
    {
        if (string.IsNullOrEmpty(bearerToken))
        {
            bearerToken = GetAccessToken();
        }
        const string url = "https://vapi.vnappmob.com/api/v2/exchange_rate/sbv";
        HttpClient client = new HttpClient();
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var responseMessage = client.GetAsync(url).Result;
        if (responseMessage.IsSuccessStatusCode)
        {
            var data = responseMessage.Content.ReadAsStringAsync().Result;

            var responseJOject = JsonConvert.DeserializeObject<JObject>(data);
            var result = responseJOject["results"];
            var dataCurrency = JsonConvert.DeserializeObject<List<CurrencyPrice>>(result.ToString());
            foreach (var item in dataCurrency)
            {
                item.CreatedDate = DateTime.Now.Date;
            }
            using (var db = new SocketContext())
            {
                var currencys = db.CurrencyPrices.Where(x =>x.CreatedDate.Value.Date == DateTime.Now.Date && dataCurrency.Select(y=>y.Currency).Contains(x.Currency)).ToList();
                if (!currencys.Any())
                {
                    db.CurrencyPrices.AddRange(dataCurrency);
                }
                else
                {
                    db.CurrencyPrices.RemoveRange(currencys);
                    db.CurrencyPrices.AddRange(dataCurrency);
                }
                db.SaveChanges();
            }
        }
        else
        {
            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized
                || responseMessage.StatusCode == HttpStatusCode.Forbidden)
            {
                bearerToken = string.Empty;
                GetData();
            }
        }

    }
    public static string GetAccessToken()
    {
        var token = string.Empty;
        try
        {
            using (var client = new HttpClient())
            {
                var responseMessage = client.GetAsync("https://vapi.vnappmob.com/api/request_api_key?scope=exchange_rate").Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    var data = responseMessage.Content.ReadAsStringAsync().Result;
                    var responseJOject = JsonConvert.DeserializeObject<JObject>(data);
                    token = (string)responseJOject["results"];
                }
            }
        }
        catch (Exception)
        {
        }
        return token;
    }

    public static void CheckLogout()
    {
        while(true)
        {
            if (ListConnected.Any())
            {
                for (int i = 0; i < ListConnected.Count; i++)            
                {
                    if (ListConnected[i]!= null && !SocketConnected(ListConnected[i]))
                    {
                        Console.WriteLine("{0} disconnect ", ListConnected[i].RemoteEndPoint);
                        ListConnected.Remove(ListConnected[i]);                      
                    }
                }
            }
           
        }
    }
    public static int Main(String[] args)
    {
        var startTimeSpan = TimeSpan.Zero;
        var periodTimeSpan = TimeSpan.FromMinutes(30);

        var timer = new System.Threading.Timer((e) =>
        {
            GetData();
        }, null, startTimeSpan, periodTimeSpan);
        var checkconnect = new Thread(CheckLogout);
        checkconnect.IsBackground = true;
        checkconnect.Start();
        StartListening();
        return 0;
    }
}