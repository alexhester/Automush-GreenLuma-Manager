namespace Automush.Pacman;

internal class Ghost : Entity
{
    internal Ghost(Game map, int x, int y)
    {
        game = map;
        type = EntityType.Ghost;
        this.x = x;
        this.y = y;
    }

    internal override void Play()
    {
        List<Pair> candidateBlocks = [];
        List<double> distanceToPacman = [];

        for (int i = x - 1; i <= x + 1; i++)
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i < game.sizeX && j >= 0 && j < game.sizeY && !(i == x && j == y) && game.Map[i, j].Type != BlockType.Wall)
                {
                    candidateBlocks.Add(new Pair(i, j));
                    distanceToPacman.Add(Math.Sqrt(Math.Pow(i - game.PacmanLocation.X, 2) + Math.Pow(j - game.PacmanLocation.Y, 2)));
                }
            }
        int minDist = distanceToPacman.IndexOf(distanceToPacman.Min());

        if (game.Map[candidateBlocks.ElementAt(minDist).X, candidateBlocks.ElementAt(minDist).Y].Entity != null)
        {
            if (game.Map[candidateBlocks.ElementAt(minDist).X, candidateBlocks.ElementAt(minDist).Y].Entity!.type == EntityType.Pacman) // Move if pacman is there
            {
                game.Map[candidateBlocks.ElementAt(minDist).X, candidateBlocks.ElementAt(minDist).Y].Entity = null;
                game.active = false;
                Move(candidateBlocks.ElementAt(minDist).X, candidateBlocks.ElementAt(minDist).Y);
            }
        }
        else
            Move(candidateBlocks.ElementAt(minDist).X, candidateBlocks.ElementAt(minDist).Y);
    }
}
