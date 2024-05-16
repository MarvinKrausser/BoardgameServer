namespace ConsoleApp1.DataContainers;

public struct ParticipantsInfo
{
    public messageEnum message;

    public ParticipantsInfo(Data data)
    {
        this.data = data;
        message = messageEnum.PARTICIPANTS_INFO;
    }

    public Data data;

    public struct Data
    {
        public string[] players;
        public string[] spectators;
        public string[] ais;
        public string[] readyPlayers;

        public Data(string[] players, string[] spectators, string[] ais, string[] readyPlayers)
        {
            this.players = players;
            this.spectators = spectators;
            this.ais = ais;
            this.readyPlayers = readyPlayers;
        }
    }
}