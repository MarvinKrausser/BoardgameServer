using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests.ConnectionTests;

[TestFixture]
public class ClientManagerTestBanClient
{
    public static List<ClientTestClass> clients = new List<ClientTestClass>();
    public static ClientManager clientManager = new ClientManager();
    public static List<ClientTest> ClientList = new List<ClientTest>();
    public static string name = "";

    [Test]
    public void Test()
    {
        clients = new List<ClientTestClass>();
        var task = Task.Run(() => Do());
        if (task.Wait(TimeSpan.FromSeconds(4)))
        {
            Assert.Pass("First Pass Option");
        }
        else
        {
            Assert.Fail("Timeout");
        }
    }

    public static void Do()
    {
        var b = new BoardConfig();
        b.startFields = new[]
        {
            new BoardConfig.StartField(new[] { 2, 4 }, directionEnum.WEST),
            new BoardConfig.StartField(new[] { 3, 4 }, directionEnum.WEST)
        };

        var g = new GameConfig();
        g.characterChoiceTimeout = 10;

        clientManager.Setup(54003, b, g, ClientManagerTest.GetPlayers, new BoardState());
        clientManager.NewClient += NewClient;
        clientManager.Pause += Pause;
        clientManager.ClientBanned += ClientBanned;
        clientManager.CharacterChoice += CharacterChoice;

        ClientTest c = new ClientTest();
        ClientList.Add(c);
        c.Setup();

        c = new ClientTest();
        ClientList.Add(c);
        c.Setup();

        while (clientManager._clientsSet.Count == 0)
        {
        }

        Error error = new Error(new Error.Data("Test", 0));
        clientManager.BanClientByGameManager(clients[0].name, JsonManager.ConvertToJason(error));

        while (clients.Count != 1)
        {
        }

        if (clients.Count != 1) Assert.Fail("Wrong number of Clients in Test");
        if (clientManager._clientsSet.Count != 1) Assert.Fail("Wrong number of Clients in ClientManager");
        clientManager.EndGame();
    }


    public static void NewClient(string reconnectToken, string name, roleEnum role, EventArgs eventArgs)
    {
        clients.Add(new ClientTestClass(reconnectToken, name));
    }

    public static void ClientBanned(string reconnectToken, string name, bool hasStarted, EventArgs eventArgs)
    {
        clients.Remove(clients.Find(p => p.name.Equals(name)));
    }

    public static void CharacterChoice(string reconnectToken, string name, characterEnum character, EventArgs eventArgs)
    {
    }

    public static void Pause(string reconnectToken, string name, bool isPaused, EventArgs eventArgs)
    {
    }

    public class ClientTest
    {
        private AutoResetEvent sem = new AutoResetEvent(false);
        private characterEnum character;
        private Thread t;

        public void Setup()
        {
            t = new Thread(Start);
            t.Start();
        }

        public void Start()
        {
            WebSocket client = new WebSocket("ws://127.0.0.1:54003/");
            client.Connect();
            client.OnMessage += NewMessageClientEvent;
            client.OnClose += OnClose;

            name += "E";
            HelloServer s = new HelloServer(new HelloServer.Data(roleEnum.PLAYER, name));
            client.Send(JsonManager.ConvertToJason(s));

            sem.WaitOne();
        }

        private void NewMessageClientEvent(object? sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            if (JsonManager.GetTypeJson(e.Data).Equals("CHARACTER_OFFER"))
            {
                character = JsonManager.DeserializeJson<CharacterOffer>(e.Data).data.characters[0];
            }

            sem.Set();
        }

        private void OnClose(object? sender, CloseEventArgs closeEventArgs)
        {
            Console.WriteLine("Closed");
            sem.Set();
        }
    }
}