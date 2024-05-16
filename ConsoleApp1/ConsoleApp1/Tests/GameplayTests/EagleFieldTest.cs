using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Tests.ConnectionTests;
using NUnit.Framework;

namespace ConsoleApp1.Tests.GameplayTests;

[TestFixture]
public class EagleFieldTest
{
    private string boardPath = @"..\\..\\..\\AdditionalExamples\\configs\\boardconfigTest1.json";
    private string gamePath = @"..\\..\\..\\AdditionalExamples\\configs\\gameconfigTest1.json";

    [Test]
    public void Test()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);
        gameManager.clientManager = new ClientManager();
        gameManager.clientManager.Setup(403, boardConfig, gameConfig, ClientManagerTest.GetPlayers,
            gameManager.gameboard.BoardState);

        Player player = new Player("ww", roleEnum.PLAYER);
        player.InitializeCharakter(new StartField(1, 1, directionEnum.EAST), 3, 3, characterEnum.GOLLUM, 0,
            new List<CheckPoint>());
        gameManager.players.Add(player);

        player = new Player("e2", roleEnum.PLAYER);
        player.InitializeCharakter(new StartField(9, 9, directionEnum.EAST), 3, 3, characterEnum.GOLLUM, 0,
            new List<CheckPoint>());
        gameManager.players.Add(player);

        gameManager.getTurnOrder();
        gameManager.TriggerEagleEvent();

        player = gameManager.players.First(p => p.Name == "ww");
        if (player.Charakter.X is not 9 || player.Charakter.Y is not 9) Assert.Fail("False Coordinates");

        player = gameManager.players.First(p => p.Name == "e2");
        if (player.Charakter.X is not 9 || player.Charakter.Y is not 1) Assert.Fail("False Coordinates");
    }
}