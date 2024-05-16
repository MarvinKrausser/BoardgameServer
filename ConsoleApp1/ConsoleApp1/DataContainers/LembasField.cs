namespace ConsoleApp1.DataContainers;

public struct LembasField
{
    public int[] position;
    public int amount;

    public LembasField(int[] position, int amount)
    {
        this.position = position;
        this.amount = amount;
    }
}