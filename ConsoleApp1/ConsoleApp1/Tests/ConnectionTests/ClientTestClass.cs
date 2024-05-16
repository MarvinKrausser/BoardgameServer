using ConsoleApp1.DataContainers;

namespace ConsoleApp1.Tests.ConnectionTests;

public class ClientTestClass
{
    public string name;
    public string reconnectToken;
    public characterEnum character;

    public ClientTestClass(string name, string reconnectToken)
    {
        this.name = name;
        this.reconnectToken = reconnectToken;
    }
}