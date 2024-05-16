using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;
using ConsoleApp1.Gameplay.Tiles;

namespace ConsoleApp1.Gameplay;

public class Gameboard
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<StartField> startFields;
    private List<StartField> tempStartFields;
    public List<CheckPoint> CheckPoints { get; set; }
    public Eye eye;
    private Random _random = new Random();
    private BoardConfig boardConfig;
    public Tile[,] tiles { get; set; }
    public static int MaxSpieler { get; }
    public readonly HashSet<EagleTile> eagleTiles = new();
    public List<Player> players;

    public BoardState BoardState
    {
        get
        {
            int i = 0;
            List<LembasField> tempLembasField = new List<LembasField>();
            foreach (var lembasFieldTile in lembasFieldTiles)
            {
                LembasField temp = new LembasField(new int[] { lembasFieldTile.X, lembasFieldTile.Y },
                    lembasFieldTile.amount);
                tempLembasField.Add(temp);
            }

            return new BoardState(tempLembasField.ToArray());
        }
        set => throw new NotImplementedException();
    }

    private List<LembasFieldTile> lembasFieldTiles;

    public Gameboard(BoardConfig boardConfig, List<Player> players)
    {
        this.boardConfig = boardConfig;
        CheckPoints = new List<CheckPoint>();
        lembasFieldTiles = new List<LembasFieldTile>();
        this.tiles = createBoard();
        this.players = players;
    }

    public bool GetIsOccupied(Tile tile)
    {
        return players.Any(p => p.Charakter.X == tile.X && p.Charakter.Y == tile.Y && !p.Charakter.IsDead);
    }

    //initialisiert das Gameboard. Für jedes Feld wird ein eigenes Objekt instanziiert
    private Tile[,] createBoard()
    {
        Width = boardConfig.width;
        Height = boardConfig.height;

        tiles = new Tile[boardConfig.width, boardConfig.height];
        startFields = new List<StartField>();

        for (int i = 0; i < boardConfig.width; i++)
        {
            for (int j = 0; j < boardConfig.height; j++)
            {
                tiles[i, j] = new Grass(i, j);
            }
        }

        foreach (var startField in boardConfig.startFields)
        {
            var x = startField.position[0];
            var y = startField.position[1];
            var direction = startField.direction;

            var tile = new StartField(x, y, direction); //initialize object that is on tile
            startFields.Add(tile);
            tiles[x, y] = tile; //place tile in array
        }

        tempStartFields = startFields;

        int orderCheckPoints = 1;
        foreach (var checkpoint in boardConfig.checkPoints)
        {
            var x = checkpoint[0];
            var y = checkpoint[1];

            var tile = new CheckPoint(x, y, orderCheckPoints); //initialize object that is on tile
            tiles[x, y] = tile; //place tile in array
            CheckPoints.Add(tile);

            orderCheckPoints++;
        }

        //liste der checkpoints ist sortiert
        CheckPoints = CheckPoints.OrderBy(x => x.order).ToList();
        foreach (var hole in boardConfig.holes)
        {
            var x = hole[0];
            var y = hole[1];

            var tile = new Hole(x, y); //initialize object that is on tile
            tiles[x, y] = tile; //place tile in array
        }

        foreach (var riverField in boardConfig.riverFields)
        {
            var x = riverField.position[0];
            var y = riverField.position[1];
            var direction = riverField.direction;

            var tile = new RiverField(x, y, direction); //initialize object that is on tile
            tiles[x, y] = tile; //place tile in array
        }

        foreach (var lembasField in boardConfig.lembasFields)
        {
            var x = lembasField.position[0];
            var y = lembasField.position[1];
            var amount = lembasField.amount;

            var tile = new LembasFieldTile(x, y, amount); //initialize object that is on tile
            tiles[x, y] = tile; //place tile in array
            lembasFieldTiles.Add(tile);
        }

        foreach (var wall in boardConfig.walls)
        {
            tiles[wall[0][0], wall[0][1]].setWall(tiles[wall[1][0], wall[1][1]]);
        }
        
        foreach (var eagle in boardConfig.eagleFields)
        {
            var x = eagle[0];
            var y = eagle[1];

            var tile = new EagleTile(x, y);
            eagleTiles.Add(tile);
            tiles[x, y] = tile;
        }

        var eye_x = boardConfig.eye.position[0];
        var eye_y = boardConfig.eye.position[1];

        var eye_direction = boardConfig.eye.direction;
        eye = new Eye(eye_x, eye_y, eye_direction);

        tiles[eye_x, eye_y] = eye;

        return tiles;
    }

    //gibt Gameboard als String aus - ist nur zu Test und Debug zwecken
    public void PrintGameboard(Tile[,] tiles)
    {
        string gameboardString = "";

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                switch (tiles[j, i].Type)
                {
                    case TileTypes.GRASS:
                        gameboardString += "+";
                        break;
                    case TileTypes.EYE:
                        gameboardString += "E";
                        break;
                    case TileTypes.CHECKPOINT:
                        gameboardString += "C";
                        Console.WriteLine("Checkpoint at [" + j + "," + i + "]");
                        break;
                    case TileTypes.HOLES:
                        gameboardString += "O";
                        break;
                    case TileTypes.RIVER_FIELD:
                        gameboardString += "F";
                        break;
                    case TileTypes.START_FIELD:
                        gameboardString += "X";
                        break;
                    case TileTypes.LEMBAS_FIELD:
                        gameboardString += "L";
                        break;
                }

                gameboardString += " | ";
            }

            gameboardString += "\n";
        }

        Console.Write(gameboardString);
    }

    //startKoordinaten für die Spieler werden zufällig ausgewählt
    public StartField getStartKoordinate()
    {
        //todo: start field dem spieler zuweisen
        Console.WriteLine("mehrfach");
        var index = _random.Next(0, tempStartFields.Count);
        var choosenField = tempStartFields[index];

        tempStartFields.Remove(choosenField);
        return choosenField;
    }

    //helfer methode zu oben
    //schaut ob feld schon belegt ist, falls nicht wird dies zum start feld von Charakter


    public List<Tile> GetNeighbors(Tile tile)
    {
        var neighbors = new List<Tile>();

        if (tile.X != 0 && !tile.hasWall(directionEnum.WEST))
        {
            neighbors.Add(tiles[tile.X - 1, tile.Y]);
        }

        if (tile.X != Width - 1 && !tile.hasWall(directionEnum.EAST))
        {
            neighbors.Add(tiles[tile.X + 1, tile.Y]);
        }

        if (tile.Y != 0 && !tile.hasWall(directionEnum.NORTH))
        {
            neighbors.Add(tiles[tile.X, tile.Y - 1]);
        }

        if (tile.Y != Width - 1 && !tile.hasWall(directionEnum.SOUTH))
        {
            neighbors.Add(tiles[tile.X, tile.Y + 1]);
        }

        //TODO_done: get the neighboring squares, Hint: all nodes are stored in the "Grid" variable 

        return neighbors;
    }

    //schaut ob wand, Ende des Spielfelds oder ein hindernis in richtung der bewegung ist
    //spieler schieben wir im GameManager behandelt
    public bool isWalkable(int x, int y, directionEnum direction)
    {
        if ((y - 1 < 0 && direction.Equals(directionEnum.NORTH))
            || (y + 1 >= Height && direction.Equals(directionEnum.SOUTH))
            || (x - 1 < 0 && direction.Equals(directionEnum.WEST))
            || (x + 1 >= Width && direction.Equals(directionEnum.EAST)))
        {
            return true;
        }

        //scahut ob in der richtung eine Wand ist
        if (tiles[x, y].hasWall(direction))
        {
            Console.WriteLine("Wall");
            return false;
        }

        //schaut ob das Feld der Richtung frei ist oder der Rand erreicht wurde
        switch (direction)
        {
            case directionEnum.NORTH:

                if (tiles[x, y - 1].IsNotWalkable)
                {
                    Console.WriteLine("North occupied or end of board");
                    return false;
                }

                return true;

            case directionEnum.SOUTH:
                if (tiles[x, y + 1].IsNotWalkable)
                {
                    Console.WriteLine("south occupied or end of board");

                    return false;
                }

                return true;

            case directionEnum.EAST:
                if (tiles[x + 1, y].IsNotWalkable)
                {
                    Console.WriteLine("East occupied ({0}) or end of board", tiles[x + 1, y].Type);

                    return false;
                }

                return true;

            case directionEnum.WEST:
                if (tiles[x - 1, y].IsNotWalkable)
                {
                    Console.WriteLine("West occupied ({0}) or end of board", tiles[x - 1, y].Type);

                    return false;
                }

                return true;
        }

        throw new Exception("Internal Error: no fitting direction found in isWalkable");
    }
}