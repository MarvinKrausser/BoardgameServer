namespace ConsoleApp1.DataContainers;

public struct BoardState
{
    public LembasField[] lembasFields;

    public BoardState(LembasField[] lembasFields)
    {
        this.lembasFields = lembasFields;
    }
}