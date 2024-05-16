namespace ConsoleApp1.DataContainers;

public struct CardEvent
{
    public messageEnum message;
    public Data data;

    public CardEvent(Data data)
    {
        this.data = data;
        message = messageEnum.CARD_EVENT;
    }

    public struct Data
    {
        public Data(string playerName, cardEnum card, PlayerState[][] playerStates, BoardState[] boardStates)
        {
            this.playerName = playerName;
            this.card = card;
            this.playerStates = playerStates;
            this.boardStates = boardStates;
        }

        public string playerName;
        public cardEnum card;
        public PlayerState[][] playerStates;
        public BoardState[] boardStates;
    }
}