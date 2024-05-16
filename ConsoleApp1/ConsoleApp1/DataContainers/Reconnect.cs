namespace ConsoleApp1.DataContainers;

public struct Reconnect
{
    public messageEnum message;
    public Data data;

    public Reconnect(Data data)
    {
        this.data = data;
        message = messageEnum.RECONNECT;
    }

    public struct Data
    {
        public string name;
        public string reconnectToken;

        public Data(string name, string reconnectToken)
        {
            this.name = name;
            this.reconnectToken = reconnectToken;
        }
    }
}