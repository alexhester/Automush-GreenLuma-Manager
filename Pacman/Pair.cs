namespace Automush.Pacman;

internal class Pair
{
    internal int X { get; set; }
    internal int Y { get; set; }

    internal Pair(int x, int y)
    {
        X = x;
        Y = y;
    }

    internal Pair()
    {
        X = 0;
        Y = 0;
    }
}
