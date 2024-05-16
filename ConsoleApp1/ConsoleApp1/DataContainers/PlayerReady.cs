namespace ConsoleApp1.DataContainers;

public struct PlayerReady
{
    public messageEnum message;
    public Data data;

    public PlayerReady(Data data)
    {
        this.data = data;
        message = messageEnum.PLAYER_READY;
    }

    public struct Data
    {
        public bool ready;

        public Data(bool ready)
        {
            this.ready = ready;
        }
    }
}