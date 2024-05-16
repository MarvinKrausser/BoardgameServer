using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public abstract class Tile
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsNotWalkable { get; set; }
    private string PlayerName { get; set; }
    public TileTypes Type { get; set; }

    //für pathfinding
    public int HCost, GCost;
    public int FCost => HCost + GCost;
    public Tile Parent;
    public List<directionEnum> walls { get; set; } //setzt relativ zum feld die wände in der jeweiligen Himmelrichtung

    protected Tile(int x, int y, bool isNotWalkable)
    {
        this.X = x;
        this.Y = y;
        this.IsNotWalkable = isNotWalkable;
        walls = new List<directionEnum>();
    }

    public string getPlayerName()
    {
        if (IsNotWalkable)
        {
            return PlayerName;
        }

        return "";
    }

    public void setWall(Tile wall)
    {
        int wall_x = wall.X;
        int wall_y = wall.Y;

        //ist wand oben/unten/link/rechts - setzt Himmelsrichtung und entgegengesetzte bei übergebenem Tile
        switch (wall_x - X)
        {
            case -1:
                walls.Add(directionEnum.WEST);
                wall.walls.Add(directionEnum.EAST);
                break;
            case 1:
                walls.Add(directionEnum.EAST);
                wall.walls.Add(directionEnum.WEST);
                break;
            default:
                switch (wall_y - Y)
                {
                    case -1:
                        walls.Add(directionEnum.NORTH);
                        wall.walls.Add(directionEnum.WEST);
                        break;
                    case 1:
                        walls.Add(directionEnum.SOUTH);
                        wall.walls.Add(directionEnum.NORTH);
                        break;
                    default:
                        //Tiles sind nich direkt neben einander
                        throw new ArgumentException("Tiles are not next to each other");
                }

                break;
        }
    }

    //wenn richtung in liste == wand => kann nicht durchlaufen werden
    public bool hasWall(directionEnum direction)
    {
        if (walls.Contains(direction))
        {
            return true;
        }

        return false;
    }
}