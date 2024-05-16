namespace ConsoleApp1.DataContainers;

public struct CardOffer
{
    public messageEnum message;
    public Data data;

    public CardOffer(Data data)
    {
        this.data = data;
        message = messageEnum.CARD_OFFER;
    }

    public struct Data
    {
        public cardEnum[] cards;

        public Data(cardEnum[] cards)
        {
            this.cards = cards;
        }
    }
}