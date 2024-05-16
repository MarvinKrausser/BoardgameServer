using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class EagleTile : Tile
{
    public EagleTile(int x, int y) : base(x, y, false)
    {
        this.Type = TileTypes.EAGLE_FIELD;
    }
}