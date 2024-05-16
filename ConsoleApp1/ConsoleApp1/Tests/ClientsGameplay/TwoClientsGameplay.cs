using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using Microsoft.VisualBasic;
using NUnit.Framework;
using WebSocketSharp;

namespace ConsoleApp1.Tests.ConnectionTests;

[TestFixture]
public class TwoClientsGameplay
{
    public static List<ClientTestClass> clients = new List<ClientTestClass>();
    public static ClientManager clientManager = new ClientManager();
    public static List<ClientTest> ClientList = new List<ClientTest>();
    public static string name = "";
    public static bool endServer = true;

    [Test]
    public void Test()
    {
        clients = new List<ClientTestClass>();
        var task = Task.Run(() => Do());
        if (task.Wait(TimeSpan.FromSeconds(100)))
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
        string port = "54009";

        string gameConfigJson = JsonManager.getConfigJson(@"..\\..\\..\\AdditionalExamples\\configs\\gameconfigTest1.json");
        string boardConfigJson =
        JsonManager.getConfigJson(@"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json");

        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);

        var player1 = new ClientTest();
        player1.Setup(true);
        var player2 = new ClientTest();
        player2.Setup(false);
        
        var gameManager = new GameManager(boardConfig, gameConfig);
        gameManager.startGame(Int32.Parse(port));
    }

    public class ClientTest
    {
        private characterEnum character = characterEnum.SAM;
        private Thread t;
        private WebSocket client;
        private bool firstCardEvent = true;
        private bool disc;
        private bool recivedCardOffer;

        public void Setup(bool disc)
        {
            t = new Thread(()=> Start(disc));
            t.Start();
        }

        public void Start(bool disc)
        {
            this.disc = disc;
            Thread.Sleep(2000);
            client = new WebSocket("ws://127.0.0.1:54009/");
            client.Connect();
            client.OnMessage += NewMessageClientEvent;

            name += "E";
            HelloServer s = new HelloServer(new HelloServer.Data(roleEnum.PLAYER, name));
            client.Send(JsonManager.ConvertToJason(s));
            
            endServer = false;
            TestContext.Progress.WriteLine("start end");
        }
        
        public void NewMessageClientEvent(object? sender, MessageEventArgs e)
        {
            TestContext.Progress.Write("Message");

            switch (JsonManager.GetTypeJson(e.Data))
            {
                case messageEnum.HELLO_CLIENT:
                    PlayerReady ready = new PlayerReady(new PlayerReady.Data(true));
                    client.Send(JsonManager.ConvertToJason(ready));
                    break;
                case messageEnum.CHARACTER_OFFER: 
                    character = JsonManager.DeserializeJson<CharacterOffer>(e.Data).data.characters[0];
                    TestContext.Progress.WriteLine(character);
                    
                    CharacterChoice choice = new CharacterChoice(new CharacterChoice.Data(character));
                    client.Send(JsonManager.ConvertToJason(choice));
                    break;
                case messageEnum.CARD_OFFER:
                    if (recivedCardOffer)
                    {
                        CardChoice cardChoice = new CardChoice(new CardChoice.Data(new[]
                            { cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY }));
                        client.Send(JsonManager.ConvertToJason(cardChoice)); 
                        
                        TestContext.Progress.WriteLine("client 2 end");
                        GoodByeServer goodBye2 = new GoodByeServer(new GoodByeServer.Data());
                        client.Send(JsonManager.ConvertToJason(goodBye2));
                    }
                    else
                    {
                        CardChoice cardChoice = new CardChoice(new CardChoice.Data(new[]
                            { cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY, cardEnum.EMPTY }));
                        client.Send(JsonManager.ConvertToJason(cardChoice));    
                    }
                    recivedCardOffer = true;
                    break;
                case messageEnum.CARD_EVENT:
                    if (firstCardEvent)
                    {
                        PauseRequest pauseRequest = new PauseRequest(new PauseRequest.Data(true));
                        client.Send(JsonManager.ConvertToJason(pauseRequest));

                        pauseRequest.data.pause = false;
                        client.Send(JsonManager.ConvertToJason(pauseRequest));
                        firstCardEvent = false;
                        
                        if (disc)
                        {
                            client.Close();
                            TestContext.Progress.WriteLine("client 1 end");
                            GoodByeServer goodBye = new GoodByeServer(new GoodByeServer.Data());
                            client.Send(JsonManager.ConvertToJason(goodBye));
                        }
                    }
                    break;
            }
        }
    }
}