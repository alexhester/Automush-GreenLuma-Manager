namespace Automush.Pacman;

internal enum BlockType
{
    Wall,
    Point,
    Empty
}

internal class Block
{
    internal BlockType Type { get; set; }
    internal Entity? Entity { get; set; }
}
