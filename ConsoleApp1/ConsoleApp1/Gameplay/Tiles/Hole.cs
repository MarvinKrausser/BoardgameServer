using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class Hole : Tile
{
    public Hole(int x, int y) : base(x, y, false)
    {
        this.Type = TileTypes.HOLES;
    }
}