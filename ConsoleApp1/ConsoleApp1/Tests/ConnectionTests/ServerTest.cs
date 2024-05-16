using System.Diagnostics;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests.ConnectionTests;

[TestFixture]
public class ServerTest
{
    public static List<string> clients = new List<string>();
    public static AutoResetEvent ev = new AutoResetEvent(false);

    [Test]
    [Repeat(100)]
    public void Test()
    {
        var task = Task.Run(() => Do());
        if (task.Wait(TimeSpan.FromSeconds(5)))
        {
            Assert.Pass();
        }
        else
        {
            Assert.Fail("Timeout");
        }
    }

    public void Do()
    {
        Server listener = new Server();
        listener.SetupServer(54000);
        listener.NewMessage += NewMessageEvent;
        listener.ClientConnected += ClientConnectedEvent;
        listener.ClientDisconnected += CleintDisconnectedEvent;

        WebSocket client = new WebSocketSharp.WebSocket("ws://127.0.0.1:54000/");
        client.Connect();
        client.OnMessage += NewMessageClientEvent;

        while (!clients.Any())
        {
        }

        listener.SendData("First Message", new[] { clients[0] });

        ev.WaitOne();

        client.Send("Second Message");

        ev.WaitOne();

        WebSocket[] sockets = new WebSocket[10];


        for (int i = 0; i < 10; i++)
        {
            sockets[i] = (new WebSocket("ws://127.0.0.1:54000/"));
            sockets[i].Connect();
            ev.WaitOne();
        }

        for (int i = 0; i < 5; i++)
        {
            sockets[i].Close();
            ev.WaitOne();
        }

        if (clients.Count != 6) Assert.Fail("Wrong number of Clients");
    }

    public static void NewMessageEvent(string message, string clientID, EventArgs e)
    {
        Console.WriteLine("Server: " + message);
        ev.Set();
    }

    public static void ClientConnectedEvent(string clientId, EventArgs eventArgs)
    {
        clients.Add(clientId);
        ev.Set();
    }

    public static void CleintDisconnectedEvent(string clientId, EventArgs eventArgs)
    {
        clients.Remove(clientId);
        ev.Set();
    }

    public static void NewMessageClientEvent(object? sender, MessageEventArgs e)
    {
        Console.WriteLine("Client: " + e.Data);
        ev.Set();
    }
}

[SetUpFixture]
public class SetupTrace
{
    private string boardPath = @"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json";
    private string gamePath = @"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json";
    
    [OneTimeSetUp]
    public void StartTest()
    {
        String boardString = JsonManager.getConfigJson(boardPath);

        String gameString = JsonManager.getConfigJson(gamePath);
        
        Trace.Listeners.Add(new ConsoleTraceListener());
        JsonManager.ChooseDirectory(boardString, gameString);
    }

    [OneTimeTearDown]
    public void EndTest()
    {
        Trace.Flush();
    }
}