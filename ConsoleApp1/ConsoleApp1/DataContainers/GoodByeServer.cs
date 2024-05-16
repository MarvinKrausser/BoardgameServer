namespace ConsoleApp1.DataContainers;

public struct GoodByeServer
{
    public messageEnum message;
    public Data data;

    public GoodByeServer(Data data)
    {
        this.data = data;
        message = messageEnum.GOODBYE_SERVER;
    }

    public struct Data
    {
    }
}