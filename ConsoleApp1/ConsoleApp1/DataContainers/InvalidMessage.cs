namespace ConsoleApp1.DataContainers;

public struct InvalidMessage
{
    public messageEnum message;
    public Data data;

    public struct Data
    {
        public string invalidMessage;

        public Data(string invalidMessage)
        {
            this.invalidMessage = invalidMessage;
        }
    }

    public InvalidMessage(Data data)
    {
        this.data = data;
        message = messageEnum.INVALID_MESSAGE;
    }
}