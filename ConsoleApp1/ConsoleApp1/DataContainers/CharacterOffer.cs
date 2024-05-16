namespace ConsoleApp1.DataContainers;

public struct CharacterOffer
{
    public messageEnum message;
    public Data data;

    public CharacterOffer(Data data)
    {
        this.data = data;
        message = messageEnum.CHARACTER_OFFER;
    }

    public struct Data
    {
        public characterEnum[] characters;

        public Data(characterEnum[] characters)
        {
            this.characters = characters;
        }
    }
}