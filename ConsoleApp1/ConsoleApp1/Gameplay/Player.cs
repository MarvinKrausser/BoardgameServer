using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Gameplay.Utility;

namespace ConsoleApp1.Gameplay;

public class Player
{
    public bool isReady { get; set; }
    public string Name { get; set; }
    public int ReachedCheckpoints { get; set; }
    public roleEnum Role { get; set; }
    private int suspended { get; set; }
    public List<cardEnum> playedCards;
    private List<cardEnum> aviableCards;

    public bool hasReceivedError = false;

    public bool PlayerConnected { get; set; }
    public bool IsPlayerBanned { get; set; }

    public Charakter Charakter { get; set; }

    public Player(string name, roleEnum role)
    {
        this.PlayerConnected = true;
        this.Name = name;
        this.Role = role;
        ReachedCheckpoints = 0;
        isReady = false;
        playedCards = new List<cardEnum>();
        aviableCards = new List<cardEnum>();
    }

    //wird von GameManager aufgerufen um charakter zu instanziieren
    public void InitializeCharakter(StartField startField, int lifes, int lembas, characterEnum charakterEnum,
        int deadRoundsLeft, List<CheckPoint> leftCheckPoints)
    {
        //TODO: initialisierung des Charakters wenn enum gesetzt

        this.Charakter = new Charakter(startField, lifes, lembas, charakterEnum, leftCheckPoints, deadRoundsLeft);
    }
}