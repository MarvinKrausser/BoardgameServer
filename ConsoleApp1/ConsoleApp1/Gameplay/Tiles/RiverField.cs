using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class RiverField : Tile
{
    public directionEnum Direction { get; set; }

    public RiverField(int x, int y, directionEnum direction) : base(x, y, false)
    {
        this.Type = TileTypes.RIVER_FIELD;
        this.Direction = direction;
    }
}