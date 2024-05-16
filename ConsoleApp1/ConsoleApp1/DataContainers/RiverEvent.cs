namespace ConsoleApp1.DataContainers;

public struct RiverEvent
{
    public messageEnum message;
    public Data data;

    public RiverEvent(Data data)
    {
        this.data = data;
        message = messageEnum.RIVER_EVENT;
    }

    public struct Data
    {
        public string playerName;
        public PlayerState[][] playerStates;
        public BoardState[] boardStates;

        public Data(string playerName, PlayerState[][] playerStates, BoardState[] boardStates)
        {
            this.playerName = playerName;
            this.playerStates = playerStates;
            this.boardStates = boardStates;
        }
    }
}