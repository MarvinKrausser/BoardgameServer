using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay.Tiles;
using ConsoleApp1.Gameplay.Utility;

namespace ConsoleApp1.Gameplay;

public class GameManager
{
    //hier wird das ganze spiel verwaltet
    //Spieler und Charaktere initialisiert 
    //Gameboard initialisiert und ausgewertet
    //Nachrichten empfangen und triggert dazugehörige Events

    public List<Player> players = new ();
    public List<Player> viewers { get; set; }
    public ClientManager clientManager;
    public Gameboard gameboard;
    private GameConfig gameConfig;
    private BoardConfig boardConfig;

    public Eye eye => gameboard.eye;

    private bool pauseVariable { get; set; }
    private PausableTimeout _pausableTimeout = new PausableTimeout();
    private readonly List<Player> playerSendetCardOffer = new List<Player>();
    private bool _receivingCardChoices;
    private bool _isRunning = true;


    //methode um moves auf charakter anzuwenden
    //parameter müssen evtl noch angepasst werden (Karte statt List von Moves?)
    //es fehlen noch ein paar moves wie U-Turn usw.

    public GameManager(BoardConfig boardconfig, GameConfig gameconfig)
    {
        this.gameboard = new Gameboard(boardconfig, players);
        this.viewers = new List<Player>();

        this.boardConfig = boardconfig;
        this.gameConfig = gameconfig;
    }


    public void startGame(int port)
    {
        clientManager = new ClientManager();
        clientManager.NewClient += NewClient;
        clientManager.ClientDisconnected2 += ClientDisconnected2;
        clientManager.ClientBanned += ClientBanned;
        clientManager.CharacterChoice += CharacterChoice;
        clientManager.Pause += Pause;
        clientManager.ReconnectEvent += ReconnectEvent;

        clientManager.Setup(port, boardConfig, gameConfig, GetPlayers, gameboard.BoardState);

        clientManager.readySemaphore.WaitOne();

        getTurnOrder();
        
        GameState gameState =
            new GameState(
                new GameState.Data(clientManager.GetPlayerStates(), gameboard.BoardState ,
                    0));
        clientManager.SendData(JsonManager.ConvertToJason(gameState), players.Concat(viewers).Select(p => p.Name).ToArray());

        Task.Run(() => ReceiveCardChoice());


        int roundNumber = 0;
        while (_isRunning)
        {
            if (players.Count == 1) //checking, if only one player is in game
            {
                EndGame(players[0]);
                _isRunning = false;
                break;
            }

            roundNumber += 1;

            if (gameConfig.maxRounds > 0 && roundNumber > gameConfig.maxRounds || 
                players.Count(p => p.Charakter.IsDead) == players.Count) //check if max rounds is reached
            {
                Console.WriteLine("max rounds reached.");
                var mostCheckpointsNumber = players.Max(p => p.ReachedCheckpoints);
                var mostCheckpoints =
                    players.Where(p => p.ReachedCheckpoints.Equals(mostCheckpointsNumber)).ToList();

                if (mostCheckpoints.Any(p => !p.Charakter.IsDead)) mostCheckpoints = mostCheckpoints.Where(p => !p.Charakter.IsDead).ToList();

                Player winner = new Player("", roleEnum.AI);

                if (mostCheckpoints.Count == 1)
                {
                    winner = mostCheckpoints[0];
                }
                else
                {
                    CheckPoint checkPointNext = gameboard.CheckPoints[mostCheckpointsNumber];
                    HashSet<(Player, int)> paths = new HashSet<(Player, int)>();
                    foreach (var p in mostCheckpoints)
                    {
                        int path = FindPath(p, checkPointNext).Count;
                        paths.Add((p, path));
                    }

                    var winners = paths.Where(p => p.Item2 == paths.Min(p => p.Item2)).Select(p => p.Item1).ToList();

                    Random random = new Random();
                    int randomIndex = random.Next(0, winners.Count);
                    winner = winners[randomIndex];
                }

                Console.WriteLine("the winner is after max rounds: " + winner.Name);
                EndGame(winner);
                break;
            }

            //Planungsphase
            //Spieler ziehen Karten vom Nachziehstapel (bis 9 abh. von Leben) - nicht genügen Karten wird Nachziestapel neu gemischt
            //Spieler loggen davon 5 ein - rest auf Ablagestapel - timeout beachten

            //Planungsphase start
            Console.WriteLine("Planungsphase start.");

            //send CARD_OFFER Message to Client
            Console.WriteLine("start sending CARD_OFFER messages to all cliends");
            _receivingCardChoices = true;
            for (int i = 0; i < players.Count; i++)
            {
                //create and send new handcards

                if (players[i].Charakter.IsDead)
                {
                    continue;
                }

                players[i].Charakter.cards.createAndGetNewHandkarten(players[i].Charakter.Lifes); //create handcards
                if (players[i].IsPlayerBanned || !players[i].PlayerConnected)
                {
                    //skip player, if he is banned or not connected
                    continue;
                }

                List<cardEnum> playerHandCards = players[i].Charakter.cards.getHandkarten();
                CardOffer cardOffer = new CardOffer(new CardOffer.Data(playerHandCards.ToArray()));

                clientManager.SendData(JsonManager.ConvertToJason(cardOffer), new[] { players[i].Name });
            }

            Console.WriteLine("sendet all CARD_OFFER messages");

            if(players.Any(p => !p.Charakter.IsDead && p.PlayerConnected && !p.IsPlayerBanned))
            {
                _pausableTimeout.StartTimeout(gameConfig.cardSelectionTimeout);
                _pausableTimeout.sem.WaitOne();
            }
            _receivingCardChoices = false;


            var playersNotChoosen = players.Where(p => !playerSendetCardOffer.Contains(p));

            foreach (var p in playersNotChoosen)
            {
                List<cardEnum> pCardChoice = new List<cardEnum>();
                for (int i = 0; i < 5; i++)
                {
                    pCardChoice.Add(cardEnum.EMPTY);
                }

                p.Charakter.cards.playersCardChoice(pCardChoice);
                Error error = new Error(new Error.Data("Kartenauswahltimeout", 8));

                clientManager.SendData(p.Name, new[] { JsonManager.ConvertToJason(error) });
            }

            foreach (var p in players)
            {
                p.hasReceivedError = false;
            }

            //receive CARD_CHOICE content and call the playerCardChoice(cards) method in Cards class for each player

            playerSendetCardOffer.Clear(); //reset

            Console.WriteLine("Planungsphase end");
            //Planungsphase end

            RoundStart roundStart = new RoundStart(new RoundStart.Data(clientManager.GetPlayerStates()));
            clientManager.SendData(JsonManager.ConvertToJason(roundStart), players.Concat(viewers).Select(p => p.Name).ToArray());
            
            _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay); //TODO: Muss das bei roundstart auch
            _pausableTimeout.sem.WaitOne();

            //Rundephase - 5 x Zugrunden (für jede Karte) - wird übersprungen wenn Spieler tot
            Console.WriteLine("Rundenphase start");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Zugrunde {i} ");
                //Bewegungsphase
                //Zugreihenfolge bestimmen
                getTurnOrder();

                //für jeden Spieler -> moveCharakter(player, cardMove)
                foreach (var player in players)
                {
                    if (player.Charakter.IsDead)
                    {
                        continue;
                    }

                    var playerCardChoice = player.Charakter.cards.getHandkarten();
                    List<cardEnum> currentMove = new List<cardEnum> { playerCardChoice[i] };
                    
                    Console.WriteLine("\n currentMove: " + currentMove[0] + "| Player: " + player.Name + "\n");
                    var states = moveCharakter(player, currentMove);
                    player.Charakter.playedCards.Add(currentMove[0]);

                    CardEvent cardEvent = new CardEvent(new CardEvent.Data(player.Name, currentMove[0],
                        states.Item1, states.Item2)); //send card event after one character moved
                    clientManager.SendData(JsonManager.ConvertToJason(cardEvent),
                        players.Concat(viewers).Select(p => p.Name).ToArray());

                    _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                    _pausableTimeout.sem.WaitOne();
                }

                Console.WriteLine("Aktionsphase start");
                //Aktionsphase
                //shoot() - ist schon für alle spieler
                //moveCharaktersOnRiver() - ist schon für alle Spieler

                Console.WriteLine("shooting");
                shoot();

                Console.WriteLine("river");
                moveCharaktersOnRiver();

                Console.WriteLine("eagle");
                TriggerEagleEvent();

                if (CheckCheckpoints()) //schaut für alle Spieler nach, ob sie nach der Aktionsphase auf einem Checkpoint stehen
                {
                    break;
                }

                if (i == 4)
                {
                    //wiederbeleben der Spieler
                    Console.WriteLine("respawning");
                    respawnAllDeadCharakters();

                    for(int j = 0; j < players.Count; j++)
                    {
                        if (players[j].IsPlayerBanned)
                        {
                            players.RemoveAt(j);
                            j--;
                        }
                    }
                }

                gameState =
                    new GameState(
                        new GameState.Data(clientManager.GetPlayerStates(), gameboard.BoardState ,
                            roundNumber));
                clientManager.SendData(JsonManager.ConvertToJason(gameState), players.Concat(viewers).Select(p => p.Name).ToArray());
                
                _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                _pausableTimeout.sem.WaitOne();
            }
            
            if (players.Count == 1)
            {
                EndGame(players[0]);
                break;
            }
            
            if (players.Count == 0)
            {
                EndGame(null);
                break;
            }

            for (int i = 0; i < players.Count; i++)
            {
                players[i].Charakter.cards.roundOver();
                players[i].Charakter.playedCards.Clear();
            }
        }

        clientManager.EndGame(); //Beendet Server, muss am Ende aufgerufen werden
        Console.WriteLine("Ende");
    }

    private List<Player> GetPlayers()
    {
        return players;
    }

    private void EndGame(Player? winner)
    {
        _isRunning = false;
        clientManager.messageSemaphore.Release();
        _pausableTimeout.semPaused.Set();

        var gameEnd = winner is null ? new GameEnd(new GameEnd.Data(clientManager.GetPlayerStates(), string.Empty)) : new GameEnd(new GameEnd.Data(clientManager.GetPlayerStates(), winner.Name));
        clientManager.SendDataNotAsync(JsonManager.ConvertToJason(gameEnd),
            players.Concat(viewers).Select(p => p.Name).ToArray());
    }

    private Task ReceiveCardChoice()
    {
        while (true)
        {
            clientManager.messageSemaphore.WaitOne(); //wait on next message
            if (pauseVariable) _pausableTimeout.semPaused.WaitOne();
            if (!_isRunning) break;
            if (clientManager.messages.Any())
            {
                var message = clientManager.messages.Dequeue(); //1. message 2. type 3. Name
                CardChoice cardChoice = JsonManager.DeserializeJson<CardChoice>(message.Item1); //deserialize Message
                var cards = cardChoice.data.cards.ToList(); //get cards from message: List <cardEnum> cards
                Player? player = players.Find(p => p.Name.Equals(message.Item2)); //get player which send message
                if (player is null) continue;
                
                if (!_receivingCardChoices)
                {
                    Error error =
                        new Error(new Error.Data("Keine Kartenauswahl Zeitraum", 0)); // Error code might be wrong
                    clientManager.SendData(JsonManager.ConvertToJason(error), new[] { player.Name });
                    player.hasReceivedError = true;
                }
                else if (playerSendetCardOffer.Contains(player))
                {
                    if (!player.hasReceivedError)
                    {
                        Error error =
                            new Error(new Error.Data("Already send Chard Choice", 0)); // Error code might be wrong
                        clientManager.SendData(JsonManager.ConvertToJason(error), new[] { player.Name });
                        player.hasReceivedError = true;
                    }
                }
                else if (!player.Charakter.cards.playersCardChoice(cards)) //call the playerCardChoice Method for the player
                {
                    Error error = new Error(new Error.Data("Keine valide Kartenauswahl", 6));
                    clientManager.BanClientByGameManager(player.Name, JsonManager.ConvertToJason(error));
                    player.IsPlayerBanned = true;
                }
                else playerSendetCardOffer.Add(player);
            }

            if (playerSendetCardOffer.Count >= players.Count(p => p.PlayerConnected && !p.IsPlayerBanned && !p.Charakter.IsDead))
            {
                //test if every player sendet a CARD_CHOICE Message
                if (_receivingCardChoices)
                {
                    _pausableTimeout.StopTimeout();
                    _pausableTimeout.sem.Set();
                }
            }
        }

        return Task.CompletedTask;
    }

    private void NewClient(string reconnect, string name, roleEnum role, EventArgs eventArgs)
    {
        Console.WriteLine($"New Client: Name: {name}, Role: {role}, ReconnectToken: {reconnect}");
        Player newPlayer = new Player(name, role);
        if (role.Equals(roleEnum.SPECTATOR))
        {
            viewers.Add(newPlayer);
        }
        else
        {
            players.Add(newPlayer);
        }
    }

    private void ClientDisconnected2(string reconnectToken, string name, EventArgs eventArgs)
    {
        Console.WriteLine($"Client disconnected: Name: {name}, ReconnectToken: {reconnectToken}");
        Player? player = players.Concat(viewers).FirstOrDefault(p => p.Name.Equals(name));
        if (player is null) return;
        player.PlayerConnected = false;
    }

    private void ClientBanned(string reconnectToken, string name, bool hasStarted, EventArgs eventArgs)
    {
        Console.WriteLine($"Client banned: Name: {name}, ReconnectToken: {reconnectToken}");
        Player? player = viewers.Find(p => p.Name.Equals(name));
        if (player is not null)
        {
            viewers.Remove(player);
            return;
        }

        player = players.Find(p => p.Name.Equals(name));
        if (player is null) return;
        if (hasStarted) player.IsPlayerBanned = true;
        else players.Remove(player);
    }

    private void CharacterChoice(string reconnectToken, string name, characterEnum character, EventArgs eventArgs)
    {
        Console.WriteLine(
            $"Character choice: Name: {name}, ReconnectToken: {reconnectToken}, CharacterChoice: {character}");
        Player? player = players.Find(p => p.Name.Equals(name));
        if (player is null) return;
        player.InitializeCharakter(gameboard.getStartKoordinate(), 3, gameConfig.startLembas, character,
            gameConfig.reviveRounds, gameboard.CheckPoints);
    }

    private void Pause(string reconnectToken, string name, bool isPaused, EventArgs eventArgs)
    {
        Console.WriteLine($"Pause: Name: {name}, IsPause: {isPaused}, ReconnectToken: {reconnectToken}");
        if (isPaused == pauseVariable) return; //Passier nie
        if (isPaused)
        {
            _pausableTimeout.PauseTimeout();
            pauseVariable = true;
            return;
        }

        pauseVariable = false;
        _pausableTimeout.ResumeTimeout();
    }

    private void ReconnectEvent(string reconnectToken, string name, EventArgs eventArgs)
    {
        Console.WriteLine($"Reconnect: Name: {name}, ReconnectToken: {reconnectToken}");
        Player? player = players.Concat(viewers).FirstOrDefault(p => p.Name.Equals(name));
        if (player is null) return;
        player.PlayerConnected = true;
    }

    public void TriggerEagleEvent()
    {
        var unoccupiedEagleField = gameboard.eagleTiles.Where(p => !gameboard.GetIsOccupied(p)).ToArray();
        if (!unoccupiedEagleField.Any())
        {
            return;
        }

        foreach (var player in players.Where((p => !p.Charakter.IsDead)))
        {
            var currTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];
            if (currTile is not EagleTile) continue;
            Random random = new Random();
            int randomIndex = random.Next(unoccupiedEagleField.Length);
            var newTile = unoccupiedEagleField[randomIndex];
            player.Charakter.X = newTile.X;
            player.Charakter.Y = newTile.Y;
            unoccupiedEagleField = gameboard.eagleTiles.Where(p => !gameboard.GetIsOccupied(p)).ToArray();

            EagleEvent eagleEvent = new EagleEvent(new EagleEvent.Data(player.Name, clientManager.GetPlayerStates()));
            clientManager.SendData(JsonManager.ConvertToJason(eagleEvent), players.Concat(viewers).Select(p => p.Name).ToArray());
            _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
            _pausableTimeout.sem.WaitOne();
        }
    }


    /// <summary>
    /// Alle standard bewegungen der Karten werden über diese Methode ausgefürt. Es wird ein Spieler, sowie dessen Bewegungen die
    /// ausgeführt werden sollen übergeben.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="moves"></param>
    public (PlayerState[][], BoardState[]) moveCharakter(Player player, List<cardEnum> moves)
    {
        List<PlayerState[]> playerStates = new List<PlayerState[]>();
        List<BoardState> boardStates = new List<BoardState>();
        cardEnum currentCard;

        foreach (var move in moves)
        {
            if (move == cardEnum.AGAIN)
            {
                currentCard = player.Charakter.lastPlayedCard;
            }
            else currentCard = move;

            Console.WriteLine("Stats of {0}: {1} {2} {3}", player.Name, player.Charakter.X, player.Charakter.Y,
                player.Charakter.direction);
            switch (currentCard)
            {
                //vor und rückwärtsbewegung des charakters. Es wird geschaut ob der move ausführbar ist sonst wird gewartet
                case cardEnum.MOVE_1:
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;

                case cardEnum.MOVE_2:
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;

                case cardEnum.MOVE_3:
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    movementHelper(player, player.Charakter.direction);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;

                case cardEnum.MOVE_BACK:
                    directionEnum dir = directionEnum.EAST;

                    switch (player.Charakter.direction)
                    {
                        case directionEnum.NORTH:
                            dir = directionEnum.SOUTH;
                            break;
                        case directionEnum.SOUTH:
                            dir = directionEnum.NORTH;
                            break;
                        case directionEnum.WEST:
                            dir = directionEnum.EAST;
                            break;
                        case directionEnum.EAST:
                            dir = directionEnum.WEST;
                            break;
                    }

                    movementHelper(player, dir);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;
                //bei einer drehung wird eine Helfermethode aufgerufen die aufgrund der aktuellen position den spieler dreht
                case cardEnum.LEFT_TURN:
                case cardEnum.RIGHT_TURN:
                    player.Charakter.direction = turnCharakterHelper(currentCard, player);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;

                case cardEnum.U_TURN:
                    player.Charakter.direction =
                        turnCharakterHelper(cardEnum.LEFT_TURN,
                            player); //zweimal in eine richtung drehen ist ein U-Turn
                    player.Charakter.direction = turnCharakterHelper(cardEnum.LEFT_TURN, player);
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;

                case cardEnum.LEMBAS:
                    player.Charakter.Lembas += 1;
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;
                case cardEnum.EMPTY:
                    playerStates.Add(clientManager.GetPlayerStates());
                    boardStates.Add(gameboard.BoardState);
                    break;
            }
            
            player.Charakter.lastPlayedCard = currentCard;
        }
        return (playerStates.ToArray(), boardStates.ToArray());
    }

    /// <summary>
    /// Lässt alle Spieler schießen.
    /// Überprüft auf Richtung und Hindernisse. Jeder Spieler kann nur einmal schießen und es wird nur ein Ziel getroffen
    /// </summary>
    public void shoot()
    {
        List<Player> shootOrder = new List<Player>();
        //jeder Spieler wird mit jedem verglichen, Abhänig von den Koordinaten und der Richtung schauen ob Ziel getroffen werden könnte
        //wenn ja, überprüfung auf Hindernisse dann wird Ziel getroffen
        foreach (var player in players)
        {
            Console.WriteLine("Player {0} looks for target", player.Name);
            //wenn spieler nicht genügen Lembas wird abgebrochen => Optimierung
            if (player.Charakter.Lembas < gameConfig.shotLembas)
            {
                Console.WriteLine("player has not enought lembas");
                continue;
            }

            if (player.Charakter.IsDead) continue;
            

            switch (player.Charakter.direction)
            {
                case directionEnum.NORTH:
                    shootOrder = players.OrderByDescending(p => p.Charakter.Y).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.X == otherPlayer.Charakter.X &&
                            player.Charakter.Y > otherPlayer.Charakter.Y) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.Y; i > otherPlayer.Charakter.Y; i--)
                            {
                                Console.WriteLine("Y coordinate getting checked is: " + i);
                                if (!gameboard.isWalkable(player.Charakter.X, i, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 1 \n");
                                
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                ShotEvent shotEvent = new ShotEvent(new ShotEvent.Data(player.Name, otherPlayer.Name,
                                    clientManager.GetPlayerStates()));
                                clientManager.SendData(JsonManager.ConvertToJason(shotEvent),
                                    players.Concat(viewers).Select(p => p.Name).ToArray());

                                _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                                _pausableTimeout.sem.WaitOne();
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                            //todo: kurzer test ob das so klappt; GameConfig muss och intitialisiert werden
                        }
                    }

                    break;
                case directionEnum.SOUTH:
                    shootOrder = players.OrderBy(p => p.Charakter.Y).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.X == otherPlayer.Charakter.X &&
                            player.Charakter.Y < otherPlayer.Charakter.Y) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.Y; i < otherPlayer.Charakter.Y; i++)
                            {
                                if (!gameboard.isWalkable(player.Charakter.X, i, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 2 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                ShotEvent shotEvent = new ShotEvent(new ShotEvent.Data(player.Name, otherPlayer.Name,
                                    clientManager.GetPlayerStates()));
                                
                                clientManager.SendData(JsonManager.ConvertToJason(shotEvent),
                                    players.Concat(viewers).Select(p => p.Name).ToArray());

                                _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                                _pausableTimeout.sem.WaitOne();
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                        }
                    }

                    break;
                case directionEnum.WEST:
                    shootOrder = players.OrderByDescending(p => p.Charakter.X).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.Y == otherPlayer.Charakter.Y &&
                            player.Charakter.X > otherPlayer.Charakter.X) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.X; i > otherPlayer.Charakter.X; i--)
                            {
                                if (!gameboard.isWalkable(i, player.Charakter.Y, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 3 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;

                                ShotEvent shotEvent = new ShotEvent(new ShotEvent.Data(player.Name, otherPlayer.Name,
                                    clientManager.GetPlayerStates()));
                                clientManager.SendData(JsonManager.ConvertToJason(shotEvent),
                                    players.Concat(viewers).Select(p => p.Name).ToArray());

                                _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                                _pausableTimeout.sem.WaitOne();
                                goto
                                    nextPlayerLabel; //verhindert dass ein spieler mehrere Ziele treffen kann. Not best practice but this will be fine :)
                            }
                        }
                    }

                    break;
                case directionEnum.EAST:
                    shootOrder = players.OrderBy(p => p.Charakter.X).ToList();
                    foreach (var otherPlayer in shootOrder)
                    {
                        if(otherPlayer.Charakter.IsDead) continue;
                        if (player.Charakter.Y == otherPlayer.Charakter.Y &&
                            player.Charakter.X < otherPlayer.Charakter.X) //wird je nach Blickrichtung angepasst
                        {
                            Console.WriteLine("{0} is a possible target", otherPlayer.Name);
                            bool canBeHit = true;
                            //other Player kommt als mögliches Ziel in frage 
                            for (int i = player.Charakter.X; i < otherPlayer.Charakter.X; i++)
                            {
                                if (!gameboard.isWalkable(i, player.Charakter.Y, player.Charakter.direction))
                                {
                                    Console.WriteLine("Shot was blocked");
                                    canBeHit = false;
                                    break;
                                }
                            }

                            //spieler wird getroffen falls true
                            if (canBeHit)
                            {
                                Console.WriteLine("\n ISSS HIT 4 \n");
                                Console.WriteLine("{0} got hit", otherPlayer.Name);
                                otherPlayer.Charakter.isHitEvent();
                                player.Charakter.Lembas -= gameConfig.shotLembas;
                                
                                ShotEvent shotEvent = new ShotEvent(new ShotEvent.Data(player.Name, otherPlayer.Name,
                                    clientManager.GetPlayerStates()));
                                clientManager.SendData(JsonManager.ConvertToJason(shotEvent),
                                    players.Concat(viewers).Select(p => p.Name).ToArray());

                                _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
                                _pausableTimeout.sem.WaitOne();
                                goto nextPlayerLabel;
                            }
                        }
                    }

                    break;
            }

            nextPlayerLabel: ;
        }
    }

    /// <summary>
    /// schaut für alle Spieler ob sie auf einem Fluss Feld stehen, wenn ja werden alle 2 tiles weiter verschobe
    /// SONDERFALL: Ende des flusses wird auf die Zugreihenfolge geachtet
    /// </summary>
    public void moveCharaktersOnRiver()
    {
        foreach (var player in players.Where(p => !p.Charakter.IsDead))
        {
            moveCharakterOnRiverHelper(player, gameConfig.riverMoveCount);
        }
    }

    private void moveCharakterOnRiverHelper(Player player, int riverMoveCount)
    {
        Console.WriteLine("{0} is being moved", player.Name);

        List<PlayerState[]> playerStates = new List<PlayerState[]>();
        List<BoardState> boardStates = new List<BoardState>();

        var posX = player.Charakter.X;
        var posY = player.Charakter.Y;
        for (int i = 0; i < riverMoveCount; i++)
        {

            //hier beginnt der spaß
            var currTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];

            if (currTile is RiverField currRiverField)
            {
                //check next Field
                var nextTile = getNextRiverTile(currRiverField);

                if (nextTile == null)
                {
                    Console.WriteLine("{0} fell from board", player.Name);
                    player.Charakter.killCharakter();
                    break;
                }
                //sollte kein anderer Spieler mehr vor einem sein so wird bewegt
                movementHelper(player, currRiverField.Direction);
                    
                if(nextTile is RiverField nextRiverField)
                {
                    RotatePlayerRiver(currRiverField, nextRiverField, player);
                }
                    
                player.Charakter.lastRiverField = currRiverField;
                playerStates.Add(clientManager.GetPlayerStates());
                boardStates.Add(gameboard.BoardState);
            }
        }
        
        if (!(player.Charakter.X.Equals(posX) && player.Charakter.Y.Equals(posY)))
        {
            RiverEvent riverEvent = new RiverEvent(new RiverEvent.Data(player.Name,
                playerStates.ToArray(), boardStates.ToArray()));
            clientManager.SendData(JsonManager.ConvertToJason(riverEvent),
                players.Concat(viewers).Select(p => p.Name).ToArray());

            _pausableTimeout.StartTimeout(gameConfig.serverIngameDelay);
            _pausableTimeout.sem.WaitOne();
        }
        player.Charakter.lastRiverField = null;
    }

    private void RotatePlayerRiver(RiverField currrentTile, RiverField nextTile, Player player)
    {
        if (InvertDirection(currrentTile.Direction).Equals(nextTile.Direction)) player.Charakter.direction = InvertDirection(player.Charakter.direction);
        if (RotateDirectionLeft(currrentTile.Direction).Equals(nextTile.Direction)) player.Charakter.direction = RotateDirectionLeft(player.Charakter.direction);
        if (RotateDirectionRight(currrentTile.Direction).Equals(nextTile.Direction)) player.Charakter.direction = RotateDirectionRight(player.Charakter.direction);
    }

    private directionEnum InvertDirection(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.EAST:
                return directionEnum.WEST;
            case directionEnum.WEST:
                return directionEnum.EAST;
            case directionEnum.SOUTH:
                return directionEnum.NORTH;
            case directionEnum.NORTH:
                return directionEnum.SOUTH;
        }
        return directionEnum.SOUTH;
    }

    private directionEnum RotateDirectionRight(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                return directionEnum.EAST;
            case directionEnum.SOUTH:
                return directionEnum.WEST;
            case directionEnum.EAST:
                return directionEnum.SOUTH;
            case directionEnum.WEST:
                return directionEnum.NORTH;
        }
        
        return directionEnum.SOUTH;
    }
    
    private directionEnum RotateDirectionLeft(directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                return directionEnum.WEST;
            case directionEnum.SOUTH:
                return directionEnum.EAST;
            case directionEnum.EAST:
                return directionEnum.NORTH;
            case directionEnum.WEST:
                return directionEnum.SOUTH;
        }
        
        return directionEnum.SOUTH;
    }


    private Tile getNextRiverTile(RiverField currentTile)
    {
        var x = currentTile.X;
        var y = currentTile.Y;

        switch (currentTile.Direction)
        {
            case directionEnum.NORTH:
                //wenn auserhalb des spielfeldes wird null returned
                if (y - 1 >= 0)
                {
                    return gameboard.tiles[x, y - 1];
                }

                break;
            case directionEnum.SOUTH:
                if (y + 1 < gameboard.Height)
                {
                    return gameboard.tiles[x, y + 1];
                }

                break;
            case directionEnum.WEST:
                if (x - 1 >= 0)
                {
                    return gameboard.tiles[x - 1, y];
                }

                break;
            case directionEnum.EAST:
                if (x + 1 < gameboard.Width)
                {
                    return gameboard.tiles[x + 1, y];
                }

                break;
        }

        return null;
    }

    //schaut sich die aktuelle orientation des Charakters and und ändert die Richtung entsprechend 
    private directionEnum turnCharakterHelper(cardEnum move, Player player)
    {
        directionEnum currentDirection = player.Charakter.direction;

        switch (currentDirection)
        {
            case directionEnum.NORTH:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.WEST;
                }

                return directionEnum.EAST;

            case directionEnum.EAST:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.NORTH;
                }

                return directionEnum.SOUTH;

            case directionEnum.SOUTH:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.EAST;
                }

                return directionEnum.WEST;

            case directionEnum.WEST:
                if (move == cardEnum.LEFT_TURN)
                {
                    return directionEnum.SOUTH;
                }

                return directionEnum.NORTH;
        }

        throw new Exception("Internal Error in Player direction movement");
    }


    //findet alle Nachbarn eines Charakteres von einem Spieler, abhänig davon in welche richtung er schaut

    private bool movementHelper(Player player, directionEnum direction)
    {
        if (player.Charakter.IsDead) return false;
        var newCoordinates = GiveCoordinateInDirection((player.Charakter.X, player.Charakter.Y), direction);

        if (newCoordinates.Item1 < 0 || newCoordinates.Item1 >= gameboard.Width || newCoordinates.Item2 < 0 ||
            newCoordinates.Item2 >= gameboard.Height)
        {
            player.Charakter.X = newCoordinates.Item1;
            player.Charakter.Y = newCoordinates.Item2;
            player.Charakter.killCharakter();
            return true;
        }

        Tile currentTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];
        Tile nextTile = gameboard.tiles[newCoordinates.Item1, newCoordinates.Item2];
        if (currentTile.hasWall(direction) || nextTile.hasWall(InvertDirection(direction)) || nextTile.IsNotWalkable)
        {
            return false;
        }

        if (gameboard.GetIsOccupied(nextTile))
        {
            if (!movementHelper(players.First(p => !p.Charakter.IsDead && p.Charakter.X == nextTile.X && p.Charakter.Y == nextTile.Y), direction))
            {
                return false;
            }
        }
        
        player.Charakter.X = newCoordinates.Item1;
        player.Charakter.Y = newCoordinates.Item2;
        
        if (nextTile is Hole) player.Charakter.killCharakter();
        else if (nextTile is LembasFieldTile { amount: >= 1 } nextTileLembas)
        {
            nextTileLembas.amount--;
            player.Charakter.Lembas++;
        }
        
        return true;
    }
    
    private (int, int) GiveCoordinateInDirection((int, int) coordinates, directionEnum direction)
    {
        switch (direction)
        {
            case directionEnum.NORTH:
                coordinates.Item2 += -1;
                break;
            case directionEnum.SOUTH:
                coordinates.Item2 += 1;
                break;
            case directionEnum.WEST:
                coordinates.Item1 += -1;
                break;
            case directionEnum.EAST:
                coordinates.Item1 += 1;
                break;
        }

        return coordinates;
    }

    public List<Player> findPushingNeighbours(Player player, directionEnum direction)
    {
        List<Player> neighbours = new List<Player>();
        findPushingNeigboursHelper(player, direction, neighbours);
        
        return neighbours;
    }

    //ruft nebenliegende Nachbarn rekursiv auf um zu sehen ob diese auch nachbarn haben
    private void findPushingNeigboursHelper(Player player, directionEnum direction, List<Player> neiList)
    {
        int x = player.Charakter.X;
        int y = player.Charakter.Y;

        if (!gameboard.isWalkable(player.Charakter.X, player.Charakter.Y, direction))
        {
            return; //Maybe
        }

        switch (direction)
        {
            case directionEnum.NORTH:
                y += -1;
                break;
            case directionEnum.SOUTH:
                y += 1;
                break;
            case directionEnum.WEST:
                x += -1;
                break;
            case directionEnum.EAST:
                x += 1;
                break;
        }

        foreach (var otherPlayer in players)
        {
            if (otherPlayer.Charakter.X == x && otherPlayer.Charakter.Y == y && !otherPlayer.Charakter.IsDead)
            {
                neiList.Add(otherPlayer);
                findPushingNeigboursHelper(otherPlayer, direction, neiList);
            }
        }
    }

    /// <summary>
    /// Findet für jeden Spieler den kürzesten Pfad zum Auge und sortiert zuerst nach Uhrzeigersinn und dann nach der Pfadlänge
    /// Methode gib sowohl eine sortierte Liste zurück, setzt aber gleichzeitig die "players" Liste
    /// </summary>
    /// <returns></returns>
    public List<Player> getTurnOrder()
    {
        List<Player> orderedPlayers = new List<Player>();
        foreach (var player in players)
        {
            player.Charakter.pathToEye = FindPath(player, gameboard.eye);
            orderedPlayers.Add(player);
        }


        var tempList = orderedPlayers.OrderBy(player =>
        {
            var xDist = player.Charakter.X - eye.X;
            var yDist = player.Charakter.Y - eye.Y;

            var angle = (Math.Atan2(yDist, xDist)) * 180 / Math.PI;

            if (angle < 0)
            {
                angle += 360; // Winkelbereich von [-180,180] auf [0,360] 
            }

            //todo: direction testen 
            // Um für jede Richtung des Auges um n x 90 Grad versetzen wegen Uhrzeigersinn
            switch (eye.Direction)
            {
                case directionEnum.NORTH:
                    angle += 90;
                    break;
                case directionEnum.EAST:
                    break;
                case directionEnum.SOUTH:
                    angle += 270;
                    break;
                case directionEnum.WEST:
                    angle += 180;
                    break;
            }

            if (angle >= 360)
            {
                angle -= 360;
            }

            return angle;
        }).ToList();
        tempList = tempList.OrderBy(player => player.Charakter.pathToEye.Count).ToList();

        int i = 0;
        foreach (var player in tempList)
        {
            if (player.Charakter.IsDead) player.Charakter.turnOrder = -1;
            else
            {
                player.Charakter.turnOrder = i;
                i++;
            }
        }

        players = tempList;
        return tempList;
    }

    /// <summary>
    /// A* Pathfinding algorithmus findet den kürzesten Weg von Spieler zum Auge und gibt diesen als Liste zurück.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public List<Tile> FindPath(Player player, Tile target)
    {
        var start = target;
        Tile end;
        Tile? extraTile = null;
        if (player.Charakter.X < 0)
        {
            end = gameboard.tiles[player.Charakter.X + 1, player.Charakter.Y];
            extraTile = new Hole(player.Charakter.X, player.Charakter.Y);
        }
        else if (player.Charakter.X >= gameboard.Width)
        {
            end = gameboard.tiles[player.Charakter.X - 1, player.Charakter.Y];
            extraTile = new Hole(player.Charakter.X, player.Charakter.Y);
        }
        else if (player.Charakter.Y < 0)
        {
            end = gameboard.tiles[player.Charakter.X, player.Charakter.Y + 1];
            extraTile = new Hole(player.Charakter.X, player.Charakter.Y);
        }
        else if (player.Charakter.Y >= gameboard.Height)
        {
            end = gameboard.tiles[player.Charakter.X, player.Charakter.Y - 1];
            extraTile = new Hole(player.Charakter.X, player.Charakter.Y);
        }
        else
        {
            end = gameboard.tiles[player.Charakter.X, player.Charakter.Y];
        }

        List<Tile> openList = new List<Tile> { start };
        List<Tile> closedList = new List<Tile>();

        for (int i = 0; i < gameboard.Width; i++)
        {
            for (int j = 0; j < gameboard.Height; j++)
            {
                var node = gameboard.tiles[i, j];
                node.GCost = int.MaxValue;
                node.Parent = null;
            }
        }

        start.GCost = 0;
        start.HCost = calculateDistanceCost(start, end);

        while (openList.Count > 0)
        {
            Tile currNode = getLowestFCostNode(openList);
            if (currNode == end)
            {
                //Das Ziel wurde erreicht
                var path = calculatePath(end);
                if(extraTile is not null) path.Add(extraTile);
                return path;
            }

            openList.Remove(currNode);
            closedList.Add(currNode);

            List<Tile> neighbors = gameboard.GetNeighbors(currNode);

            foreach (var neighbor in neighbors)
            {
                if (closedList.Contains(neighbor)) continue;
                if (neighbor.IsNotWalkable && neighbor != end)
                {
                    closedList.Add(neighbor);
                    continue;
                }

                int tempGCost = currNode.GCost + calculateDistanceCost(currNode, neighbor);
                if (tempGCost < neighbor.GCost)
                {
                    //wenn der neue Pfad schneller als der alte ist, werden alle stats akutalisiert
                    neighbor.Parent = currNode;
                    neighbor.GCost = tempGCost;
                    neighbor.HCost = calculateDistanceCost(neighbor, end);

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return new List<Tile>();
    }

    private int calculateDistanceCost(Tile a, Tile b)
    {
        int xDistance = Math.Abs(a.X - b.X);
        int yDistance = Math.Abs(a.Y - b.Y);
        return (xDistance + yDistance) * 10; //MOVE_COST ist konstant
    }

    private Tile getLowestFCostNode(List<Tile> nodeList)
    {
        Tile lowestFCostNode = nodeList[0];
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].FCost < lowestFCostNode.FCost)
            {
                lowestFCostNode = nodeList[i];
            }
        }

        return lowestFCostNode;
    }

    private List<Tile> calculatePath(Tile end)
    {
        List<Tile> path = new List<Tile>();
        //path.Add(end);
        Tile currNode = end;
        path.Add(end);
        while (currNode.Parent != null) //ist es wirklich immer true ? ich glaube nicht
        {
            path.Add(currNode.Parent);
            currNode = currNode.Parent;
        }

        path.Remove(currNode);
        path.Reverse();
        return path;
    }


    /// <summary>
    /// this method should be called, when a character needs to be revived 
    /// //TODO: Diese Methode testen. Außerdem: geht das vlt effizienter?
    /// </summary>
    public void respawnAllDeadCharakters()
    {
        List<StartField> notOccupiedStartFields = new List<StartField>();
        List<Charakter> allDeadCharakters = new List<Charakter>();

        for (int i = 0; i < players.Count; i++)
        {
            if (!isRespawnPointOccupied(players[i].Charakter))
            {
                notOccupiedStartFields.Add(players[i].Charakter.startField);
            }
        }


        for (int m = 0; m < players.Count; m++)
        {
            if (players[m].Charakter.deadRoundsLeft < 0) continue;
            if (players[m].Charakter.deadRoundsLeft != 0)
            {
                players[m].Charakter.deadRoundsLeft--;
                continue;
            }

            if (players[m].Charakter.IsDead)
            {
                Charakter charakter = players[m].Charakter;
                allDeadCharakters.Add(charakter);

                bool respawnPointOccupied = isRespawnPointOccupied(charakter);

                if (!respawnPointOccupied)
                {
                    if (notOccupiedStartFields.Contains(charakter
                            .startField)) //check if the respawn point is in "notOccupiedStartFields" and remove it and it's character
                    {
                        //this 2. if case should be in every iteration true
                        charakter.respawn(charakter.startField);
                        notOccupiedStartFields.Remove(charakter.startField);
                        allDeadCharakters.Remove(charakter);
                    }
                }
            }
        }

        int numberOfAllDeadCharakter = allDeadCharakters.Count;
        for (int i = 0; i < numberOfAllDeadCharakter; i++) //assign every dead charakter to a not occupied respawn point
        {
            Random rng = new Random();
            int randomNr = rng.Next(notOccupiedStartFields.Count);
            Console.WriteLine($"{randomNr}  {notOccupiedStartFields.Count}");

            allDeadCharakters[0].respawn(notOccupiedStartFields[randomNr]);

            allDeadCharakters.RemoveAt(0);
            notOccupiedStartFields.RemoveAt(randomNr);
        }
    }

    /// <summary>
    /// this is a helper method for the method respawnAllDeadCharakters(). It returns "false" when the respawn Point is
    /// not occupied and returns "true" when it is occupied.
    /// </summary>
    /// <param name="charakter"></param>
    /// <returns></returns>
    private bool isRespawnPointOccupied(Charakter charakter)
    {
        for (int i = 0; i < players.Count; i++) //test if respawn point is occupied from an other player
        {
            Charakter charakterToTest = players[i].Charakter;


            if (!charakterToTest.Equals(charakter) && !charakterToTest.IsDead)
            {
                if (charakterToTest.X.Equals(charakter.startField.X) &&
                    charakterToTest.Y.Equals(charakter.startField.Y))
                {
                    break;
                }
            }

            if (i == players.Count - 1)
            {
                return false;
            }
        }

        return true;
    }

    private void checkHoleandLembasTile(Player player)
    {
        var currentTile = gameboard.tiles[player.Charakter.X, player.Charakter.Y];

        if (currentTile is Hole)
        {
            player.Charakter.killCharakter();
        }
        else if (currentTile is LembasFieldTile lembasFieldTile)
        {
            if (lembasFieldTile.amount <= 0) return;
            player.Charakter.Lembas += 1;
            lembasFieldTile.amount -= 1;
        }
    }

    private bool CheckCheckpoints()
    {
        foreach (var player in players.Where(p => !p.Charakter.IsDead))
        {
            if (gameboard.tiles[player.Charakter.X, player.Charakter.Y] is CheckPoint checkPoint)
            {
                if (player.ReachedCheckpoints + 1 == checkPoint.order)
                {
                    player.ReachedCheckpoints++;
                    if(player.ReachedCheckpoints == gameboard.CheckPoints.Count)
                    {
                        EndGame(player);
                        return true;
                    }
                }
            }
        }

        return false;
    }
}