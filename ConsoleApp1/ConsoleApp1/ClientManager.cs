using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using ConsoleApp1.Gameplay.Utility;
using NUnit.Framework;

namespace ConsoleApp1;

public delegate void NewClientEventHandler(string reconnectToken, string name, roleEnum role, EventArgs e);

public delegate void ClientDisconnected2EventHandler(string reconnectToken, string name, EventArgs e);

public delegate void ClientBannedEventHandler(string reconnectToken, string name, bool hasStarted, EventArgs e);

public delegate void CharacterChoiceEventHandler(string reconnectToken, string name, characterEnum character,
    EventArgs e);

public delegate void PauseEventHandler(string reconnectToken, string name, bool isPaused, EventArgs e);

public delegate void ReconnectEventHandler(string reconnectToken, string name, EventArgs e);

public delegate List<Player> GetPlayers();

public class ClientManager
{
    public event NewClientEventHandler NewClient;

    public event ClientDisconnected2EventHandler
        ClientDisconnected2; //Wird auch bei bann aufgerufen  kann auch ban aufrufen

    public event CharacterChoiceEventHandler CharacterChoice;
    public event PauseEventHandler Pause;
    public event ClientBannedEventHandler ClientBanned;
    public event ReconnectEventHandler ReconnectEvent;

    //Semaphoren in die die Nachichten abgelegt werden falls neue ankommen
    public readonly Queue<(string, string)> messages = new(); //Neue Nachrichten
    public readonly UserLevelSemaphore messageSemaphore = new(0); //Für neue Nachrichten
    public readonly AutoResetEvent readySemaphore = new(false); //Für die Lobby

    
    public readonly HashSet<ClientData> _clientsSet = new();
    private readonly Server _server = new();
    private int _maxPlayer;

    private readonly HashSet<characterEnum> _charactersAvailable =
        new HashSet<characterEnum>(Enum.GetValues(typeof(characterEnum)).Cast<characterEnum>());

    private readonly Random _random = new();
    private GameConfig _gameConfig;
    private BoardConfig _boardConfig;
    private bool _isPaused;
    private readonly AutoResetEvent _allHaveChosen = new(false);
    private BoardState _boardState;
    private readonly UserLevelSemaphore _semNewMessage = new(0);

    //mode 0: item 1 = message, item2 = clientID  NewMessage
    //mode 1: item1 = ClientID, item2 = message  BanClient
    private readonly Queue<(string, string, int)> _newTaskQueue = new();

    private readonly Semaphore _synchronize = new(1, 1);
    private bool _working = true;
    private int _characterChoiceTimeout;
    private bool _receivingReady = true; //Ends with characteroffer
    private bool _gameStarted; //starts with real gamestart
    private AutoResetEvent _waitTillEnd = new(false);
    private HashSet<string> _oldReconnectToken = new();
    private string _playerWhoPaused = String.Empty;
    private GetPlayers _getPlayers;

    public void Setup(int port, BoardConfig boardConfig, GameConfig gameConfig, GetPlayers getPlayers,
        in BoardState boardStates)
    {
        //Setzt den Server auf 
        _server.SetupServer(port);
        // subscribed die jeweiligen Events
        _server.ClientDisconnected += ClientDisconnected;
        _server.NewMessage += GetMessage;
        _server.ClientConnected += ClientConnected;
        //nötigen variablen werden gesetzt
        _gameConfig = gameConfig;
        _boardConfig = boardConfig;
        _maxPlayer = boardConfig.startFields.Length;
        _boardState = boardStates;
        _getPlayers = getPlayers;
        _characterChoiceTimeout = gameConfig.characterChoiceTimeout;

        Task.Run(ProcessInput);
    }

    private void ClientConnected(string clientID, EventArgs e)
    {
        Console.WriteLine("Client has connected");
    }

    public void EndGame()
    {
        _working = false;
        _semNewMessage.Release();
        _waitTillEnd.WaitOne();
    }
    
    /// <summary>
    /// Gets Messages by Server and queues it
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    /// <param name="e"></param>

    private void GetMessage(string message, string clientID, EventArgs e)
    {   //erhält die Nachriten der Clients
        _synchronize.WaitOne();
        _newTaskQueue.Enqueue((message, clientID, 0));
        _semNewMessage.Release();
        _synchronize.Release();
    }

    
    /// <summary>
    /// Verarbeitet die Eingabe, überprüft diese und bannt den Client oder verarbeitet die Nachricht weiter 
    /// </summary>
    /// <returns></returns>
    private Task ProcessInput()
    {
        while (true)
        {
            _semNewMessage.WaitOne();

            if (!_working) break;

            _synchronize.WaitOne();
            var data = _newTaskQueue.Dequeue();
            _synchronize.Release();

            //entscheidet ob gebannt oder verarbeitet wird
            switch (data.Item3)
            {
                case 0: //New Message
                    ProcessMessage(data.Item1, data.Item2);
                    break;
                case 1: //Ban Client
                    ClientData? c = _clientsSet.FirstOrDefault(p => p.name.Equals(data.Item1));
                    if (c is null) continue;
                    if (data.Item2.Equals("")) BanClient(c);
                    else BanClientWithMessage(c, data.Item2);
                    break;
            }
        }

        _server.Close();
        _waitTillEnd.Set();
        return Task.CompletedTask;
    }

    
    /// <summary>
    /// Verarbeitet die empfangenen Nachrichten und löst die dazugehörigen Aktionen aus.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    private void ProcessMessage(string message, string clientID)
    {
        //Ist die Nachricht valide
        Console.WriteLine("Server: " + message);
        messageEnum? type = JsonManager.GetTypeJson(message);
        if (!JsonManager.IsValid(message, type))
        {
            Invalid(message, clientID);
            return;
        }
        //verarbeitung je nach Art der Nachricht
        switch (type)
        {
            case messageEnum.HELLO_SERVER:
                Connect(clientID, JsonManager.DeserializeJson<HelloServer>(message));
                break;
            case messageEnum.RECONNECT:
                Reconnect(clientID, JsonManager.DeserializeJson<Reconnect>(message));
                break;
            case messageEnum.GOODBYE_SERVER:
                BanClientByID(clientID);
                break;
            case messageEnum.PLAYER_READY:
                ReadyReceived(JsonManager.DeserializeJson<PlayerReady>(message), clientID);
                break;
            case messageEnum.CHARACTER_CHOICE:
                CharacterChoiceReceived(JsonManager.DeserializeJson<CharacterChoice>(message), clientID);
                break;
            case messageEnum.PAUSE_REQUEST:
                PausedReceived(JsonManager.DeserializeJson<PauseRequest>(message), clientID);
                break;
            case messageEnum.CARD_CHOICE:
                if (!_gameStarted)
                {
                    //wenn spiel noch nicht läuft darf diese Nachricht nicht gesendet werden
                    Error error = new Error(new Error.Data("Spiel hat noch nicht angefangen", 0));
                    _server.SendData(JsonManager.ConvertToJason(error), new[] { clientID });
                    break;
                }

                if (CheckStatePlayerKi(clientID, true)) return;
                ClientData? c = _clientsSet.Where(p => p.role.Equals(roleEnum.AI) || p.role.Equals(roleEnum.PLAYER))
                    .FirstOrDefault(p => p.clientID.Equals(clientID));
                if (c is null) return;
                messages.Enqueue((message, c.name));
                messageSemaphore.Release();
                break;
            default:
                Invalid(message, clientID);
                break;
        }
    }

    
    /// <summary>
    /// Bekommt die Pause-Nachricht und verarbeitet diese. Sollten alle nötigen Bedinungen erfüllt sein, so wird das Spiel pausiert
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    private void PausedReceived(PauseRequest message, string clientID)
    {
        if (CheckStatePlayer(clientID, true)) return;

        //Spiel muss gestartet sein um Pause auslösen zu können
        if (!_gameStarted)
        {
            Error error = new Error(new Error.Data("Spiel hat noch nicht angefangen", 0));
            _server.SendData(JsonManager.ConvertToJason(error), new[] { clientID });
        }

        Console.WriteLine(message.data.pause);
        Console.WriteLine(_isPaused);
        //setzt Pause oder löst diese je nach aktuellem stand
        if (_isPaused != message.data.pause)
        {
            _isPaused = !_isPaused;
        }
        else
        {
            Console.WriteLine("Fehler");
            return;
        }
        //überprüft den Client
        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));
        if (c is null)
        {
            Console.WriteLine("Client not existing");
            return;
        }
        //initialisiert die Pause 
        Pause(c.reconnectToken, c.name, _isPaused, EventArgs.Empty);
        _playerWhoPaused = c.name;


        Paused paused = new Paused(new Paused.Data(_isPaused, c.name));
        _server.SendData(JsonManager.ConvertToJason(paused),
            _clientsSet.Where(p => p.isConnected).Select(p => p.clientID).ToArray());
    }

    
    /// <summary>
    /// Überprüft ob ein Client eine KI ist oder nicht. 
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private bool CheckStatePlayerKi(string clientID, bool isPlayer)
    {
        //Client ist ein Spieler
        if (_clientsSet.Where(p => IsPlayerOrAi(p))
                .Any(p => p.clientID.Equals(clientID)) && !isPlayer)
        {
            Error error = new Error(new Error.Data("Du bist bereits Spieler", 0));
            BanClientByIDWithMessage(clientID, JsonManager.ConvertToJason(error));
            return true;
        }

        //Client ist kein Spieler
        if (!_clientsSet.Where(p => IsPlayerOrAi(p))
                .Any(p => p.clientID.Equals(clientID)) && isPlayer)
        {
            Error error = new Error(new Error.Data("Du bist kein Spieler", 0));
            BanClientByIDWithMessage(clientID, JsonManager.ConvertToJason(error));
            return true;
        }

        return false;
    }

    
    /// <summary>
    /// Überprüft ob ein Client eine KI ist oder nicht.
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    private bool CheckStatePlayer(string clientID, bool isPlayer)
    {
        //Client ist bereit ein Spieler
        if (_clientsSet.Where(p => p.role.Equals(roleEnum.PLAYER)).Any(p => p.clientID.Equals(clientID)) && !isPlayer)
        {
            Error error = new Error(new Error.Data("Du bist bereits Spieler", 0));
            BanClientByIDWithMessage(clientID, JsonManager.ConvertToJason(error));
            return true;
        }
        //Client ist kein Spieler
        if (!_clientsSet.Where(p => p.role.Equals(roleEnum.PLAYER)).Any(p => p.clientID.Equals(clientID)) && isPlayer)
        {
            Error error = new Error(new Error.Data("Du bist kein Spieler", 0));
            BanClientByIDWithMessage(clientID, JsonManager.ConvertToJason(error));
            return true;
        }

        return false;
    }

    
    /// <summary>
    /// Client wird gebannt, sobald eine invalide Nachricht geschickt wird
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    private void Invalid(string message, string clientID)
    {
        InvalidMessage inv = new InvalidMessage(new InvalidMessage.Data(message));
        BanClientByIDWithMessage(clientID, JsonManager.ConvertToJason(inv));
    }

    private void Invalid(string message, ClientData client)
    {
        InvalidMessage inv = new InvalidMessage(new InvalidMessage.Data(message));
        BanClientWithMessage(client, JsonManager.ConvertToJason(inv));
    }

    
    /// <summary>
    /// Baut die Verbindung zu einem Client auf und checkt den Status des Spiels
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="message"></param>
    private void Connect(string clientID, HelloServer message)
    
    {   //checkt ob sich der Client überhaut verbinden darf
        if (CheckStatePlayerKi(clientID, false)) return;

        if (!_receivingReady && !message.data.role.Equals(roleEnum.SPECTATOR))
        {
            Error error = new Error(new Error.Data("Spiel hat bereits angefangen", 0));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
        }
        else if (message.data.name.Length > 20)
        {
            Error error = new Error(new Error.Data("Name ist zu lang", 3));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
        }
        else if (_clientsSet.Any(p => p.name.Equals(message.data.name))) //Falls Name vergeben
        {
            Error error = new Error(new Error.Data("Name ist bereits vergeben", 2));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
        }
        else if (message.data.role == roleEnum.SPECTATOR) //Fals Spectator ist
        {
            if(!_gameStarted) SendHello(clientID, message);
            else SendReconnectDataSpectator(message.data.name, clientID);
        }
        else if (_clientsSet.Count(p => p.role.Equals(roleEnum.PLAYER) || p.role.Equals(roleEnum.AI)) >=
                 _maxPlayer) //Da kein Spectator muss Spieleranzahl überprüft werden
        {
            Error error = new Error(new Error.Data("Die maximale Anzahl an Spielern ist bereits im Spiel", 5));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
        }
        else //Dann Player oder Ai
        {
            SendHello(clientID, message);
        }
    }

    
    /// <summary>
    /// Verarbeitet die Nachricht um einen Spieler auf bereit zu setzten
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    private void ReadyReceived(PlayerReady message, string clientID)
    {
        //gibt es den Spieler / ist er eine KI
        if (CheckStatePlayerKi(clientID, true)) return;
        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));
        if (c is null)
        {
            Console.WriteLine("Client not existing");
            return;
        }
        
        if (!_receivingReady)
        {
            if (!c.isConnected) return;
            Error error = new Error(new Error.Data("Spiel hat bereits angefangen", 0));
            _server.SendData(JsonManager.ConvertToJason(error), new[] { c.clientID });
            return;
        }
        //ist valide - der Player wird auf bereit gesetzt
        if (c.isReady == message.data.ready) return;

        c.isReady = message.data.ready;

        _server.SendData(JsonManager.ConvertToJason(GetParticipants()),
            _clientsSet.Select(p => p.clientID).ToArray());

        CheckIfAllReady();
    }

    
    
    /// <summary>
    /// Überprüft ob alle Spieler bereit sind, sodass das Spiel beginnen kann
    /// </summary>
    private void CheckIfAllReady()
    {
        //überprüft ob alle Spieler ready sind - KIs werden nicht betrachtet
        int numberOfPlayer = _clientsSet.Count(p => IsPlayerOrAi(p));
        if (!(numberOfPlayer >= 2 && _clientsSet.Count(p => p.isReady && IsPlayerOrAi(p)) == numberOfPlayer)) return;
        
        //wenn alle bereit sind werden alle nötigen nachrichten gesendet
        _receivingReady = false;

        string[] clientIDs = _clientsSet.Where(p => p.isConnected).Select(p => p.clientID).ToArray();

        _server.SendData(JsonManager.ConvertToJason(GetParticipants()), clientIDs);

        GameStart start = new GameStart(new GameStart.Data());
        _server.SendData(JsonManager.ConvertToJason(start), clientIDs);

        foreach (var c in _clientsSet.Where(p => p.role.Equals(roleEnum.PLAYER) || p.role.Equals(roleEnum.AI)))
        {
            SendCharacterOffer(c);
        }

        Task.Run(AllPlayerReady);
    }

    private void AllPlayerReady()
    {
        _allHaveChosen.WaitOne(_characterChoiceTimeout);
        GameStart();
    }

    /// <summary>
    /// Bekommt und validiert die Charakter Auswahl eines Clients - löst alle nötigen Aktionen aus
    /// </summary>
    /// <param name="message"></param>
    /// <param name="clientID"></param>
    private void CharacterChoiceReceived(CharacterChoice message, string clientID)
    {
        //client is KI oder nicht existent
        if (CheckStatePlayerKi(clientID, true)) return;

        if (_gameStarted || _receivingReady)
        {
            Error error = new Error(new Error.Data("Es kann noch nicht gewählt werden", 0));
            _server.SendData(JsonManager.ConvertToJason(error), new []{clientID});
            return;
        }

        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));
        if (c is null)
        {
            Console.WriteLine("Client not existing");
            return;
        }
        //wahl nicht valide
        if (!c.characterOffer.Contains(message.data.characterChoice))
        {
            Error error = new Error(new Error.Data("Keine valide Characterauswahl", 7));
            BanClientWithMessage(c, JsonManager.ConvertToJason(error));
            return;
        }
        //spieler hat bereits seine Auswahl getroffen
        if (c.hasChosen)
        {
            if (!c.isConnected) return;
            Error error = new Error(new Error.Data("Auswahl bereits getroffen", 0));
            _server.SendData(JsonManager.ConvertToJason(error), new[] { clientID });
            return;
        }

        //wenn Bedingungen stimmen werden nötige Aktionen ausgelöst
        c.hasChosen = true;
        c.chosenCharacter = message.data.characterChoice;

        characterEnum character = c.characterOffer.FirstOrDefault(p => !p.Equals(message.data.characterChoice));

        _charactersAvailable.Add(character);

        CharacterChoice(c.reconnectToken, c.name, message.data.characterChoice, EventArgs.Empty);

        CheckIfAllHaveChoosen();
    }
    private void CheckIfAllHaveChoosen()
    {
        if (_clientsSet.Count(p => p.hasChosen && IsPlayerOrAi(p)) == _clientsSet.Count(p => IsPlayerOrAi(p))) _allHaveChosen.Set();
    }

    /// <summary>
    /// Startet den Spielbalauf und sendet / erwartet alle Nachriten die vor dem Gameplay gebraucht werden
    /// </summary>
    private void GameStart()
    {
        //Charakter auswahl schicken und bekommen - auf 
        _gameStarted = true;
        Error error = new Error(new Error.Data("Charakterauswahltimeout", 9));
        IEnumerable<ClientData> clients = _clientsSet.Where(p =>
            !p.hasChosen && IsPlayerOrAi(p));
        foreach (var c in clients)
        {
            c.chosenCharacter = c.characterOffer[0];
            CharacterChoice(c.reconnectToken, c.name, c.chosenCharacter, EventArgs.Empty);
            _charactersAvailable.Add(c.characterOffer[0]);
            _charactersAvailable.Add(c.characterOffer[1]);
            characterEnum character = _charactersAvailable.ElementAt(_random.Next(_charactersAvailable.Count));
            CharacterChoice(c.reconnectToken, c.name, character, EventArgs.Empty);
            _charactersAvailable.Remove(character);
            c.hasChosen = true;
            if (!c.isConnected) continue;
            _server.SendData(JsonManager.ConvertToJason(error), new[] { c.clientID });
        }
        
        readySemaphore.Set();
    }
    
    private void Reconnect(string clientID, Reconnect reconnect)
    {
        //Ausgelöst wenn der Reconnect nicht möglich ist
        if (_oldReconnectToken.Contains(reconnect.data.reconnectToken))
        {
            Error error = new Error(new Error.Data("Nicht möglich zu verbinden", 1));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
            return;
        }
        //Spieler war noch nicht connectet also kanne nicht reconnecten
        ClientData? c = _clientsSet.FirstOrDefault(p => p.reconnectToken.Equals(reconnect.data.reconnectToken));
        if (c is null)
        {
            Error error = new Error(new Error.Data("Nicht möglich zu verbinden", 1));
            _server.SendMessageAndKick(JsonManager.ConvertToJason(error), new[] { clientID });
            return;
        }
        //client wird reconnected
        c.clientID = clientID;
        c.isConnected = true;
        SendReconnectData(c, reconnect);
        ReconnectEvent(c.reconnectToken, c.name, EventArgs.Empty);
    }

    
    private void SendReconnectData(ClientData client, Reconnect reconnect)
    {
        //schickt alle nachrichten die für einen reconnect nötig sind
        client.isConnected = true;
        
        HelloClient helloClient =
            new HelloClient(new HelloClient.Data(reconnect.data.reconnectToken, _boardConfig, _gameConfig));
        _server.SendData(JsonManager.ConvertToJason(helloClient), new[] { client.clientID });
        
        _server.SendData(JsonManager.ConvertToJason(GetParticipants()), new[] { client.clientID });

        if(_gameStarted)
        {
            GameState gameState = new GameState(new GameState.Data(GetPlayerStates(), _boardState, 0));
            _server.SendData(JsonManager.ConvertToJason(gameState), new[] { client.clientID });

            Paused paused = new Paused(new Paused.Data(_isPaused, _playerWhoPaused));
            _server.SendData(JsonManager.ConvertToJason(paused), new[] { client.clientID });
        }
    }
    
    
    private void SendReconnectDataSpectator(string name, string clientID)
    {
        //wenn es sich um eine zuschauer handelt werden alle Events hier ausgelöst
        string reconnect = Guid.NewGuid().ToString();
        _clientsSet.Add(new ClientData(clientID, reconnect, name, roleEnum.SPECTATOR));
        
        NewClient(reconnect, name, roleEnum.SPECTATOR, EventArgs.Empty);
        
        HelloClient helloClient =
            new HelloClient(new HelloClient.Data(reconnect, _boardConfig, _gameConfig));
        _server.SendData(JsonManager.ConvertToJason(helloClient), new[] { clientID });

        _server.SendData(JsonManager.ConvertToJason(GetParticipants()), new[] { clientID });

        GameState gameState = new GameState(new GameState.Data(GetPlayerStates(),  _boardState , 0));
        _server.SendData(JsonManager.ConvertToJason(gameState), new[] { clientID });

        if (_isPaused)
        {
            Paused paused = new Paused(new Paused.Data(true, _playerWhoPaused));
            _server.SendData(JsonManager.ConvertToJason(paused), new[] { clientID });
        }
    }

    private void SendHello(string clientID, HelloServer message)
    {
        //schickt die Hallo nachricht an die Clients
        HelloClient helloClient =
            new HelloClient(new HelloClient.Data(Guid.NewGuid().ToString(), _boardConfig, _gameConfig));
        _server.SendData(JsonManager.ConvertToJason(helloClient), new[] { clientID });

        NewClient(helloClient.data.reconnectToken, message.data.name, message.data.role, EventArgs.Empty);

        _clientsSet.Add(new ClientData(clientID, helloClient.data.reconnectToken, message.data.name,
            message.data.role));

        _server.SendData(JsonManager.ConvertToJason(GetParticipants()), _clientsSet.Select(p => p.clientID).ToArray());
    }

    private void RemoveClientFromList(ClientData client)
    {
        //Client wird von der aktuellen Liste genommen und der reconenctoken wird gespiechert
        _oldReconnectToken.Add(client.reconnectToken);
        if (client.hasChosen && IsPlayerOrAi(client))
        {
            _charactersAvailable.Add(client.chosenCharacter);
        }

        _clientsSet.Remove(client);

        _server.SendData(JsonManager.ConvertToJason(GetParticipants()), _clientsSet.Select(p => p.clientID).ToArray());
    }

    
    private ParticipantsInfo GetParticipants()
    {
        //erstellt die Participants info aus allen clients
        return new ParticipantsInfo(new ParticipantsInfo.Data(
            _clientsSet.Where(p => p.role.Equals(roleEnum.PLAYER)).Select(p => p.name).ToArray(),
            _clientsSet.Where(p => p.role.Equals(roleEnum.SPECTATOR)).Select(p => p.name).ToArray(),
            _clientsSet.Where(p => p.role.Equals(roleEnum.AI)).Select(p => p.name).ToArray(),
            _clientsSet.Where(p => p.isReady && IsPlayerOrAi(p)).Select(p => p.name).ToArray()));
    }

    private bool IsPlayerOrAi(ClientData client)
    {
        return client.role.Equals(roleEnum.PLAYER) || client.role.Equals(roleEnum.AI);
    }

    private void ClientDisconnected(string clientID, EventArgs e)
    {
        //Client lost connection
        //Remains in List with Reconnect Token
        //Gamelogic be informed Client lost connection

        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));

        if (c is null) return;

        if (!c.isReady && _receivingReady)
        {
            BanClientByGameManager(c.name);
        }
        else
        {
            c.isConnected = false;
        }

        ClientDisconnected2(c.reconnectToken, c.name, EventArgs.Empty);
        if (_gameStarted) messageSemaphore.Release();
    }

    private void BanClient(ClientData client)
    {
        //Bannt den Client und kickt diesen 

        RemoveClientFromList(client);
        Task.Run(() => _server.KickClientAsync(client.clientID));

        if (_receivingReady) CheckIfAllReady();
        else CheckIfAllHaveChoosen();

        ClientBanned(client.reconnectToken, client.name, _gameStarted, EventArgs.Empty);
        messageSemaphore.Release();
    }

    private void BanClientWithMessage(ClientData client, string messages)
    {
        //Selbes wie oben nur mit nachricht an den Client

        RemoveClientFromList(client);

        if (client.isConnected) _server.SendMessageAndKick(messages, new[] { client.clientID });

        if (_receivingReady) CheckIfAllReady();
        else CheckIfAllHaveChoosen();

        ClientBanned(client.reconnectToken, client.name, _gameStarted, EventArgs.Empty);
    }

    public void BanClientByGameManager(string name, string message = "")
    {
        //Can be used by GameLogic to ban Client
        //Will be enqueued to be processed

        _synchronize.WaitOne();
        _newTaskQueue.Enqueue((name, message, 1));
        _synchronize.Release();
        _semNewMessage.Release();
    }

    private void BanClientByID(string clientID)
    {
        //Is used when Client sends GoodyBy_Server or Invalid Message

        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));
        if (c is null) return;
        BanClient(c);
    }

    private void BanClientByIDWithMessage(string clientID, string messages)
    {
        //Is used when Client sends GoodyBy_Server or Invalid Message

        ClientData? c = _clientsSet.FirstOrDefault(p => p.clientID.Equals(clientID));
        if (c is null)
        {
            _server.SendMessageAndKick(messages, new[] { clientID });
            return;
        }

        BanClientWithMessage(c, messages);
    }

    public void SendData(string message, string[] names)
    {
        //schickt die nachrichten and alle clients
        string[] clients = _clientsSet.Where(p => names.Contains(p.name) && p.isConnected).Select(p => p.clientID)
            .ToArray();

        _server.SendData(message, clients);
    }

    public void SendDataNotAsync(string message, string[] names)
    {
        string[] clients = _clientsSet.Where(p => names.Contains(p.name) && p.isConnected).Select(p => p.clientID)
            .ToArray();

        _server.SendDataNotAsync(message, clients);
    }

    private void SendCharacterOffer(ClientData client)
    {
        //schickt die Charakter auswahl an die Clients
        characterEnum[] characters = new characterEnum[2];
        characters[0] = _charactersAvailable.ElementAt(_random.Next(_charactersAvailable.Count));
        _charactersAvailable.Remove(characters[0]);
        characters[1] = _charactersAvailable.ElementAt(_random.Next(_charactersAvailable.Count));
        _charactersAvailable.Remove(characters[1]);

        client.characterOffer = characters;
        CharacterOffer characterOffer = new CharacterOffer(new CharacterOffer.Data(characters));

        _server.SendData(JsonManager.ConvertToJason(characterOffer), new[] { client.clientID });
    }

    public PlayerState[] GetPlayerStates()
    {
        //holt siche alle informationen der Spieler und returned ein Playerstate
        List<Player> _playerStates = _getPlayers();
        PlayerState[] playerStates = new PlayerState[_playerStates.Count];
        for (int i = 0; i < _playerStates.Count; i++)
        {
            playerStates[i].direction = _playerStates[i].Charakter.direction;
            playerStates[i].character = _playerStates[i].Charakter.Character;
            playerStates[i].playerName = _playerStates[i].Name;
            playerStates[i].playedCards = _playerStates[i].Charakter.playedCards.ToArray();
            playerStates[i].lives = _playerStates[i].Charakter.Lifes;
            playerStates[i].suspended = _playerStates[i].Charakter.deadRoundsLeft;
            playerStates[i].reachedCheckpoints = _playerStates[i].ReachedCheckpoints;
            playerStates[i].spawnPosition = new[]
                { _playerStates[i].Charakter.startField.X, _playerStates[i].Charakter.startField.Y };
            playerStates[i].turnOrder = _playerStates[i].Charakter.turnOrder;
            playerStates[i].currentPosition = new[] { _playerStates[i].Charakter.X, _playerStates[i].Charakter.Y };
            playerStates[i].lembasCount = _playerStates[i].Charakter.Lembas;
        }

        return playerStates;
    }
}