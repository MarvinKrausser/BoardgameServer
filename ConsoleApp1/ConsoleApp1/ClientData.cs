using ConsoleApp1.DataContainers;

namespace ConsoleApp1;

public class ClientData
{
    //Testing

    public string clientID;
    public readonly string reconnectToken;
    public readonly string name;
    public bool isReady = false;
    public bool hasChosen = false;
    public bool isConnected = true;
    public readonly roleEnum role;

    public characterEnum[] characterOffer;
    public characterEnum chosenCharacter;

    public ClientData(string clientID, string reconnectToken, string name, roleEnum role)
    {
        this.clientID = clientID;
        this.reconnectToken = reconnectToken;
        this.name = name;
        this.role = role;
    }
}