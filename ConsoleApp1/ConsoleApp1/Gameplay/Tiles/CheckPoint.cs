using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;
/// <summary>
/// Repräsentiert ein Checkpoint 
/// </summary>
public class CheckPoint : Tile
{
    public int order { get; set; }

    public CheckPoint(int x, int y, int order) : base(x, y, false)
    {
        this.Type = TileTypes.CHECKPOINT;
        this.order = order;
    }
}