using ConsoleApp1.DataContainers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ConsoleApp1
{
    /// <summary>
    /// Die Klasse ist für das Serialisieren und Deserialisieren der Json Dateien zuständig
    /// </summary>
    class JsonManager
    {
        private static readonly Dictionary<messageEnum, string> _schemas = new ();
        /// <summary>
        /// Überprüft ob ein übergebenes Json mit bestimmtem Typ valide ist
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValid(string json, messageEnum? type)
        {
            if (type is null) return false;
            JObject jObject = JObject.Parse(json);
            string schemaJason = _schemas[(messageEnum)type];
                
            //Parst das Schema - Rückgabe ob geglückt oder nciht
            JSchema schema = JSchema.Parse(schemaJason);
            if (jObject.IsValid(schema)) return true;
            Console.WriteLine("Not valid");
            return false;
        }
        /// <summary>
        /// Gibt den Typ einer Json Datei zurück
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static messageEnum? GetTypeJson(string json)
        {
            //Parse Objekt - rückgabe falls geglückt
            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            if (jObject.First is null) return null;
            string? type = jObject.First.ToObject<string>();
            if (type is null) return null;
            if (Enum.IsDefined(typeof(messageEnum), type)) return Enum.Parse<messageEnum>(type);

            return null;
        }

        public static T DeserializeJson<T>(string json)
        {
            T o = JsonConvert.DeserializeObject<T>(json);
            return o;
        }

        public static string ConvertToJason<T>(T ob)
        {
            return JsonConvert.SerializeObject(ob);
        }
        /// <summary>
        /// Läd die Konfigurationsdateien die dem Spiel übergeben werden. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string getConfigJson(string name)
        {
            //name should be board or game
            //default /var/lib/app/config/board.json(game.json)

            //Ursprünglicher Pfad: E:\\Projekte\\JsonManager\\Examples\\" + name + "config.json"
            string input;
            using (StreamReader
                   sr = new StreamReader(name)) //pfad geht von .exe aus unter bin/Debug/.net7.0/GameServer.exe
            {
                input = sr.ReadToEnd();
            }

            return input;
        }
        
        public static bool ChooseDirectory(string boardConfig, string gameConfig)
        {
            string path1 = Directory.GetCurrentDirectory() + "\\..\\..\\..\\Schemas\\error.schema.json"; //Local Project
            string path2 = "/src/ConsoleApp1/Schemas/error.schema.json";  //Docker File
            string path3 = "/ConsoleApp1/Schemas/error.schema.json";  //Docker Compose
            string path4 = String.Empty;

            if (File.Exists(path1)) path4 = @"..\\..\\..\\Schemas\\";
            else if (File.Exists(path2)) path4 = "/src/ConsoleApp1/Schemas/";
            else if (File.Exists(path3)) path4 = "/ConsoleApp1/Schemas/";


            foreach (var m in Enum.GetValues<messageEnum>())
            {
                using (StreamReader sr =
                       new StreamReader(path4 + m.ToString().ToLower().Replace("_", String.Empty) + ".schema.json"))
                {
                    string schemaJason = sr.ReadToEnd();
                    _schemas.Add(m, schemaJason);
                }
            }
            
            using (StreamReader sr = new StreamReader(path4 + "boardconfig.schema.json"))
            {
                JObject jObject = JObject.Parse(boardConfig); 
                string schemaJason = sr.ReadToEnd();
                JSchema schema = JSchema.Parse(schemaJason);
                if (!jObject.IsValid(schema)) return false;
            }
            
            using (StreamReader sr = new StreamReader(path4 + "gameconfig.schema.json"))
            {
                JObject jObject = JObject.Parse(gameConfig); 
                string schemaJason = sr.ReadToEnd();
                JSchema schema = JSchema.Parse(schemaJason);
                if (!jObject.IsValid(schema)) return false;
            }

            return true;
        }
    }
}