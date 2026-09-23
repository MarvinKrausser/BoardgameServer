# Battle of the Centerländ – Game Server

Game server for *Battle of the Centerländ*, a multiplayer board game themed on The Lord of the Rings. Built by Team 23 as a university group project (Softwaregrundprojekt, Universität Ulm, WiSe 2022/23 – SoSe 2023).

Players pick a character (Frodo, Gandalf, Legolas, …) and program their moves with cards to reach all checkpoints on the board before everyone else. Along the way they deal with rivers, holes, walls, eagles, the Eye and each other.

## Features

- WebSocket server that handles players, spectators and AI clients
- JSON message protocol, with every message checked against a JSON schema
- Board and game rules loaded from config files
- Reconnecting with a token, pausing the game, and timeouts that kick or ban inactive or misbehaving clients
- Starts a new game automatically after each game ends

## Tech Stack

- C# / .NET 7
- [websocket-sharp](https://github.com/sta/websocket-sharp) for networking
- Newtonsoft.Json and Newtonsoft.Json.Schema for messages
- NUnit for tests
- DocFX for API documentation

## Getting Started

### Docker Compose

```bash
docker compose up
```

This mounts `Config/` and starts the server on port `3018`.

### Locally

Requires the .NET 7 SDK.

```bash
export PORT=3018
export BOARD_CONFIG=Config/board.json
export GAME_CONFIG=Config/game.json
dotnet run --project ConsoleApp1/ConsoleApp1/ConsoleApp1.csproj
```

All three environment variables must be set, or the server won't start. Clients connect to `ws://<host>:<PORT>/`.

## Configuration

### Board (`BOARD_CONFIG`)

Sets the board size, start fields, checkpoints, the Eye, holes, river fields (with flow direction), walls, lembas fields and eagle fields. See `Config/board.json`.

### Game (`GAME_CONFIG`)

| Key | Description |
|---|---|
| `startLembas` | Lembas each character starts with |
| `shotLembas` | Lembas it costs to shoot |
| `cardSelectionTimeout` | Time to choose cards (ms) |
| `characterChoiceTimeout` | Time to choose a character (ms) |
| `riverMoveCount` | How far rivers move characters |
| `serverIngameDelay` | Delay between game events (ms) |
| `reviveRounds` | Rounds before a dead character respawns |
| `maxRounds` | Round limit (`0` = unlimited) |

The JSON schemas for both config files are in `ConsoleApp1/ConsoleApp1/Schemas/`.

## Game Flow

1. **Login:** the client sends `HELLO_SERVER` (name and role) and gets back `HELLO_CLIENT` (reconnect token and both configs). Players then send `PLAYER_READY`.
2. **Character selection:** `CHARACTER_OFFER` / `CHARACTER_CHOICE`, followed by `GAME_START`.
3. **Each round:**
   - **Planning:** each player gets a hand of cards (`CARD_OFFER`); how many depends on their lives. They lock in 5 of them (`CARD_CHOICE`).
   - **Moves:** 5 turns, one card each, played in turn order (`CARD_EVENT`).
   - **Actions:** shooting, river movement, eagles and checkpoint checks (`SHOT_EVENT`, `RIVER_EVENT`, `EAGLE_EVENT`, `GAME_STATE`).
4. **End:** `GAME_END` announces the winner. A player wins by reaching all checkpoints first or by being the last one left. When `maxRounds` runs out, the player with the most checkpoints wins, and ties go to whoever is closest to the next checkpoint.

Example messages for every type are in `ConsoleApp1/ConsoleApp1/AdditionalExamples/`.

## Project Structure

```
Config/                     Default board and game configs
ConsoleApp1/ConsoleApp1/
├── MainClass.cs            Entry point (reads env vars, validates configs)
├── Server.cs               WebSocket server
├── ClientManager.cs        Client sessions, lobby, message handling
├── JsonManager.cs          (De)serialization and schema validation
├── DataContainers/         Protocol message classes
├── Gameplay/               Game logic (GameManager, Gameboard, Tiles, Cards)
├── Schemas/                JSON schemas
├── Tests/                  NUnit tests
└── AdditionalExamples/     Example messages and configs
Documentation/              DocFX sources
```

## Tests

```bash
dotnet test ConsoleApp1/ConsoleApp1.sln
```

## Documentation

- [User manual](Team23_Benutzerhandbuch_BoC.pdf) (German)
- [Developer manual](Team23_Entwicklerhandbuch_BoC.pdf) (German)
- API docs: `docfx docfx.json --serve`

## Authors

Team 23: Johannes Martin Ertle, Jonathan Torben Hielscher, Marvin Kraußer, Samed Bilgili, Sündüz Can, Yannik Bleilinger
