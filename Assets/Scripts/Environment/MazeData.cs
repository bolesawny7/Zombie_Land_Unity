namespace ZombieLand.Environment
{
    /// <summary>
    /// Hard-coded ASCII map. Strings are read top-to-bottom; the first
    /// row corresponds to the highest Z value in the world.
    /// Legend:
    ///   '#' wall
    ///   '.' floor
    ///   'P' player spawn (one)
    ///   'E' exit portal  (one)
    ///   'F' memory fragment spawn
    ///   'Z' walker zombie spawn (slow shambler)
    ///   'R' runner zombie spawn (fast, lean, glowing red eyes)
    ///   'B' brute zombie spawn  (large, slow, heavy)
    /// All walls are isolated 2x2 pillars surrounded by floor, so the
    /// arena is always one fully connected walkable region.
    /// </summary>
    public static class MazeData
    {
        public static readonly string[] Layout =
        {
            "#################",
            "#P..............#",
            "#...##.....##...#",
            "#...##..R..##...#",
            "#.......F.......#",
            "#...............#",
            "#...##.....##...#",
            "#.F.##..Z..##...#",
            "#...##.....##...#",
            "#......R........#",
            "#...##.....##...#",
            "#...##..F..##...#",
            "#...##.....##...#",
            "#.....B.....F...#",
            "#...............#",
            "#...##.....##...#",
            "#.F.##..Z..##...#",
            "#......E........#",
            "#################",
        };
    }
}
