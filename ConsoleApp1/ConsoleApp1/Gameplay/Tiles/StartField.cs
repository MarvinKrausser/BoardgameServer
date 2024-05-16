using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class StartField : Tile
{
    public directionEnum Direction { get; set; }

    public StartField(int x, int y, directionEnum direction) : base(x, y, false)
    {
        this.Type = TileTypes.START_FIELD;
        this.Direction = direction;
    }
}