namespace ConsoleApp1.DataContainers;

public struct CardChoice
{
    public messageEnum message;
    public Data data;

    public CardChoice(Data data)
    {
        this.data = data;
        message = messageEnum.CARD_CHOICE;
    }

    public struct Data
    {
        public Data(cardEnum[] cards)
        {
            this.cards = cards;
        }

        public cardEnum[] cards;
    }
}