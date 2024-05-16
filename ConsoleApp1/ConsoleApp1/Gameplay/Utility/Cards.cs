using ConsoleApp1.DataContainers;

namespace ConsoleApp1.Gameplay.Utility;

public class Cards
{
    private List<cardEnum> _nachziehstapel; // initial: 20 cards
    private List<cardEnum> _handkarten; // max 9 cards
    private List<cardEnum> _ablagestapel;

    public Cards()
    {
        _nachziehstapel = new List<cardEnum>();

        setInitialNachziehstapelCards();

        _handkarten = new List<cardEnum>();
        _ablagestapel = new List<cardEnum>();
    }

    /// <summary>
    /// setting or resetting 20 Cards into the deck "Nachziehstapel"
    /// </summary>
    private void setInitialNachziehstapelCards()
    {
        _nachziehstapel.Add(cardEnum.MOVE_3); //adding 1 "Move 3" card
        for (int i = 0; i < 3; i++)
        {
            _nachziehstapel.Add(cardEnum.MOVE_2);
        } //adding 3 "Move 2" cards

        for (int i = 0; i < 5; i++)
        {
            _nachziehstapel.Add(cardEnum.MOVE_1);
        } //adding 5 "Move 1" cards

        _nachziehstapel.Add(cardEnum.MOVE_BACK); //adding 1 "Move Back" card
        _nachziehstapel.Add(cardEnum.U_TURN); //adding 1 "U-Turn" card
        for (int i = 0; i < 3; i++)
        {
            _nachziehstapel.Add(cardEnum.RIGHT_TURN);
        } //adding 3 "Right Turn" cards

        for (int i = 0; i < 3; i++)
        {
            _nachziehstapel.Add(cardEnum.LEFT_TURN);
        } //adding 3 "Left Turn" cards

        for (int i = 0; i < 2; i++)
        {
            _nachziehstapel.Add(cardEnum.AGAIN);
        } //adding 2 "Again" cards

        _nachziehstapel.Add(cardEnum.LEMBAS); //adding 1 "Lembas" card

        shuffleDeck(_nachziehstapel);
    }

    /// <summary>
    /// shuffle a stack
    /// <param name="stack"></param>
    /// </summary>
    private void shuffleDeck(List<cardEnum> stack)
    {
        Random rng = new Random();

        int n = stack.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); //Next(MaxValue): returns a not negativ random integer, that is lower than MaxValue
            cardEnum switchVariable = stack[k];
            stack[k] = stack[n];
            stack[n] = switchVariable;
        }
    }

    /// <summary>
    /// The function returns the randomly chosen hand cards ("_handkarten").
    /// if you want the cards, you need this function needs the lifes of the player.
    /// </summary>
    /// <param name="lifes"></param>
    /// <returns></returns>
    public List<cardEnum> createAndGetNewHandkarten(int lifes)
    {
        _handkarten.Clear(); //reset 

        createRandomHandkarten(lifes); //creates new cards as "Handkarten"

        return _handkarten; //returns 
    }

    /// <summary>
    /// chooses randomly 7-9 cards and removes them from the deck "_nachziehstapel".
    /// <param name="lifes"></param>
    /// </summary>
    private void createRandomHandkarten(int lifes)
    {
        int playerLifes = lifes; //max 3 min 0 

        if (playerLifes > 0 && playerLifes < 4)
        {
            int numberOfCards;

            switch (playerLifes)
            {
                case 1:
                    numberOfCards = 7;
                    break;
                case 2:
                    numberOfCards = 8;
                    break;
                case 3:
                    numberOfCards = 9;
                    break;
                default:
                    numberOfCards = 0; //this should never happen. 
                    break;
            }

            for (int i = 0; i < numberOfCards; i++) //selects 7 or 8 or 9 random cards as "_handkarten"
            {
                if (_nachziehstapel.Count <= 0)
                {
                    shuffleDeck(_ablagestapel); //first: shuffle deck "_ablagestapel"
                    fromAblagestapelToNachziehstapel(); //then: put Cards from "_ablagestapel" to "_nachziehstapel"
                }

                _handkarten.Add(_nachziehstapel[0]);
                _nachziehstapel.RemoveAt(0); //RemoveAt(): removes an element from a list at a position/index = 0
            }
        }
    }

    private void fromAblagestapelToNachziehstapel()
    {
        for (int i = 0; i < _ablagestapel.Count; i++)
        {
            _nachziehstapel.Add(_ablagestapel[i]);
        }

        _ablagestapel = new List<cardEnum>();
    }


    /// <summary>
    /// 1) should be called after a CARD_CHOICE Message with "cards" as the cards that have been logged in
    /// 2) the cards that have not been logged in are placed on the _ablagestapel
    /// 3) checks if the player logged none or not all five cards in. if there are less than 5 cards, this method adds the rest
    /// </summary>
    /// <param name="cards"></param>
    public bool playersCardChoice(List<cardEnum> cards)
    {
        bool returnValue = true;

        List<cardEnum> loggedInCards = new List<cardEnum>();
        List<cardEnum> notLoggedInCards = new List<cardEnum>();

        for (int i = 0; i < _handkarten.Count; i++) //initial: all "_handkarten" are not logged in
        {
            notLoggedInCards.Add(_handkarten[i]);
        }

        for (int i = 0; i < 5; i++) //It should be: cards.Count == 5. If not, then the rest will be ignored
        {
            if (cards.Count < 5) //this case should never happen if the client sends the message CARD_CHOICE correctly
            {
                cards.Add(cardEnum.EMPTY);
            }

            if (notLoggedInCards.Contains(cards[i])) //Test which cards are logged in. if case: card is logged in
            {
                loggedInCards.Add(cards[i]);
                notLoggedInCards.Remove(cards[i]);
            }
            else
            {
                if (!cards[i].Equals(cardEnum.EMPTY))
                {
                    returnValue = false;
                }

                loggedInCards.Add(cardEnum.EMPTY); //This will be replaced later with a random card
            }
        }

        addCardsToAblagestapel(notLoggedInCards);
        notLoggedInCards.Clear();


        int counter = 0;
        //Test if loggedInCards == 5. If not, the Server has to add a few Cards
        while (counter < 5)
        {
            if (_nachziehstapel.Count <= 0 && loggedInCards[counter].Equals(cardEnum.EMPTY))
            {
                loggedInCards.RemoveAll(x =>
                    x.Equals(cardEnum.EMPTY)); //remove all EMPTY cards, so we can put the cards into the Ablagestapel
                addCardsToAblagestapel(
                    loggedInCards); //this is required - See "Lastenheft" page 19: 2.12.2: "Wenn der Nachziehstapel hier nicht genug Karten hat, werden die Ablagestapelkarten samt aller aktuellen Handkarten gemischt und zum neuen Nachziehstapel. Nun werden so viele Karten gezogen und verdeckt eingeloggt, bis der Spielende f�nf Karten eingeloggt hat."
                loggedInCards = new List<cardEnum>();
                for (int i = 0; i < 5; i++) //add 5 EMPTY cards, which will be replaced later 
                {
                    loggedInCards.Add(cardEnum.EMPTY);
                }

                shuffleDeck(_ablagestapel); //first: shuffle deck "_ablagestapel"
                fromAblagestapelToNachziehstapel(); //then: put Cards from "_ablagestapel" to "_nachziehstapel"
                counter = 0;
            }

            if (loggedInCards[counter]
                .Equals(cardEnum.EMPTY)) //If card == EMPTY, than replace it with a card from deck "_nachziehstapel"
            {
                loggedInCards[counter] = _nachziehstapel[0];
                _nachziehstapel.RemoveAt(0); //RemoveAt(): removes an element from a list at a position/index = 0
            }

            counter++;
        }

        _handkarten = loggedInCards;
        return returnValue;
    }

    /// <summary>
    /// This function adds the cards "cards" to the deck "_ablagestapel"
    /// </summary>
    /// <param name="cards"></param>
    private void addCardsToAblagestapel(List<cardEnum> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            _ablagestapel.Add(cards[i]);
        }
    }

    /// <summary>
    /// This function can be called outside of this class.
    /// Should be called, in case:
    /// After every Round, when Handkarten were played. It puts the cards "_handkarten" on the stack "_ablagestapel"
    /// </summary>
    public void roundOver()
    {
        addCardsToAblagestapel(_handkarten);
    }

    /// <summary>
    /// This function can be called outside of this class. It should be called, when a player died.
    /// Puts _handkarten on _ablagestapel and Clears the handkarten
    /// </summary>
    /// <returns></returns>
    public List<cardEnum> playerDied()
    {
        addCardsToAblagestapel(_handkarten);
        _handkarten.Clear();
        fromAblagestapelToNachziehstapel(); //first: put Cards from "_ablagestapel" to "_nachziehstapel"
        shuffleDeck(_nachziehstapel); //then: shuffle deck "_nachziehstapel"

        return _handkarten;
    }


    //The following 3 methods are getter methods
    public List<cardEnum> getHandkarten()
    {
        return _handkarten;
    }

    public List<cardEnum> getAblagestapel()
    {
        return _ablagestapel;
    }

    public List<cardEnum> getNachziehstapel()
    {
        return _nachziehstapel;
    }
}