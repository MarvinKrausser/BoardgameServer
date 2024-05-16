namespace ConsoleApp1.DataContainers;

public struct EagleEvent
{
    public messageEnum message;
    public Data data;

    public EagleEvent(Data data)
    {
        message = messageEnum.EAGLE_EVENT;
        this.data = data;
    }

    public struct Data
    {
        public string playerName;
        public PlayerState[] playerStates;

        public Data(string playerName, PlayerState[] playerStates)
        {
            this.playerName = playerName;
            this.playerStates = playerStates;
        }
    }
}