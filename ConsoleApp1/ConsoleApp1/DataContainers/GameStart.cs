namespace ConsoleApp1.DataContainers;

public struct GameStart
{
    public messageEnum message;
    public Data data;

    public struct Data
    {
    }

    public GameStart(Data data)
    {
        this.data = data;
        message = messageEnum.GAME_START;
    }
}