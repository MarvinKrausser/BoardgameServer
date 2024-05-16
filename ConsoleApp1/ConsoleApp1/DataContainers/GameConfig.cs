namespace ConsoleApp1.DataContainers;

public struct GameConfig
{
    public int startLembas;
    public int shotLembas;
    public int cardSelectionTimeout;
    public int characterChoiceTimeout;
    public int riverMoveCount;
    public int serverIngameDelay;
    public int reviveRounds;
    public int maxRounds;

    public GameConfig(int startLembas, int shotLembas, int cardSelectionTimeout, int characterChoiceTimeout,
        int riverMoveCount, int serverIngameDelay, int reviveRounds, int maxRounds)
    {
        this.startLembas = startLembas;
        this.shotLembas = shotLembas;
        this.cardSelectionTimeout = cardSelectionTimeout;
        this.characterChoiceTimeout = characterChoiceTimeout;
        this.riverMoveCount = riverMoveCount;
        this.serverIngameDelay = serverIngameDelay;
        this.reviveRounds = reviveRounds;
        this.maxRounds = maxRounds;
    }
}