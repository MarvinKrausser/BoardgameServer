using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ConsoleApp1.DataContainers;

[JsonConverter(typeof(StringEnumConverter))]
public enum cardEnum
{
    MOVE_3,
    MOVE_2,
    MOVE_1,
    MOVE_BACK,
    U_TURN,
    RIGHT_TURN,
    LEFT_TURN,
    AGAIN,
    LEMBAS,
    EMPTY
}

[JsonConverter(typeof(StringEnumConverter))]
public enum characterEnum
{
    FRODO,
    SAM,
    LEGOLAS,
    GIMLI,
    GANDALF,
    ARAGORN,
    GOLLUM,
    GALADRIEL,
    BOROMIR,
    BAUMBART,
    MERRY,
    PIPPIN,
    ARWEN
}

[JsonConverter(typeof(StringEnumConverter))]
public enum directionEnum
{
    NORTH,
    SOUTH,
    WEST,
    EAST
}

[JsonConverter(typeof(StringEnumConverter))]
public enum roleEnum
{
    PLAYER,
    SPECTATOR,
    AI
}

[JsonConverter(typeof(StringEnumConverter))]
public enum messageEnum
{
    ERROR,
    GOODBYE_SERVER,
    INVALID_MESSAGE,
    PARTICIPANTS_INFO,
    CARD_CHOICE,
    CARD_EVENT,
    CARD_OFFER,
    GAME_END,
    GAME_STATE,
    PAUSE_REQUEST,
    PAUSED,
    RIVER_EVENT,
    ROUND_START,
    SHOT_EVENT,
    CHARACTER_CHOICE,
    CHARACTER_OFFER,
    GAME_START,
    HELLO_CLIENT,
    HELLO_SERVER,
    PLAYER_READY,
    RECONNECT,
    EAGLE_EVENT
}