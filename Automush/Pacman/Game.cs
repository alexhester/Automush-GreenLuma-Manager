using System.Windows.Input;

namespace Automush.Pacman;

internal class Game
{
    internal Block[,] Map { get; set; }
    internal int sizeX = 29;
    internal int sizeY = 28;
    internal bool active = true;
    internal Pair PacmanLocation;
    internal int pointsRemaining;
    internal Entity[] player;

    internal Game()
    {
        Map = new Block[sizeX, sizeY];
        player = new Entity[4];

        pointsRemaining = 0;
        for (int i = 0; i < sizeX; i++)
        {
            string line = Chart.chart[i];
            for (int j = 0; j < sizeY; j++)
            {
                Map[i, j] = new()
                {
                    Entity = null
                };

                if (line[j] == '.')
                {
                    Map[i, j].Type = BlockType.Point;
                    pointsRemaining++;
                }
                else if (line[j] == '*')
                {
                    Map[i, j].Type = BlockType.Wall;
                }
                else
                    Map[i, j].Type = BlockType.Empty;
            }
        }
        player[0] = new Pacman(this, 23, 13);
        Map[23, 13].Entity = player[0];
        PacmanLocation = new Pair(23, 13);

        player[1] = new Ghost(this, 5, 5);
        Map[5, 5].Entity = player[1];

        player[2] = new Ghost(this, 5, 20);
        Map[5, 20].Entity = player[2];

        player[3] = new Ghost(this, 8, 5);
        Map[8, 5].Entity = player[3];
    }
    internal void KeyPressed(KeyEventArgs keyEvent)
    {
        Console.WriteLine("Key pressed: " + keyEvent.Key);

        switch (keyEvent.Key)
        {
            case System.Windows.Input.Key.Up:
                player[0].direction = Direction.Up;
                break;
            case System.Windows.Input.Key.Down:
                player[0].direction = Direction.Down;
                break;
            case System.Windows.Input.Key.Left:
                player[0].direction = Direction.Left;
                break;
            case System.Windows.Input.Key.Right:
                player[0].direction = Direction.Right;
                break;
            default:
                break;
        }
    }

    internal void PlayRound()
    {
        for (int i = 0; i < 4; i++)
            player[i].Play();
    }
}

internal class Chart
{
    internal static string[] chart =
    [
        "****************************",
        "*............**............*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*..........................*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*......**....**....**......*",
        "******.***** ** *****.******",
        "******.***** ** *****.******",
        "******.**    **    **.******",
        "******.** ******** **.******",
        "******.** ******** **.******",
        "*     .   ********   .     *",
        "******.** ******** **.******",
        "******.** ******** **.******",
        "******.**    **    **.******",
        "******.***** ** *****.******",
        "******.***** ** *****.******",
        "*......**....**....**......*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*..........................*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*.****.*****.**.*****.****.*",
        "*............**............*",
        "****************************",
    ];
}
