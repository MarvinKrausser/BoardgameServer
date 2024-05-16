using ConsoleApp1.DataContainers;
using ConsoleApp1.Gameplay;
using WebSocketSharp;

namespace ConsoleApp1;

class MainClass
{
    public static void Main(String[] args)
    {
        //läd env - erstellt jsons und übergibt diese dem Gamemanager
        string? port = Environment.GetEnvironmentVariable("PORT");
        //string port = "3018";

        string? gameConfigPath = Environment.GetEnvironmentVariable("GAME_CONFIG");
        string? boardConfigPath = Environment.GetEnvironmentVariable("BOARD_CONFIG");

        if (boardConfigPath is null)
        {
            throw new Exception("BoardConfigPath not exisitng");
        }
        else if (gameConfigPath is null)
        {
            throw new Exception("GameConfigPath not exisitng");
        }
        else if (port is null)
        {
            throw new Exception("Port not exisitng");
        }


        string gameConfigJson = JsonManager.getConfigJson(gameConfigPath);
        string boardConfigJson = JsonManager.getConfigJson(boardConfigPath);

        //string gameConfigJson = JsonManager.getConfigJson(@"..\\..\\..\\AdditionalExamples\\configs\\gameconfig.json");
        //string boardConfigJson =
            //JsonManager.getConfigJson(@"..\\..\\..\\AdditionalExamples\\configs\\boardconfig.json");
        
        
        if (!JsonManager.ChooseDirectory(boardConfigJson, gameConfigJson))
        {
            throw new Exception("Config not Valid");
        }

        BoardConfig boardConfig = JsonManager.DeserializeJson<BoardConfig>(boardConfigJson);
        GameConfig gameConfig = JsonManager.DeserializeJson<GameConfig>(gameConfigJson);

        while (true)
        {
            var gameManager = new GameManager(boardConfig, gameConfig);
            gameManager.startGame(Int32.Parse(port));
        }
    }
}