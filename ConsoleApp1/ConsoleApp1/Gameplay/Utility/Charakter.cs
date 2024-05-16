using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;

namespace ConsoleApp1.Gameplay.Utility;

public class Charakter
{
    public int Lifes { get; set; }
    private int LifesFinal { get; }
    public int Lembas { get; set; }
    private int LembasFinal { get; }
    public int X { get; set; }
    public int Y { get; set; }

    public List<cardEnum> playedCards = new ();

    public StartField startField { get; set; }

    public Cards cards { get; set; }

    //for finding turn order
    public List<Tile> pathToEye;

    //riverEvent 
    public RiverField? lastRiverField { get; set; }
    public int turnOrder { get; set; }
    public directionEnum direction { get; set; }
    public characterEnum Character;
    public bool IsDead { get; set; }
    public int deadRoundsLeft { get; set; }
    public int deadRoundsLeftMax { get; }

    public cardEnum lastPlayedCard;

    public Charakter(StartField startField, int Lifes, int Lembas, characterEnum characterEnum,
        List<CheckPoint> checkPoints, int deadRoundsesLeft)
    {
        //position of charakter
        this.X = startField.X;
        this.Y = startField.Y;
        this.direction = startField.Direction;

        //safe startfield of charakter
        this.startField = startField;

        this.cards = new Cards();

        //andere eigenschaften
        this.Lifes = Lifes;
        this.LifesFinal = Lifes;
        this.Lembas = Lembas;
        this.LembasFinal = Lembas;
        this.Character = characterEnum;
        IsDead = false;
        this.deadRoundsLeftMax = deadRoundsesLeft;

        lastPlayedCard = cardEnum.EMPTY;
    }

    //wird aufgerufen wenn spieler getroffen wird
    public void isHitEvent()
    {
        Lifes -= 1;
        if (Lifes <= 0)
        {
            killCharakter();
        }
    }

    //wird getriggert sobald spieler tot ist und die neue runde beginnt
    public void respawn(StartField respawnPoint)
    {
        IsDead = false;
        this.X = respawnPoint.X;
        this.Y = respawnPoint.Y;
        this.direction = respawnPoint.Direction;
        this.Lifes = this.LifesFinal;
        this.Lembas = this.LembasFinal;
    }

    //wird getriggert wenn spieler ins loch oder vom Spielfeld fällt
    public void killCharakter()
    {
        IsDead = true;
        this.deadRoundsLeft = this.deadRoundsLeftMax;
        Lifes = 0;
        cards.playerDied();
    }
}