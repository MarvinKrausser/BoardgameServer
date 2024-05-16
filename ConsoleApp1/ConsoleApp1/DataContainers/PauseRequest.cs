namespace ConsoleApp1.DataContainers;

public struct PauseRequest
{
    public messageEnum message;
    public Data data;

    public PauseRequest(Data data)
    {
        this.data = data;
        message = messageEnum.PAUSE_REQUEST;
    }

    public struct Data
    {
        public bool pause;

        public Data(bool pause)
        {
            this.pause = pause;
        }
    }
}