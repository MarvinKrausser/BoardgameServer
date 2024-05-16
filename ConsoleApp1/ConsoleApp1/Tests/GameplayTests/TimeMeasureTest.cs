using System.Diagnostics;
using System.Security.AccessControl;
using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Tests.ConnectionTests;
using NUnit.Framework;

namespace ConsoleApp1.Tests.GameplayTests;


[TestFixture]
public class TimeMeasureTest
{
    private string boardPath = @"..\\..\\..\\AdditionalExamples\\configs\\boardconfigTest1.json";
    private string gamePath = @"..\\..\\..\\AdditionalExamples\\configs\\gameconfigTest1.json";

    [Test]
    public void Test()
    {
        var stringWriter = new StringWriter();
        var originalConsoleOut = Console.Out;
        Console.SetOut(stringWriter);
        double time = 0;
        foreach (var unused in Enumerable.Range(0, 10))
        {
            try
            {
                time += Do();
            }
            catch (Exception e)
            {
                TestContext.Progress.WriteLine(e.StackTrace);
            }
        }

        time /= 10;
        Console.SetOut(originalConsoleOut);
        stringWriter.Dispose();
        Console.WriteLine($"{time} Milliseconds");
    }

    public double Do()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);
        
        GameManager gameManager = new GameManager(boardConfig, gameConfig);
        gameManager.clientManager = new ClientManager();
        gameManager.clientManager.Setup(3, boardConfig, gameConfig, ClientManagerTest.GetPlayers, gameManager.gameboard.BoardState);


        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM,
            3, gameManager.gameboard.CheckPoints);
        
        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player4 = new Player("Player 4", roleEnum.PLAYER);
        player4.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM,
            3, gameManager.gameboard.CheckPoints);
        
        Player player5 = new Player("Player 3", roleEnum.PLAYER);
        player5.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player6 = new Player("Player 4", roleEnum.PLAYER);
        player6.InitializeCharakter(gameManager.gameboard.getStartKoordinate(), 3, 3, characterEnum.SAM,
            3, gameManager.gameboard.CheckPoints);


        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);
        gameManager.players.Add(player4);
        gameManager.players.Add(player5);
        gameManager.players.Add(player6);


        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        foreach (var p in gameManager.players)
        {
            foreach (var unused in Enumerable.Range(0, 5))
            {
                Random random = new Random();
                int enumLength = Enum.GetValues(typeof(cardEnum)).Length;
                int randomIndex = random.Next(enumLength);
                cardEnum randomEnumValue = (cardEnum)randomIndex;


                gameManager.moveCharakter(p, new List<cardEnum>(){randomEnumValue});
                gameManager.shoot();
                gameManager.moveCharaktersOnRiver();
                gameManager.TriggerEagleEvent();
            }
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        
        gameManager.clientManager.EndGame();

        // Format and display the TimeSpan value.
        return ts.TotalMilliseconds;
    }

    public void showTime(Stopwatch st)
    {
        st.Stop();
        TimeSpan ts = st.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        Console.WriteLine(elapsedTime);
        st.Restart();
    }
}