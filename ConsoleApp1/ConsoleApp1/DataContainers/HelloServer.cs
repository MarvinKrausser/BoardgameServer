namespace ConsoleApp1.DataContainers;

public struct HelloServer
{
    public messageEnum message;
    public Data data;

    public struct Data
    {
        public roleEnum role;
        public string name;

        public Data(roleEnum role, string name)
        {
            this.role = role;
            this.name = name;
        }
    }

    public HelloServer(Data data)
    {
        this.data = data;
        message = messageEnum.HELLO_SERVER;
    }
}