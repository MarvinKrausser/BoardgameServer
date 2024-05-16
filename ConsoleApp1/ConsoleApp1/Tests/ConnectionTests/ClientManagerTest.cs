using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests.ConnectionTests;

[TestFixture]
public class ClientManagerTest
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
        if (task.Wait(TimeSpan.FromSeconds(7)))
        {
            Assert.Pass("First Pass Option");
        }
        else
        {
            Assert.Fail("Timeout");
        }
    }

    public static List<Player> GetPlayers()
    {
        return new List<Player>();
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

        clientManager.Setup(54008, b, g, GetPlayers, new BoardState());
        clientManager.NewClient += NewClient;
        clientManager.Pause += Pause;
        clientManager.ClientDisconnected2 += ClientDisconnected2;
        clientManager.CharacterChoice += CharacterChoice;

        ClientTest c = new ClientTest();
        ClientList.Add(c);
        c.Setup();

        c = new ClientTest();
        ClientList.Add(c);
        c.Setup();

        clientManager.readySemaphore.WaitOne();
        if (clients.Count != 2) Assert.Fail("Wrong number of Clients");
        clientManager.EndGame();
    }


    public static void NewClient(string reconnectToken, string name, roleEnum role, EventArgs eventArgs)
    {
        clients.Add(new ClientTestClass(reconnectToken, name));
    }

    public static void ClientDisconnected2(string reconnectToken, string name, EventArgs eventArgs)
    {
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
        private characterEnum character = characterEnum.SAM;
        private Thread t;

        public void Setup()
        {
            t = new Thread(Start);
            t.Start();
        }

        public void Start()
        {
            WebSocket client = new WebSocket("ws://127.0.0.1:54008/");
            client.Connect();
            client.OnMessage += NewMessageClientEvent;

            name += "E";
            HelloServer s = new HelloServer(new HelloServer.Data(roleEnum.PLAYER, name));
            client.Send(JsonManager.ConvertToJason(s));

            PlayerReady ready = new PlayerReady(new PlayerReady.Data(true));
            client.Send(JsonManager.ConvertToJason(ready));

            sem.WaitOne();

            CharacterChoice choice = new CharacterChoice(new CharacterChoice.Data(character));
            client.Send(JsonManager.ConvertToJason(choice));
        }

        public void NewMessageClientEvent(object? sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Data);
            if (JsonManager.GetTypeJson(e.Data).Equals(messageEnum.CHARACTER_OFFER))
            {
                character = JsonManager.DeserializeJson<CharacterOffer>(e.Data).data.characters[0];
                TestContext.Progress.WriteLine(character);
                sem.Set();
            }
        }
    }
}