using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Tests.ConnectionTests;
using NUnit.Framework;

namespace ConsoleApp1.Tests.GameplayTests;

[TestFixture]
public class GameManagerTest
{
    private string boardPath = @"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json";
    private string gamePath = @"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json";

        [Test]
    public void testNeighbours()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(2, 4, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(3, 4, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(4, 4, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);


        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);


        List<Player> neigbours = gameManager.findPushingNeighbours(player1, player1.Charakter.direction);

        foreach (var player in neigbours)
        {
            Console.WriteLine("Player at {0} {1} is going to get pushed", player.Charakter.X, player.Charakter.Y);
        }
    }

    [Test]
    public void testCharakterMovement()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(1, 1, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(2, 1, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(3, 1, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);


        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);

        gameManager.clientManager = new ClientManager();
        gameManager.clientManager.Setup(400, boardConfig, gameConfig, ClientManagerTest.GetPlayers, gameManager.gameboard.BoardState);

        List<cardEnum> instructions = new List<cardEnum>
            { cardEnum.MOVE_3, cardEnum.MOVE_3, cardEnum.MOVE_3, cardEnum.LEFT_TURN, cardEnum.MOVE_1, cardEnum.AGAIN };
        gameManager.moveCharakter(player1, instructions);

        foreach (var player in gameManager.players)
        {
            Console.WriteLine("{0} befindet sich an {1} {2} mit Richtung {3}. IsDead: {4}", player.Name,
                player.Charakter.X, player.Charakter.Y, player.Charakter.direction, player.Charakter.IsDead);
        }
    }

    [Test]
    public void testShoot()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);
        gameManager.clientManager = new ClientManager();
        gameManager.clientManager.Setup(3, boardConfig, gameConfig, ClientManagerTest.GetPlayers, new BoardState());
        
        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(1, 0, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(5, 0, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(2, 0, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player4 = new Player("Player 4", roleEnum.PLAYER);
        player4.InitializeCharakter(new StartField(2, 5, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);
        gameManager.players.Add(player4);

        gameManager.shoot();

        foreach (var player in gameManager.players)
        {
            Console.WriteLine("{1} has: {0} lifes left", player.Charakter.Lifes, player.Name);
        }
    }

    [Test]
    public void testTurnOrderCalculation()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        //Eye and Stelle [4,9]

        //distanz 1
        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(4, 2, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        //distanz 10
        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(6, 4, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        //distanz 1, vor player1
        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(4, 6, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player4 = new Player("Player 4", roleEnum.PLAYER);
        player4.InitializeCharakter(new StartField(2, 4, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);
        gameManager.players.Add(player4);

        List<Player> orderedPlayers = gameManager.getTurnOrder();

        foreach (var player in orderedPlayers)
        {
            var xDist = player.Charakter.X - gameManager.eye.X;
            var yDist = player.Charakter.Y - gameManager.eye.Y;

            var angle = (Math.Atan2(yDist, xDist)) * (180 / Math.PI);

            if (angle < 0)
            {
                angle += 360; // Adjust negative angles to be within [0, 360] range
            }

            // Shift the angles by 180 degrees
            angle += 90;
            if (angle >= 360)
            {
                angle -= 360; // Wrap angles greater than or equal to 360
            }

            Console.WriteLine("{0} Distance: {1}, Angle: {2}", player.Name, player.Charakter.pathToEye.Count, angle);
        }
    }

    [Test]
    public void PathfindingTest()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        //Eye and Stelle [4,9]

        //distanz 1
        Player player4 = new Player("Player 1", roleEnum.PLAYER);
        player4.InitializeCharakter(new StartField(9, 9, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        gameManager.players.Add(player4);

        List<Tile> path = gameManager.FindPath(player4, gameManager.gameboard.eye);

        foreach (var tile in path)
        {
            Console.WriteLine("[{0} {1}]", tile.X, tile.Y);
        }

        Console.WriteLine(path.Count);
    }

    [Test]
    public void moveCharakterOnRiverTest()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);
        gameManager.clientManager = new ClientManager();
        gameManager.clientManager.Setup(2, boardConfig, gameConfig, ClientManagerTest.GetPlayers, new BoardState());

        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(1, 2, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(5, 0, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(1, 5, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);
        player3.Charakter.turnOrder = 2;

        Player player4 = new Player("Player 4", roleEnum.PLAYER);
        player4.InitializeCharakter(new StartField(2, 7, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);
        player4.Charakter.turnOrder = 1;

        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);
        gameManager.players.Add(player4);

        TestContext.Progress.Write("Initialize river");
        gameManager.moveCharaktersOnRiver();

        foreach (var player in gameManager.players)
        {
            Console.WriteLine("{0} befindet sich an {1} {2} mit Richtung {3}", player.Name, player.Charakter.X,
                player.Charakter.Y, player.Charakter.direction);
        }
    }

    [Test]
    public void respawnAllDeadCharaktersTest()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        Player player1 = new Player("Player 1", roleEnum.PLAYER);
        player1.InitializeCharakter(new StartField(9, 9, directionEnum.NORTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player2 = new Player("Player 2", roleEnum.PLAYER);
        player2.InitializeCharakter(new StartField(1, 1, directionEnum.EAST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player3 = new Player("Player 3", roleEnum.PLAYER);
        player3.InitializeCharakter(new StartField(1, 2, directionEnum.SOUTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player4 = new Player("Player 1", roleEnum.PLAYER);
        player4.InitializeCharakter(new StartField(1, 3, directionEnum.WEST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player5 = new Player("Player 2", roleEnum.PLAYER);
        player5.InitializeCharakter(new StartField(1, 4, directionEnum.SOUTH), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);

        Player player6 = new Player("Player 3", roleEnum.PLAYER);
        player6.InitializeCharakter(new StartField(5, 5, directionEnum.WEST), 3, 3, characterEnum.SAM, 3,
            gameManager.gameboard.CheckPoints);


        gameManager.players.Add(player1);
        gameManager.players.Add(player2);
        gameManager.players.Add(player3);
        gameManager.players.Add(player4);
        gameManager.players.Add(player5);
        gameManager.players.Add(player6);

        Console.WriteLine("Initial");
        Console.WriteLine("Life: player1 = " + player1.Charakter.Lifes + "; player2 = " + player2.Charakter.Lifes +
                          "; player3 = " + player3.Charakter.Lifes + "; player4 = " + player4.Charakter.Lifes +
                          "; player5 = " + player5.Charakter.Lifes + "; player6 = " + player6.Charakter.Lifes);
        Console.WriteLine("Respawn Points: player1 X/Y: " + player1.Charakter.startField.X + "/" +
                          player1.Charakter.startField.Y + "; player2 X/Y: " + player2.Charakter.startField.X + "/" +
                          player2.Charakter.startField.Y + "; player3 X/Y: " + player3.Charakter.startField.X + "/" +
                          player3.Charakter.startField.Y + "; player4 X/Y: " + player4.Charakter.startField.X + "/" +
                          player4.Charakter.startField.Y + "; player5 X/Y: " + player5.Charakter.startField.X + "/" +
                          player5.Charakter.startField.Y + "; player6 X/Y: " + player6.Charakter.startField.X + "/" +
                          player6.Charakter.startField.Y);
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------------------------------");

        Console.WriteLine("Test 1: Test if respawn works if no player blocks a respawn point");
        Console.WriteLine();

        player3.Charakter.isHitEvent();
        player3.Charakter.isHitEvent();
        player3.Charakter.isHitEvent();
        player3.Charakter.deadRoundsLeft = 0;
        player4.Charakter.isHitEvent();
        player4.Charakter.isHitEvent();
        player4.Charakter.isHitEvent();
        player4.Charakter.deadRoundsLeft = 0;
        player5.Charakter.isHitEvent();
        player5.Charakter.isHitEvent();
        player5.Charakter.isHitEvent();
        player5.Charakter.deadRoundsLeft = 1;

        player5.Charakter.X = 5;
        player4.Charakter.X = 5;

        Console.WriteLine("isHitEvent Method on player3, player4 and player5");
        Console.WriteLine("Life: player1 = " + player1.Charakter.Lifes + "; player2 = " + player2.Charakter.Lifes +
                          "; player3 = " + player3.Charakter.Lifes + "; player4 = " + player4.Charakter.Lifes +
                          "; player5 = " + player5.Charakter.Lifes);
        Console.WriteLine("Is Player Dead? player1: " + player1.Charakter.IsDead + "; player2: " +
                          player2.Charakter.IsDead + "; player3: " + player3.Charakter.IsDead + "; player4: " +
                          player4.Charakter.IsDead + "; player5: " + player5.Charakter.IsDead);
        Console.WriteLine("Player Position: player1 X/Y: " + player1.Charakter.X + "/" + player1.Charakter.Y +
                          "; player2 X/Y: " + player2.Charakter.X + "/" + player2.Charakter.Y + "; player3 X/Y: " +
                          player3.Charakter.X + "/" + player3.Charakter.Y + "; player4 X/Y: " + player4.Charakter.X +
                          "/" + player4.Charakter.Y + "; player5 X/Y: " + player5.Charakter.X + "/" +
                          player5.Charakter.Y);
        Console.WriteLine("Dead Rounds Left: player1 = " + player1.Charakter.deadRoundsLeft + "; player2 = " +
                          player2.Charakter.deadRoundsLeft + "; player3 = " + player3.Charakter.deadRoundsLeft +
                          "; player4 = " + player4.Charakter.deadRoundsLeft + "; player5 = " +
                          player5.Charakter.deadRoundsLeft);
        Console.WriteLine();

        gameManager.respawnAllDeadCharakters();

        Console.WriteLine("Reviving all Dead Players");
        Console.WriteLine("Life: player1 = " + player1.Charakter.Lifes + "; player2 = " + player2.Charakter.Lifes +
                          "; player3 = " + player3.Charakter.Lifes + "; player4 = " + player4.Charakter.Lifes +
                          "; player5 = " + player5.Charakter.Lifes);
        Console.WriteLine("Is Player Dead? player1: " + player1.Charakter.IsDead + "; player2: " +
                          player2.Charakter.IsDead + "; player3: " + player3.Charakter.IsDead + "; player4: " +
                          player4.Charakter.IsDead + "; player5: " + player5.Charakter.IsDead);
        Console.WriteLine("Player Position: player1 X/Y: " + player1.Charakter.X + "/" + player1.Charakter.Y +
                          "; player2 X/Y: " + player2.Charakter.X + "/" + player2.Charakter.Y + "; player3 X/Y: " +
                          player3.Charakter.X + "/" + player3.Charakter.Y + "; player4 X/Y: " + player4.Charakter.X +
                          "/" + player4.Charakter.Y + "; player5 X/Y: " + player5.Charakter.X + "/" +
                          player5.Charakter.Y);
        Console.WriteLine();
        Console.WriteLine("------------------------------------------------------------------------------");

        Console.WriteLine("Test 2: Test if respawn works if other players are blocking respawn points");
        Console.WriteLine();

        player1.Charakter.isHitEvent();
        player3.Charakter.isHitEvent();
        player3.Charakter.isHitEvent();
        player3.Charakter.isHitEvent();
        player3.Charakter.deadRoundsLeft = 0;
        player4.Charakter.isHitEvent();
        player4.Charakter.isHitEvent();
        player4.Charakter.isHitEvent();
        player4.Charakter.deadRoundsLeft = 0;
        player5.Charakter.isHitEvent();
        player5.Charakter.isHitEvent();
        player5.Charakter.isHitEvent();
        player5.Charakter.deadRoundsLeft = 0;

        player3.Charakter.X = 2;
        player4.Charakter.X = 2;
        player5.Charakter.X = 2;
        player1.Charakter.X = 1;
        player1.Charakter.Y = 2;
        player2.Charakter.Y = 4;
        player6.Charakter.X = 4;

        player1.Charakter.Lembas -= 3;
        player4.Charakter.Lembas -= 3;
        player5.Charakter.Lembas -= 2;

        Console.WriteLine("isHitEvent Method on player3, player4 and player5");
        Console.WriteLine("Is Player Dead? player1: " + player1.Charakter.IsDead + "; player2: " +
                          player2.Charakter.IsDead + "; player3: " + player3.Charakter.IsDead + "; player4: " +
                          player4.Charakter.IsDead + "; player5: " + player5.Charakter.IsDead + "; player6: " +
                          player6.Charakter.IsDead);
        Console.WriteLine("Player Position: player1 X/Y: " + player1.Charakter.X + "/" + player1.Charakter.Y +
                          "; player2 X/Y: " + player2.Charakter.X + "/" + player2.Charakter.Y + "; player3 X/Y: " +
                          player3.Charakter.X + "/" + player3.Charakter.Y + "; player4 X/Y: " + player4.Charakter.X +
                          "/" + player4.Charakter.Y + "; player5 X/Y: " + player5.Charakter.X + "/" +
                          player5.Charakter.Y + "; player6 X/Y: " + player6.Charakter.X + "/" + player6.Charakter.Y);
        Console.WriteLine("Lifes/Lembas: player1: " + player1.Charakter.Lifes + "/" + player1.Charakter.Lembas +
                          "; player2: " + player2.Charakter.Lifes + "/" + player2.Charakter.Lembas + "; player3: " +
                          player3.Charakter.Lifes + "/" + player3.Charakter.Lembas + "; player4: " +
                          player4.Charakter.Lifes + "/" + player4.Charakter.Lembas + "; player5: " +
                          player5.Charakter.Lifes + "/" + player5.Charakter.Lembas + "; player6: " +
                          player6.Charakter.Lifes + "/" + player6.Charakter.Lembas);
        Console.WriteLine("Direction: player1 = " + player1.Charakter.direction + "; player2 = " +
                          player2.Charakter.direction + "; player3 = " + player3.Charakter.direction + "; player4 = " +
                          player4.Charakter.direction + "; player5 = " + player5.Charakter.direction + "; player6 = " +
                          player6.Charakter.direction);
        Console.WriteLine();

        gameManager.respawnAllDeadCharakters();

        Console.WriteLine("Reviving all Dead Players");
        Console.WriteLine("Is Player Dead? player1: " + player1.Charakter.IsDead + "; player2: " +
                          player2.Charakter.IsDead + "; player3: " + player3.Charakter.IsDead + "; player4: " +
                          player4.Charakter.IsDead + "; player5: " + player5.Charakter.IsDead + "; player6: " +
                          player6.Charakter.IsDead);
        Console.WriteLine("Player Position: player1 X/Y: " + player1.Charakter.X + "/" + player1.Charakter.Y +
                          "; player2 X/Y: " + player2.Charakter.X + "/" + player2.Charakter.Y + "; player3 X/Y: " +
                          player3.Charakter.X + "/" + player3.Charakter.Y + "; player4 X/Y: " + player4.Charakter.X +
                          "/" + player4.Charakter.Y + "; player5 X/Y: " + player5.Charakter.X + "/" +
                          player5.Charakter.Y + "; player6 X/Y: " + player6.Charakter.X + "/" + player6.Charakter.Y);
        Console.WriteLine("Lifes/Lembas: player1: " + player1.Charakter.Lifes + "/" + player1.Charakter.Lembas +
                          "; player2: " + player2.Charakter.Lifes + "/" + player2.Charakter.Lembas + "; player3: " +
                          player3.Charakter.Lifes + "/" + player3.Charakter.Lembas + "; player4: " +
                          player4.Charakter.Lifes + "/" + player4.Charakter.Lembas + "; player5: " +
                          player5.Charakter.Lifes + "/" + player5.Charakter.Lembas + "; player6: " +
                          player6.Charakter.Lifes + "/" + player6.Charakter.Lembas);
        Console.WriteLine("Direction: player1 = " + player1.Charakter.direction + "; player2 = " +
                          player2.Charakter.direction + "; player3 = " + player3.Charakter.direction + "; player4 = " +
                          player4.Charakter.direction + "; player5 = " + player5.Charakter.direction + "; player6 = " +
                          player6.Charakter.direction);
        Console.WriteLine();
    }

    [Test]
    public void testGetBoardState()
    {
        String boardString = JsonManager.getConfigJson(boardPath);
        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardString);

        String gameString = JsonManager.getConfigJson(gamePath);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameString);

        GameManager gameManager = new GameManager(boardConfig, gameConfig);

        BoardState boardState = gameManager.gameboard.BoardState;

        foreach (var lembasField in boardState.lembasFields)
        {
            Console.WriteLine("Field at: {0} {1} with {2} Lembas left", lembasField.position[0],
                lembasField.position[1], lembasField.amount);
        }
    }
}