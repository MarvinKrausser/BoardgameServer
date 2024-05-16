namespace ConsoleApp1.DataContainers;

public struct CharacterChoice
{
    public messageEnum message;
    public Data data;

    public CharacterChoice(Data data)
    {
        this.data = data;
        message = messageEnum.CHARACTER_CHOICE;
    }

    public struct Data
    {
        public characterEnum characterChoice;

        public Data(characterEnum character)
        {
            this.characterChoice = character;
        }
    }
}