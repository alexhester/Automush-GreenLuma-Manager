namespace Automush.Pacman;

internal class Pacman : Entity
{
    internal Pacman(Game map, int x, int y)
    {
        game = map;
        type = EntityType.Pacman;
        this.x = x;
        this.y = y;
    }

    internal override void Play()
    {
        List<Pair> candidateBlocks = [];

        for(int i = x - 1; i <= x + 1; i++)
            for(int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i < game.sizeX && j >=0 && j < game.sizeY && !(i == x && j == y) && game.Map[i, j].Type != BlockType.Wall)
                    candidateBlocks.Add(new Pair(i, j));
            }
        switch (direction)
        {
            case Direction.Up:
                if (candidateBlocks.Find(p => p.X == x - 1 && p.Y == y) != null)
                {
                    if (game.Map[x - 1, y].Entity != null)
                        game.active = false;
                    Move(x - 1, y);
                }
                break;
            case Direction.Down:
                if (candidateBlocks.Find(p => p.X == x + 1 && p.Y == y) != null)
                {
                    if (game.Map[x + 1, y].Entity != null)
                        game.active = false;
                    Move(x + 1, y);
                }
                break;
            case Direction.Left:
                if (candidateBlocks.Find(p => p.X == x && p.Y == y - 1) != null)
                {
                    if (game.Map[x, y - 1].Entity != null)
                        game.active = false;
                    Move(x, y - 1);
                }
                break;
            case Direction.Right:
                if (candidateBlocks.Find(p => p.X == x && p.Y == y + 1) != null)
                {
                    if (game.Map[x, y + 1].Entity != null)
                        game.active = false;
                    Move(x, y + 1);
                }
                break;
        }

        game.PacmanLocation.X = x;
        game.PacmanLocation.Y = y;

        if (game.Map[x, y].Type == BlockType.Point)
        {
            game.Map[x, y].Type = BlockType.Empty;
            game.pointsRemaining--;
            if (game.pointsRemaining == 0)
                game.active = false;
        }
    }
}
