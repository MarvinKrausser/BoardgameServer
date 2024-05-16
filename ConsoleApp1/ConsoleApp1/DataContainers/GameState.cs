namespace ConsoleApp1.DataContainers;

public struct GameState
{
    public messageEnum message;
    public Data data;

    public struct Data
    {
        public PlayerState[] playerStates;
        public BoardState boardState;
        public int currentRound;

        public Data(PlayerState[] playerStates, BoardState boardState, int currentRound)
        {
            this.playerStates = playerStates;
            this.boardState = boardState;
            this.currentRound = currentRound;
        }
    }

    public GameState(Data data)
    {
        this.data = data;
        message = messageEnum.GAME_STATE;
    }
}