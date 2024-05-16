namespace ConsoleApp1.DataContainers;

public struct HelloClient
{
    public messageEnum message;
    public Data data;

    public struct Data
    {
        public string reconnectToken;
        public BoardConfig boardConfig;
        public GameConfig gameConfig;

        public Data(string reconnectToken, BoardConfig boardConfig, GameConfig gameConfig)
        {
            this.reconnectToken = reconnectToken;
            this.boardConfig = boardConfig;
            this.gameConfig = gameConfig;
        }
    }

    public HelloClient(Data data)
    {
        this.data = data;
        message = messageEnum.HELLO_CLIENT;
    }
}