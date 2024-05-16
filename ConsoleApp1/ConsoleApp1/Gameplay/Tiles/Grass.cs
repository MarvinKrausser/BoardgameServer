using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class Grass : Tile
{
    public Grass(int x, int y) : base(x, y, false)
    {
        this.Type = TileTypes.GRASS;
    }
}