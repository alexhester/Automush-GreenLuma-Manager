namespace Automush.Pacman;

internal enum EntityType
{
    Pacman,
    Ghost
}

internal enum Direction
{
    Up,
    Down,
    Left,
    Right
}

internal abstract class Entity
{
    internal int x, y;
    internal Direction direction;
    internal EntityType type;
    internal Game game;

    internal void Move(int newX, int newY)
    {
        game.Map[newX, newY].Entity = game.Map[x, y].Entity;
        game.Map[x, y].Entity = null;
        x = newX;
        y = newY;
    }

    internal abstract void Play();
}
