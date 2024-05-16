using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class Eye : Tile
{
    public directionEnum Direction { get; set; }

    public Eye(int x, int y, directionEnum direction) : base(x, y, true)
    {
        this.Type = TileTypes.EYE;
        this.Direction = direction;
    }
}