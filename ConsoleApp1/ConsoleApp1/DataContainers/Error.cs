namespace ConsoleApp1.DataContainers;

public struct Error
{
    public messageEnum message;
    public Data data;

    public Error(Data data)
    {
        this.data = data;
        message = messageEnum.ERROR;
    }

    public struct Data
    {
        public string reason;
        public int errorCode;

        public Data(string reason, int errorCode)
        {
            this.reason = reason;
            this.errorCode = errorCode;
        }
    }
}