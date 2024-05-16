namespace ConsoleApp1.DataContainers;

public struct RoundStart
{
    public messageEnum message;
    public Data data;

    public RoundStart(Data data)
    {
        this.data = data;
        message = messageEnum.ROUND_START;
    }

    public struct Data
    {
        public PlayerState[] playerStates;

        public Data(PlayerState[] playerStates)
        {
            this.playerStates = playerStates;
        }
    }
}