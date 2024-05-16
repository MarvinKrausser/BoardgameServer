using NUnit.Framework;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ConsoleApp1
{
    public delegate void NewMessageEventHandler(string message, string clientID, EventArgs e);

    public delegate void ClientDisconnectedEventHandler(string clientID, EventArgs e);

    public delegate void ClientConnectedEventHandler(string clientID, EventArgs e);

    public class Server
    {
        public event NewMessageEventHandler NewMessage; //subscribe to event for new Messages
        public event ClientDisconnectedEventHandler ClientDisconnected;
        public event ClientConnectedEventHandler ClientConnected;
        private WebSocketServer _listener;

        public void SetupServer(int port)
        {
            _listener = new WebSocketServer("ws://0.0.0.0:" + port);
            _listener.AddWebSocketService<Echo>("/",
                () => new Echo(NewMessage, ClientDisconnected, ClientConnected));
            _listener.Start();
            Console.WriteLine($"Server listens on Port: {port}");
        }

        public Task KickClientAsync(string clientID)
        {
            try
            {
                Console.WriteLine($"Kick Client: {clientID}");
                _listener.WebSocketServices["/"].Sessions.CloseSession(clientID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        public void SendData(string message, string[] clientIDs)
        {
            //Starts Tasks to send Data async
            foreach (var s in clientIDs)
            {
                Task.Run(() => SendDataAsync(message, s));
            }
        }

        public void SendDataNotAsync(string message, string[] clientIDs)
        {
            foreach (var s in clientIDs)
            {
                try
                {
                    Console.WriteLine($"Send Message: ClientID: {s}, Message: {message}");
                    _listener.WebSocketServices["/"].Sessions.SendTo(message, s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private Task SendDataAsync(string message, string clientID)
        {
            try
            {
                Console.WriteLine($"Send Message: ClientID: {clientID}, Message: {message}");
                _listener.WebSocketServices["/"].Sessions.SendTo(message, clientID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        public void Close()
        {
            Thread.Sleep(20);
            _listener.Stop();
            Console.WriteLine("server stopped");
        }

        public void SendMessageAndKick(string messages, string[] clientIDs)
        {
            //Starts Tasks to send Data and Kick async
            foreach (var s in clientIDs)
            {
                Task.Run(() => SendMessageAndKickAsync(messages, s));
            }
        }

        private Task SendMessageAndKickAsync(string messages, string clientID)
        {
                try
                {
                    Console.WriteLine($"Send Message: ClientID: {clientID}, Message: {messages}");
                    _listener.WebSocketServices["/"].Sessions.SendTo(messages, clientID);

                    Thread.Sleep(20);
                    
                    _listener.WebSocketServices["/"].Sessions.CloseSession(clientID);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            TestContext.Progress.WriteLine("done");
            return Task.CompletedTask;
        }
    }

    public class Echo : WebSocketBehavior
    {
        public event NewMessageEventHandler newMessage;
        public event ClientDisconnectedEventHandler clientDisconnected;
        public event ClientConnectedEventHandler clientConnected;

        public Echo(NewMessageEventHandler newMessage, ClientDisconnectedEventHandler clientDisconnected,
            ClientConnectedEventHandler clientConnected)
        {
            this.newMessage = newMessage;
            this.clientDisconnected = clientDisconnected;
            this.clientConnected = clientConnected;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            newMessage(e.Data, ID, EventArgs.Empty);
        }

        protected override void OnClose(CloseEventArgs e)
        {
            clientDisconnected(ID, EventArgs.Empty);
        }

        protected override void OnOpen()
        {
            clientConnected(ID, EventArgs.Empty);
        }
    }
}