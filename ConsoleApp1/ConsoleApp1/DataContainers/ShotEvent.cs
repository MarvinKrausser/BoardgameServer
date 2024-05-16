namespace ConsoleApp1.DataContainers;

public struct ShotEvent
{
    public messageEnum message;
    public Data data;

    public ShotEvent(Data data)
    {
        this.data = data;
        message = messageEnum.SHOT_EVENT;
    }

    public struct Data
    {
        public string shooterName;
        public string targetName;
        public PlayerState[] playerStates;

        public Data(string shooterName, string targetName, PlayerState[] playerStates)
        {
            this.shooterName = shooterName;
            this.targetName = targetName;
            this.playerStates = playerStates;
        }
    }
}