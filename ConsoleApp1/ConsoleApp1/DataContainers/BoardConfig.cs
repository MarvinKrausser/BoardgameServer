namespace ConsoleApp1.DataContainers;

public struct BoardConfig
{
    public string name;
    public int height;
    public int width;

    public StartField[] startFields;
    public int[][] checkPoints;
    public Eye eye;
    public int[][] holes;
    public RiverField[] riverFields;
    public int[][][] walls;
    public LembasField[] lembasFields;
    public int[][] eagleFields;


    public struct StartField
    {
        public StartField(int[] position, directionEnum direction)
        {
            this.position = position;
            this.direction = direction;
        }

        public int[] position;
        public directionEnum direction;
    }

    public struct Eye
    {
        public int[] position;
        public directionEnum direction;

        public Eye(int[] position, directionEnum direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }

    public struct RiverField
    {
        public int[] position;
        public directionEnum direction;

        public RiverField(int[] position, directionEnum direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }
}