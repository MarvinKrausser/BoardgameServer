using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Utility;
using NUnit.Framework;

namespace ConsoleApp1.Tests.GameplayTests;

[TestFixture]
public class CardsTest
{
    private List<cardEnum> nachziehstapel;
    private List<cardEnum> ablagestapel;
    private List<cardEnum> handkarten;

    [Test]
    public void CreateCards()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write(CreateCardsOutputString());
    }

    [Test]
    public void CreateAndGetNewHandkartenTest()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();


        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("lifes: 3 \r\n" + CreateCardsOutputString());
        /*
        cards.createAndGetNewHandkarten(2);
        Console.Write("lifes: 2 \r\n" + CreateCardsOutputString());*/
        /*
        cards.createAndGetNewHandkarten(1);
        Console.Write("lifes: 1 \r\n" + CreateCardsOutputString());*/
        /*
        cards.createAndGetNewHandkarten(0);
        Console.Write("lifes: 0 \r\n" + CreateCardsOutputString());*/
    }

    [Test]
    public void PlayersCardChoiceWith5CardsLoggedInTest()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer with 3 lifes  \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[6]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[7]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card Choice \r\n" + CreateCardsOutputString());
    }

    [Test]
    public void PlayersCardChoiceWith4OrLessCardsLoggedInTest()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer with 3 lifes \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[7]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card Choice \r\n" + CreateCardsOutputString());
    }

    [Test]
    public void NachziehstapelHas4OrLessCards()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);
        handkarten = cards.getHandkarten();
        cards.roundOver();
        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        //there should be 2 cards in the nachziehstapel
        Console.Write("Card offer with 3 lifes \r\n" + CreateCardsOutputString());
        Console.WriteLine();
        cards.roundOver();
        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        //there should be 11 cards in the nachziehstapel
        Console.Write("Card offer with 3 lifes \r\n" + CreateCardsOutputString());
        Console.WriteLine();


        /*List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[6]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[7]);
        playerCardChoice.Add(cardEnum.EMPTY);

        cards.playersCardChoice(playerCardChoice);
        
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        
        Console.Write("Card Choice \r\n" + CreateCardsOutputString());*/
    }

    [Test]
    public void PlayersCardChoiceWith4OrLessCardsAndNachziehstapelHasTooFewCards()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);
        handkarten = cards.getHandkarten();
        cards.roundOver();
        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        //there should be 2 cards in the nachziehstapel
        Console.Write("Card offer with 3 lifes \r\n" + CreateCardsOutputString());
        Console.WriteLine();


        List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(cardEnum.EMPTY);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card Choice \r\n" + CreateCardsOutputString());
    }

    [Test]
    public void PlayersCardChoiceWith4OrLessCardsAndNachziehstapelHasTooFewCards2()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        //there should be 2 cards in the nachziehstapel
        Console.Write("Card offer with 3 lifes: 1 \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        List<cardEnum> playerCardChoice1 = new List<cardEnum>();
        playerCardChoice1.Add(handkarten[0]);
        playerCardChoice1.Add(handkarten[1]);
        playerCardChoice1.Add(cardEnum.EMPTY);
        playerCardChoice1.Add(cardEnum.EMPTY);
        playerCardChoice1.Add(handkarten[2]);

        cards.playersCardChoice(playerCardChoice1);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card Choice: 1 \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.roundOver();
        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();
        //there should be 2 cards in the nachziehstapel
        Console.Write("Card offer with 3 lifes: 2 \r\n" + CreateCardsOutputString());
        Console.WriteLine();


        List<cardEnum> playerCardChoice2 = new List<cardEnum>();
        playerCardChoice2.Add(cardEnum.EMPTY);
        playerCardChoice2.Add(cardEnum.EMPTY);
        playerCardChoice2.Add(cardEnum.EMPTY);
        playerCardChoice2.Add(cardEnum.EMPTY);
        playerCardChoice2.Add(cardEnum.EMPTY);

        cards.playersCardChoice(playerCardChoice2);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card Choice: 2 \r\n" + CreateCardsOutputString());
        Console.WriteLine();
    }

    [Test]
    public void PlayerDiedTest()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer with 3 lifes: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[1]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[4]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        //Now Test: What happens, when the player dies
        cards.playerDied();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Player died \r\n" + CreateCardsOutputString());
        Console.WriteLine();
    }

    [Test]
    public void CreateAndGetNewHandkartenTestWith0Lifes()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("initial: \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(0);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("lifes: 0 \r\n" + CreateCardsOutputString());
    }

    [Test]
    public void Played5RoundsTest()
    {
        Cards cards = new Cards();

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write(CreateCardsOutputString());
        Console.WriteLine();

        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer 1 Rounds: There sould be 11 cards in deck nachziehstapel \r\n" +
                      CreateCardsOutputString());
        Console.WriteLine();

        List<cardEnum> playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[1]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[4]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.roundOver();
        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer 2 Rounds: There sould be 2 cards in deck nachziehstapel \r\n" +
                      CreateCardsOutputString());
        Console.WriteLine();

        playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[1]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[4]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.roundOver();
        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer 3 Rounds: There sould be 11 cards in deck nachziehstapel \r\n" +
                      CreateCardsOutputString());
        Console.WriteLine();

        playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[1]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[4]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.roundOver();
        cards.createAndGetNewHandkarten(3);
        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer 4 Rounds: There sould be 2 cards in deck nachziehstapel \r\n" +
                      CreateCardsOutputString());
        Console.WriteLine();

        playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(cardEnum.EMPTY);
        playerCardChoice.Add(cardEnum.EMPTY);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();

        cards.roundOver();
        cards.createAndGetNewHandkarten(3);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card offer 5 Rounds: There sould be 11 cards in deck nachziehstapel \r\n" +
                      CreateCardsOutputString());
        Console.WriteLine();

        playerCardChoice = new List<cardEnum>();
        playerCardChoice.Add(handkarten[0]);
        playerCardChoice.Add(handkarten[1]);
        playerCardChoice.Add(handkarten[2]);
        playerCardChoice.Add(handkarten[3]);
        playerCardChoice.Add(handkarten[4]);

        cards.playersCardChoice(playerCardChoice);

        nachziehstapel = cards.getNachziehstapel();
        ablagestapel = cards.getAblagestapel();
        handkarten = cards.getHandkarten();

        Console.Write("Card choice \r\n" + CreateCardsOutputString());
        Console.WriteLine();
    }

    public string CreateCardsOutputString()
    {
        string cardsOutput = "nachziehstapel: ";

        for (int i = 0; i < nachziehstapel.Count; i++)
        {
            cardsOutput += nachziehstapel[i] + " | ";
        }

        cardsOutput += "\r\n";
        cardsOutput += "ablagestapel: ";

        for (int i = 0; i < ablagestapel.Count; i++)
        {
            cardsOutput += ablagestapel[i] + " | ";
        }

        cardsOutput += "\r\n";
        cardsOutput += "handkarten: ";
        for (int i = 0; i < handkarten.Count; i++)
        {
            cardsOutput += handkarten[i] + " | ";
        }

        cardsOutput += "\r\n";

        return cardsOutput;
    }
}