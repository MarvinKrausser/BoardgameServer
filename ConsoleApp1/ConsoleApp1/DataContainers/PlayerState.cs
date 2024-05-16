namespace ConsoleApp1.DataContainers;

public struct PlayerState
{
    public string playerName;
    public int[] currentPosition;
    public int[] spawnPosition;
    public directionEnum direction;
    public characterEnum character;
    public int lives;
    public int lembasCount;
    public int suspended;
    public int reachedCheckpoints;
    public cardEnum[] playedCards;
    public int turnOrder;

    public PlayerState(string playerName, int[] currentPosition, int[] spawnPosition, directionEnum direction,
        characterEnum character, int lives, int lembasCount, int suspended, int reachedCheckpoints,
        cardEnum[] playedCards, int turnOrder)
    {
        this.playerName = playerName;
        this.currentPosition = currentPosition;
        this.spawnPosition = spawnPosition;
        this.direction = direction;
        this.character = character;
        this.lives = lives;
        this.lembasCount = lembasCount;
        this.suspended = suspended;
        this.reachedCheckpoints = reachedCheckpoints;
        this.playedCards = playedCards;
        this.turnOrder = turnOrder;
    }
}