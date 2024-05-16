namespace ConsoleApp1.DataContainers;

public struct GameEnd
{
    public messageEnum message;
    public Data data;

    public GameEnd(Data data)
    {
        this.data = data;
        message = messageEnum.GAME_END;
        this.data.additional = Array.Empty<Data.Additional>();
    }

    public struct Data
    {
        public PlayerState[] playerStates;
        public string winner;
        public Additional[] additional; //Additional

        public Data(PlayerState[] playerStates, string winner)
        {
            this.playerStates = playerStates;
            this.winner = winner;
        }

        public struct Additional
        {
            public string name;
            public string value;
            public string description;

            public Additional(string name, string value, string description)
            {
                this.name = name;
                this.value = value;
                this.description = description;
            }
        }
    }
}