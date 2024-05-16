namespace ConsoleApp1.DataContainers;

public struct Paused
{
    public messageEnum message;
    public Data data;

    public Paused(Data data)
    {
        this.data = data;
        message = messageEnum.PAUSED;
    }

    public struct Data
    {
        public bool paused;
        public string playerName;

        public Data(bool paused, string playerName)
        {
            this.paused = paused;
            this.playerName = playerName;
        }
    }
}