using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Gameplay.Utility;
using NUnit.Framework;

namespace ConsoleApp1.Tests.GameplayTests;

[TestFixture]
public class GameboardTest
{
    private Gameboard _gameboard;
    
    private string boardPath = @"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json";
    private string gamePath = @"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json";

    [Test]
    public void CreateGameboard()
    {
        var boardConfigString = JsonManager.getConfigJson(boardPath);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigString);

        Gameboard gameboard = new Gameboard(boardConfig,new List<Player>());
        Tile[,] tiles = gameboard.tiles;
        gameboard.PrintGameboard(tiles);

        for (int i = 0; i < gameboard.Height; i++)
        {
            for (int j = 0; j < gameboard.Width; j++)
            {
                Console.Write("\n Tile {0} {1}: ", i, j);
                foreach (var direction in tiles[i, j].walls)
                {
                    Console.Write(direction + " ");
                }
            }
        }
    }

    [Test]
    public void ChooseStartFieldTest()
    {
        List<StartField> list = new List<StartField>();
        var boardConfigString = JsonManager.getConfigJson(boardPath);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigString);


        Gameboard gameboard = new Gameboard(boardConfig, new List<Player>());
        //Console.WriteLine("There are {0} Start Fields", gameboard.startFields.Count);

        list.Add(gameboard.getStartKoordinate());
        list.Add(gameboard.getStartKoordinate());
        list.Add(gameboard.getStartKoordinate());
        list.Add(gameboard.getStartKoordinate());


        foreach (var item in list)
        {
            Console.Write(item + " ");
        }
    }

    [Test]
    public void isWakableTest()
    {
        var boardConfigString = JsonManager.getConfigJson(boardPath);
        var boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigString);

        Gameboard gameboard = new Gameboard(boardConfig, new List<Player>());
        Charakter charakter1 = new Charakter(new StartField(4, 4, directionEnum.EAST), 3, 3, characterEnum.SAM,
            new List<CheckPoint>(), 3);
        Player player1 = new Player("hello", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(4, 4, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameboard.CheckPoints);

        Console.WriteLine(gameboard.isWalkable(player1.Charakter.X, player1.Charakter.Y, directionEnum.EAST));
    }
}