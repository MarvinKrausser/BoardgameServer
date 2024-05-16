using ConsoleApp1.Gameplay.Enums;

namespace ConsoleApp1.Gameplay.Tiles;

public class LembasFieldTile : Tile
{
    public int amount { get; set; }

    public LembasFieldTile(int x, int y, int amount) : base(x, y, false)
    {
        this.Type = TileTypes.LEMBAS_FIELD;
        this.amount = amount;
    }
}